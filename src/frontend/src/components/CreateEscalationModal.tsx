import { useEffect, useState } from 'react'
import { useAuth } from '../context/AuthContext'
import {
  CreateEscalationDto,
  EscalationPriority, EscalationType,
  PRIORITY_LABELS, TYPE_LABELS,
  SlaPolicy, getSlaPolicies, createEscalation,
} from '../api/escalations'
import { UserProfile, getUsers, WORK_TYPE_LABELS } from '../api/users'
import { ApiError } from '../api/auth'

interface Props {
  projectId: number
  onClose: () => void
  onCreated: () => void
}

const PRIORITIES: EscalationPriority[] = ['low', 'medium', 'high', 'critical', 'blocker']
const TYPES: EscalationType[] = ['incident', 'service_request', 'bug', 'feature_request']

const PARTICIPANT_ROLE_LABELS: Record<string, string> = {
  assignee: 'Исполнитель',
  observer: 'Наблюдатель',
}

export default function CreateEscalationModal({ projectId, onClose, onCreated }: Props) {
  const { user } = useAuth()

  const [form, setForm] = useState({
    name: '',
    description: '',
    priority: 'medium' as EscalationPriority,
    type: 'incident' as EscalationType,
    slaPolicyId: 0,
  })

  const [slaPolicies, setSlaPolicies] = useState<SlaPolicy[]>([])
  const [users, setUsers] = useState<UserProfile[]>([])
  const [selectedParticipants, setSelectedParticipants] = useState<
    Array<{ user: UserProfile; role: 'assignee' | 'observer' }>
  >([])
  const [userSearch, setUserSearch] = useState('')
  const [showUserDropdown, setShowUserDropdown] = useState(false)

  const [criteria, setCriteria] = useState<Array<{ name: string; description: string }>>([])
  const [newCriterionName, setNewCriterionName] = useState('')
  const [newCriterionDesc, setNewCriterionDesc] = useState('')

  const [loading, setLoading] = useState(false)
  const [initLoading, setInitLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let done = 0
    const finish = () => { if (++done === 2) setInitLoading(false) }

    getSlaPolicies()
      .then(policies => {
        const active = policies.filter(p => p.isActive)
        setSlaPolicies(active)
        if (active.length > 0) setForm(f => ({ ...f, slaPolicyId: active[0].id }))
      })
      .catch(err => {
        console.error('[CreateEscalationModal] getSlaPolicies failed:', err)
        setError(prev => prev ? prev : 'Не удалось загрузить SLA-политики')
      })
      .finally(finish)

    getUsers()
      .then(page => setUsers(page.items))
      .catch(err => {
        console.error('[CreateEscalationModal] getUsers failed:', err)
        setError(prev => prev ? prev : 'Не удалось загрузить список пользователей')
      })
      .finally(finish)
  }, [])

  const filteredUsers = users.filter(u => {
    const q = userSearch.toLowerCase()
    const full = `${u.firstName} ${u.lastName}`.toLowerCase()
    return full.includes(q) && !selectedParticipants.some(p => p.user.id === u.id)
  })

  function addParticipant(u: UserProfile) {
    setSelectedParticipants(prev => [...prev, { user: u, role: 'assignee' }])
    setUserSearch('')
    setShowUserDropdown(false)
  }

  function removeParticipant(userId: string) {
    setSelectedParticipants(prev => prev.filter(p => p.user.id !== userId))
  }

  function changeRole(userId: string, role: 'assignee' | 'observer') {
    setSelectedParticipants(prev =>
      prev.map(p => p.user.id === userId ? { ...p, role } : p)
    )
  }

  function addCriterion() {
    const trimmed = newCriterionName.trim()
    if (!trimmed) return
    setCriteria(prev => [...prev, { name: trimmed, description: newCriterionDesc.trim() }])
    setNewCriterionName('')
    setNewCriterionDesc('')
  }

  function removeCriterion(idx: number) {
    setCriteria(prev => prev.filter((_, i) => i !== idx))
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!form.name.trim()) { setError('Введите название эскалации'); return }
    if (form.slaPolicyId === 0) { setError('Выберите SLA-политику'); return }
    if (!user) { setError('Пользователь не авторизован'); return }

    setLoading(true)
    setError(null)

    // Автоматически добавляем текст из поля ввода, если пользователь не нажал +
    const finalCriteria = newCriterionName.trim()
      ? [...criteria, { name: newCriterionName.trim(), description: newCriterionDesc.trim() }]
      : criteria

    const dto: CreateEscalationDto = {
      name: form.name.trim(),
      description: form.description.trim() || undefined,
      priority: form.priority,
      type: form.type,
      authorId: user.id,
      slaPolicyId: form.slaPolicyId,
      projectId,
      participants: selectedParticipants.map(p => ({ userId: p.user.id, role: p.role })),
      criteria: finalCriteria.map(c => ({ name: c.name, description: c.description || undefined })),
    }

    try {
      await createEscalation(dto)
      onCreated()
    } catch (err) {
      const apiErr = err as ApiError
      setError(apiErr.detail ?? 'Не удалось создать эскалацию')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      {/* Backdrop */}
      <div className="absolute inset-0 bg-black/40 backdrop-blur-sm" onClick={onClose} />

      {/* Modal */}
      <div className="relative bg-white rounded-2xl shadow-2xl w-full max-w-lg max-h-[90vh] flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
          <h2 className="text-lg font-bold text-gray-900">Новая эскалация</h2>
          <button
            onClick={onClose}
            className="w-8 h-8 flex items-center justify-center rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 transition-colors"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {initLoading ? (
          <div className="flex-1 flex items-center justify-center py-16">
            <div className="w-8 h-8 border-2 border-blue-600 border-t-transparent rounded-full animate-spin" />
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="flex-1 overflow-y-auto px-6 py-5 space-y-4">

            {/* Name */}
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1.5">
                Название <span className="text-red-500">*</span>
              </label>
              <input
                type="text"
                value={form.name}
                onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
                placeholder="Кратко опишите проблему"
                className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                maxLength={200}
              />
            </div>

            {/* Description */}
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1.5">Описание</label>
              <textarea
                value={form.description}
                onChange={e => setForm(f => ({ ...f, description: e.target.value }))}
                placeholder="Подробности, шаги воспроизведения, ссылки..."
                rows={3}
                className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none"
              />
            </div>

            {/* Type + Priority */}
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1.5">Тип</label>
                <select
                  value={form.type}
                  onChange={e => setForm(f => ({ ...f, type: e.target.value as EscalationType }))}
                  className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                >
                  {TYPES.map(t => (
                    <option key={t} value={t}>{TYPE_LABELS[t]}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1.5">Приоритет</label>
                <select
                  value={form.priority}
                  onChange={e => setForm(f => ({ ...f, priority: e.target.value as EscalationPriority }))}
                  className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                >
                  {PRIORITIES.map(p => (
                    <option key={p} value={p}>{PRIORITY_LABELS[p]}</option>
                  ))}
                </select>
              </div>
            </div>

            {/* SLA Policy */}
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1.5">
                SLA-политика <span className="text-red-500">*</span>
              </label>
              {slaPolicies.length === 0 ? (
                <p className="text-xs text-gray-400 py-2">Нет доступных SLA-политик</p>
              ) : (
                <select
                  value={form.slaPolicyId}
                  onChange={e => setForm(f => ({ ...f, slaPolicyId: Number(e.target.value) }))}
                  className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                >
                  {slaPolicies.map(p => (
                    <option key={p.id} value={p.id}>
                      {p.name} (отв. {p.responseTimeMinutes} мин)
                    </option>
                  ))}
                </select>
              )}
            </div>

            {/* Criteria */}
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1.5">Критерии выполнения</label>

              {criteria.length > 0 && (
                <div className="mb-2 space-y-1.5">
                  {criteria.map((c, idx) => (
                    <div key={idx} className="flex items-start gap-2 bg-gray-50 border border-gray-100 rounded-lg px-3 py-2">
                      <div className="w-4 h-4 rounded border-2 border-gray-300 shrink-0 mt-0.5" />
                      <div className="flex-1 min-w-0">
                        <p className="text-xs text-gray-700 font-medium">{c.name}</p>
                        {c.description && (
                          <p className="text-xs text-gray-400 mt-0.5 truncate">{c.description}</p>
                        )}
                      </div>
                      <button
                        type="button"
                        onClick={() => removeCriterion(idx)}
                        className="w-4 h-4 flex items-center justify-center text-gray-300 hover:text-red-500 transition-colors shrink-0 mt-0.5"
                      >
                        <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                      </button>
                    </div>
                  ))}
                </div>
              )}

              <div className="space-y-1.5">
                <div className="flex items-center gap-2">
                  <input
                    type="text"
                    value={newCriterionName}
                    onChange={e => setNewCriterionName(e.target.value)}
                    onKeyDown={e => { if (e.key === 'Enter') { e.preventDefault(); addCriterion() } }}
                    placeholder="Добавить критерий…"
                    maxLength={300}
                    className="flex-1 px-3 py-2 text-sm border border-dashed border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-400"
                  />
                  <button
                    type="button"
                    onClick={addCriterion}
                    disabled={!newCriterionName.trim()}
                    className="w-8 h-8 flex items-center justify-center rounded-lg bg-blue-600 text-white hover:bg-blue-700 disabled:opacity-40 transition-colors shrink-0"
                  >
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                    </svg>
                  </button>
                </div>
                {newCriterionName.trim() && (
                  <textarea
                    value={newCriterionDesc}
                    onChange={e => setNewCriterionDesc(e.target.value)}
                    placeholder="Описание (необязательно)…"
                    maxLength={500}
                    rows={2}
                    className="w-full px-3 py-2 text-sm border border-dashed border-gray-200 rounded-lg focus:outline-none focus:ring-1 focus:ring-blue-400 text-gray-600 resize-none"
                  />
                )}
              </div>
            </div>

            {/* Participants */}
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1.5">Участники</label>

              {/* Selected participants */}
              {selectedParticipants.length > 0 && (
                <div className="mb-2 space-y-1.5">
                  {selectedParticipants.map(({ user: u, role }) => (
                    <div key={u.id} className="flex items-center gap-2 bg-blue-50 rounded-lg px-3 py-2">
                      <div className="w-7 h-7 rounded-full bg-blue-600 flex items-center justify-center text-white text-xs font-bold shrink-0">
                        {u.firstName[0]}{u.lastName[0]}
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="text-xs font-medium text-gray-800 truncate">
                          {u.firstName} {u.lastName}
                        </p>
                        <p className="text-xs text-gray-400">{WORK_TYPE_LABELS[u.workType] ?? '—'}</p>
                      </div>
                      <select
                        value={role}
                        onChange={e => changeRole(u.id, e.target.value as 'assignee' | 'observer')}
                        className="text-xs border border-blue-200 rounded-md px-1.5 py-1 bg-white focus:outline-none focus:ring-1 focus:ring-blue-400"
                      >
                        <option value="assignee">{PARTICIPANT_ROLE_LABELS.assignee}</option>
                        <option value="observer">{PARTICIPANT_ROLE_LABELS.observer}</option>
                      </select>
                      <button
                        type="button"
                        onClick={() => removeParticipant(u.id)}
                        className="w-5 h-5 flex items-center justify-center text-gray-400 hover:text-red-500 transition-colors shrink-0"
                      >
                        <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                      </button>
                    </div>
                  ))}
                </div>
              )}

              {/* User search */}
              <div className="relative">
                <input
                  type="text"
                  value={userSearch}
                  onChange={e => { setUserSearch(e.target.value); setShowUserDropdown(true) }}
                  onFocus={() => setShowUserDropdown(true)}
                  placeholder="Найти пользователя..."
                  className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
                {showUserDropdown && filteredUsers.length > 0 && (
                  <div className="absolute z-10 top-full left-0 right-0 mt-1 bg-white border border-gray-200 rounded-lg shadow-lg max-h-44 overflow-y-auto">
                    {filteredUsers.slice(0, 8).map(u => (
                      <button
                        key={u.id}
                        type="button"
                        onClick={() => addParticipant(u)}
                        className="w-full flex items-center gap-2.5 px-3 py-2.5 hover:bg-gray-50 transition-colors text-left"
                      >
                        <div className="w-7 h-7 rounded-full bg-blue-100 flex items-center justify-center text-blue-600 text-xs font-bold shrink-0">
                          {u.firstName[0]}{u.lastName[0]}
                        </div>
                        <div>
                          <p className="text-sm font-medium text-gray-800">{u.firstName} {u.lastName}</p>
                          <p className="text-xs text-gray-400">{WORK_TYPE_LABELS[u.workType] ?? '—'}</p>
                        </div>
                      </button>
                    ))}
                  </div>
                )}
              </div>
            </div>

            {/* Error */}
            {error && (
              <div className="bg-red-50 border border-red-200 rounded-lg px-3 py-2.5">
                <p className="text-xs text-red-600">{error}</p>
              </div>
            )}
          </form>
        )}

        {/* Footer */}
        <div className="flex items-center justify-end gap-2 px-6 py-4 border-t border-gray-100">
          <button
            type="button"
            onClick={onClose}
            className="px-4 py-2 text-sm font-medium text-gray-600 hover:text-gray-800 hover:bg-gray-100 rounded-lg transition-colors"
          >
            Отмена
          </button>
          <button
            type="button"
            disabled={loading || initLoading}
            onClick={handleSubmit}
            className="px-4 py-2 text-sm font-semibold text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed rounded-lg transition-colors flex items-center gap-2"
          >
            {loading && (
              <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
            )}
            Создать
          </button>
        </div>
      </div>
    </div>
  )
}
