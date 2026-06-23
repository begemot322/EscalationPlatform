import { useState } from 'react'
import { useAuth } from '../context/AuthContext'
import {
  EscalationDetail,
  EscalationPriority, EscalationStatus, EscalationType,
  CriterionUpdateItem,
  PRIORITY_LABELS, STATUS_LABELS, TYPE_LABELS,
  updateEscalation,
} from '../api/escalations'
import { UserProfile, getUsers, WORK_TYPE_LABELS } from '../api/users'
import { ApiError } from '../api/auth'

interface Props {
  escalation: EscalationDetail
  onClose: () => void
  onSaved: () => void
}

const PRIORITIES: EscalationPriority[] = ['low', 'medium', 'high', 'critical', 'blocker']
const STATUSES: EscalationStatus[] = ['new', 'in_progress', 'waiting_for_response', 'resolved', 'closed', 'cancelled']
const TYPES: EscalationType[] = ['incident', 'service_request', 'bug', 'feature_request']

const PARTICIPANT_ROLE_LABELS: Record<string, string> = {
  assignee: 'Исполнитель',
  observer: 'Наблюдатель',
}

type Tab = 'info' | 'criteria' | 'participants'

export default function EditEscalationModal({ escalation, onClose, onSaved }: Props) {
  const { user } = useAuth()

  const [tab, setTab] = useState<Tab>('info')

  // ── Основная информация ──────────────────────────────────────────────────
  const [name, setName] = useState(escalation.name)
  const [description, setDescription] = useState(escalation.description ?? '')
  const [priority, setPriority] = useState<EscalationPriority>(escalation.priority)
  const [status, setStatus] = useState<EscalationStatus>(escalation.status)
  const [type, setType] = useState<EscalationType>(escalation.type)

  // ── Критерии ────────────────────────────────────────────────────────────
  const [criteria, setCriteria] = useState<CriterionUpdateItem[]>(
    escalation.criteria.map(c => ({ name: c.name, description: c.description, isCompleted: c.isCompleted })),
  )
  const [newCriterionName, setNewCriterionName] = useState('')
  const [newCriterionDesc, setNewCriterionDesc] = useState('')

  // ── Участники ────────────────────────────────────────────────────────────
  const [participants, setParticipants] = useState<
    Array<{ user: UserProfile; role: 'assignee' | 'observer' }>
  >(() =>
    escalation.participants
      .filter(p => p.role !== 'approver')
      .map(p => {
        // Заглушка — имя заполнится при открытии поиска
        const stub: UserProfile = {
          id: p.userId,
          firstName: p.userId.slice(0, 4),
          lastName: '…',
          email: '',
          role: 0,
          workType: 0,
          profileImageFileName: null,
          isEmailVerified: true,
          createdAt: '',
        }
        return { user: stub, role: p.role as 'assignee' | 'observer' }
      }),
  )
  const [userSearch, setUserSearch] = useState('')
  const [userList, setUserList] = useState<UserProfile[]>([])
  const [dropdownOpen, setDropdownOpen] = useState(false)
  const [usersLoaded, setUsersLoaded] = useState(false)

  async function ensureUsersLoaded() {
    if (usersLoaded) return
    const page = await getUsers(1, 200)
    setUserList(page.items)
    // Обновляем имена участников из реального списка
    setParticipants(prev =>
      prev.map(p => {
        const found = page.items.find(u => u.id === p.user.id)
        return found ? { ...p, user: found } : p
      }),
    )
    setUsersLoaded(true)
  }

  // ── Сохранение ───────────────────────────────────────────────────────────
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleSave() {
    if (!name.trim()) { setError('Введите название'); return }
    if (!user) { setError('Пользователь не авторизован'); return }

    // Автоматически фиксируем текст из поля ввода, если нажали «Сохранить» не добавив критерий
    const finalCriteria = newCriterionName.trim()
      ? [...criteria, { name: newCriterionName.trim(), description: newCriterionDesc.trim() || undefined, isCompleted: false }]
      : criteria

    setLoading(true)
    setError(null)
    try {
      await updateEscalation(escalation.id, {
        name: name.trim(),
        description: description.trim() || undefined,
        priority,
        status,
        type,
        firstResponseAt: escalation.firstResponseAt,
        resolvedAt: escalation.resolvedAt,
        participants: participants.map(p => ({ userId: p.user.id, role: p.role })),
        criteria: finalCriteria,
      })
      onSaved()
    } catch (err) {
      const apiErr = err as ApiError
      setError(apiErr.detail ?? 'Не удалось сохранить изменения')
    } finally {
      setLoading(false)
    }
  }

  // ── Criterion helpers ─────────────────────────────────────────────────────
  function addCriterion() {
    const trimmed = newCriterionName.trim()
    if (!trimmed) return
    setCriteria(prev => [...prev, { name: trimmed, description: newCriterionDesc.trim() || undefined, isCompleted: false }])
    setNewCriterionName('')
    setNewCriterionDesc('')
  }

  function toggleCriterion(idx: number) {
    setCriteria(prev => prev.map((c, i) => i === idx ? { ...c, isCompleted: !c.isCompleted } : c))
  }

  function removeCriterion(idx: number) {
    setCriteria(prev => prev.filter((_, i) => i !== idx))
  }

  function renameCriterion(idx: number, val: string) {
    setCriteria(prev => prev.map((c, i) => i === idx ? { ...c, name: val } : c))
  }

  function changeDescription(idx: number, val: string) {
    setCriteria(prev => prev.map((c, i) => i === idx ? { ...c, description: val || undefined } : c))
  }

  // ── Participant helpers ───────────────────────────────────────────────────
  const filteredUsers = userList.filter(u => {
    const q = userSearch.toLowerCase()
    return (
      `${u.firstName} ${u.lastName}`.toLowerCase().includes(q) &&
      !participants.some(p => p.user.id === u.id)
    )
  })

  function addParticipant(u: UserProfile) {
    setParticipants(prev => [...prev, { user: u, role: 'assignee' }])
    setUserSearch('')
    setDropdownOpen(false)
  }

  function removeParticipant(userId: string) {
    setParticipants(prev => prev.filter(p => p.user.id !== userId))
  }

  function changeRole(userId: string, role: 'assignee' | 'observer') {
    setParticipants(prev => prev.map(p => p.user.id === userId ? { ...p, role } : p))
  }

  const TABS: { key: Tab; label: string }[] = [
    { key: 'info', label: 'Основное' },
    { key: 'criteria', label: `Критерии (${criteria.length})` },
    { key: 'participants', label: `Участники (${participants.length})` },
  ]

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/40 backdrop-blur-sm" onClick={onClose} />

      <div className="relative bg-white rounded-2xl shadow-2xl w-full max-w-lg max-h-[92vh] flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
          <h2 className="text-lg font-bold text-gray-900">Редактировать эскалацию</h2>
          <button
            onClick={onClose}
            className="w-8 h-8 flex items-center justify-center rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 transition-colors"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Tabs */}
        <div className="flex border-b border-gray-100 px-6">
          {TABS.map(t => (
            <button
              key={t.key}
              onClick={() => {
                setTab(t.key)
                if (t.key === 'participants') ensureUsersLoaded()
              }}
              className={`py-3 mr-5 text-sm font-medium border-b-2 transition-colors ${
                tab === t.key
                  ? 'border-blue-600 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700'
              }`}
            >
              {t.label}
            </button>
          ))}
        </div>

        {/* Body */}
        <div className="flex-1 overflow-y-auto px-6 py-5">

          {/* ── Tab: Основное ── */}
          {tab === 'info' && (
            <div className="space-y-4">
              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1.5">
                  Название <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={name}
                  onChange={e => setName(e.target.value)}
                  maxLength={200}
                  className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1.5">Описание</label>
                <textarea
                  value={description}
                  onChange={e => setDescription(e.target.value)}
                  rows={3}
                  className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none"
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1.5">Статус</label>
                  <select
                    value={status}
                    onChange={e => setStatus(e.target.value as EscalationStatus)}
                    className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                  >
                    {STATUSES.map(s => (
                      <option key={s} value={s}>{STATUS_LABELS[s]}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-700 mb-1.5">Приоритет</label>
                  <select
                    value={priority}
                    onChange={e => setPriority(e.target.value as EscalationPriority)}
                    className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                  >
                    {PRIORITIES.map(p => (
                      <option key={p} value={p}>{PRIORITY_LABELS[p]}</option>
                    ))}
                  </select>
                </div>
              </div>

              <div>
                <label className="block text-xs font-medium text-gray-700 mb-1.5">Тип</label>
                <select
                  value={type}
                  onChange={e => setType(e.target.value as EscalationType)}
                  className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                >
                  {TYPES.map(t => (
                    <option key={t} value={t}>{TYPE_LABELS[t]}</option>
                  ))}
                </select>
              </div>
            </div>
          )}

          {/* ── Tab: Критерии ── */}
          {tab === 'criteria' && (
            <div className="space-y-3">
              {criteria.length === 0 && (
                <p className="text-sm text-gray-400 text-center py-4">Критериев пока нет</p>
              )}

              {criteria.map((c, idx) => (
                <div key={idx} className="group border border-gray-200 rounded-lg p-2.5 space-y-1.5">
                  <div className="flex items-center gap-2">
                    {/* Toggle */}
                    <button
                      type="button"
                      onClick={() => toggleCriterion(idx)}
                      className={`w-5 h-5 rounded shrink-0 flex items-center justify-center border-2 transition-colors ${
                        c.isCompleted
                          ? 'bg-green-500 border-green-500'
                          : 'border-gray-300 hover:border-blue-400'
                      }`}
                    >
                      {c.isCompleted && (
                        <svg className="w-3 h-3 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M5 13l4 4L19 7" />
                        </svg>
                      )}
                    </button>

                    {/* Name input */}
                    <input
                      type="text"
                      value={c.name}
                      onChange={e => renameCriterion(idx, e.target.value)}
                      maxLength={300}
                      className={`flex-1 px-3 py-1.5 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                        c.isCompleted ? 'line-through text-gray-400' : 'text-gray-800'
                      }`}
                    />

                    {/* Delete */}
                    <button
                      type="button"
                      onClick={() => removeCriterion(idx)}
                      className="w-6 h-6 flex items-center justify-center text-gray-300 hover:text-red-500 transition-colors shrink-0"
                    >
                      <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                      </svg>
                    </button>
                  </div>

                  {/* Description input */}
                  <textarea
                    value={c.description ?? ''}
                    onChange={e => changeDescription(idx, e.target.value)}
                    maxLength={500}
                    placeholder="Описание (необязательно)…"
                    rows={2}
                    className="w-full px-3 py-2 text-sm border border-gray-100 rounded-lg focus:outline-none focus:ring-1 focus:ring-blue-400 text-gray-600 placeholder:text-gray-300 resize-none"
                  />
                </div>
              ))}

              {/* Add new */}
              <div className="pt-1 space-y-1.5">
                <div className="flex items-center gap-2">
                  <input
                    type="text"
                    value={newCriterionName}
                    onChange={e => setNewCriterionName(e.target.value)}
                    onKeyDown={e => { if (e.key === 'Enter') { e.preventDefault(); addCriterion() } }}
                    placeholder="Новый критерий…"
                    maxLength={300}
                    className="flex-1 px-3 py-1.5 text-sm border border-dashed border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-400"
                  />
                  <button
                    type="button"
                    onClick={addCriterion}
                    disabled={!newCriterionName.trim()}
                    className="w-8 h-8 flex items-center justify-center rounded-lg bg-blue-600 text-white hover:bg-blue-700 disabled:opacity-40 disabled:cursor-not-allowed transition-colors shrink-0"
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
              <p className="text-xs text-gray-400">Enter — добавить критерий</p>
            </div>
          )}

          {/* ── Tab: Участники ── */}
          {tab === 'participants' && (
            <div className="space-y-3">
              {participants.map(({ user: u, role }) => (
                <div key={u.id} className="flex items-center gap-2 bg-blue-50 rounded-lg px-3 py-2">
                  <div className="w-7 h-7 rounded-full bg-blue-600 flex items-center justify-center text-white text-xs font-bold shrink-0">
                    {u.firstName[0]}{(u.lastName[0] ?? '')}
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-xs font-medium text-gray-800 truncate">
                      {u.firstName} {u.lastName}
                    </p>
                    {u.workType > 0 && (
                      <p className="text-xs text-gray-400">{WORK_TYPE_LABELS[u.workType] ?? '—'}</p>
                    )}
                  </div>
                  <select
                    value={role}
                    onChange={e => changeRole(u.id, e.target.value as 'assignee' | 'observer')}
                    className="text-xs border border-blue-200 rounded-md px-1.5 py-1 bg-white focus:outline-none"
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

              {/* Поиск */}
              <div className="relative">
                <input
                  type="text"
                  value={userSearch}
                  onChange={e => { setUserSearch(e.target.value); setDropdownOpen(true) }}
                  onFocus={() => { setDropdownOpen(true); ensureUsersLoaded() }}
                  placeholder="Найти пользователя…"
                  className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                {dropdownOpen && filteredUsers.length > 0 && (
                  <div className="absolute z-10 top-full left-0 right-0 mt-1 bg-white border border-gray-200 rounded-lg shadow-lg max-h-44 overflow-y-auto">
                    {filteredUsers.slice(0, 8).map(u => (
                      <button
                        key={u.id}
                        type="button"
                        onClick={() => addParticipant(u)}
                        className="w-full flex items-center gap-2.5 px-3 py-2.5 hover:bg-gray-50 text-left"
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
          )}

          {error && (
            <div className="mt-4 bg-red-50 border border-red-200 rounded-lg px-3 py-2.5">
              <p className="text-xs text-red-600">{error}</p>
            </div>
          )}
        </div>

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
            onClick={handleSave}
            disabled={loading}
            className="px-4 py-2 text-sm font-semibold text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50 rounded-lg transition-colors flex items-center gap-2"
          >
            {loading && (
              <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
            )}
            Сохранить
          </button>
        </div>
      </div>
    </div>
  )
}
