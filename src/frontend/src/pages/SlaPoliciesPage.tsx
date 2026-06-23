import { useEffect, useState } from 'react'
import Navbar from '../components/Navbar'
import { useAuth } from '../context/AuthContext'
import {
  SlaPolicyResponse, CreateSlaPolicyRequest, UpdateSlaPolicyRequest,
  PRIORITIES, PRIORITY_LABELS, PRIORITY_COLORS, EscalationPriority,
  getSlaPolicies, createSlaPolicy, updateSlaPolicy, deleteSlaPolicy,
} from '../api/slaPolicies'
import { ApiError } from '../api/auth'

// ─── helpers ────────────────────────────────────────────────────────────────

function fmtMinutes(m: number): string {
  if (m < 60) return `${m} мин`
  const h = Math.floor(m / 60)
  const rem = m % 60
  return rem ? `${h} ч ${rem} мин` : `${h} ч`
}

// ─── modal ───────────────────────────────────────────────────────────────────

interface FormState {
  name: string
  description: string
  minPriorityLevel: EscalationPriority
  responseTimeMinutes: string
  resolutionTimeMinutes: string
}

const EMPTY_FORM: FormState = {
  name: '',
  description: '',
  minPriorityLevel: 'medium',
  responseTimeMinutes: '60',
  resolutionTimeMinutes: '240',
}

interface ModalProps {
  title: string
  initial: FormState
  saving: boolean
  error: string | null
  onSave: (f: FormState) => void
  onClose: () => void
}

function SlaPolicyModal({ title, initial, saving, error, onSave, onClose }: ModalProps) {
  const [form, setForm] = useState<FormState>(initial)

  function set(patch: Partial<FormState>) {
    setForm(f => ({ ...f, ...patch }))
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    onSave(form)
  }

  const responseMin = parseInt(form.responseTimeMinutes) || 0
  const resolutionMin = parseInt(form.resolutionTimeMinutes) || 0
  const timeError = resolutionMin > 0 && responseMin > 0 && resolutionMin < responseMin
    ? 'Время решения не может быть меньше времени реакции'
    : null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <div className="w-full max-w-lg bg-white rounded-2xl shadow-xl">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
          <h2 className="text-base font-semibold text-gray-900">{title}</h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 transition-colors"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit} className="px-6 py-5 space-y-4">
          {/* Name */}
          <div>
            <label className="block text-xs font-medium text-gray-700 mb-1">
              Название <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              required
              maxLength={200}
              value={form.name}
              onChange={e => set({ name: e.target.value })}
              placeholder="Например: SLA для критических инцидентов"
              className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {/* Description */}
          <div>
            <label className="block text-xs font-medium text-gray-700 mb-1">Описание</label>
            <textarea
              rows={2}
              value={form.description}
              onChange={e => set({ description: e.target.value })}
              placeholder="Необязательно"
              className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none"
            />
          </div>

          {/* Min priority */}
          <div>
            <label className="block text-xs font-medium text-gray-700 mb-1">
              Минимальный приоритет <span className="text-red-500">*</span>
            </label>
            <select
              required
              value={form.minPriorityLevel}
              onChange={e => set({ minPriorityLevel: e.target.value as EscalationPriority })}
              className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
            >
              {PRIORITIES.map(p => (
                <option key={p} value={p}>{PRIORITY_LABELS[p]}</option>
              ))}
            </select>
          </div>

          {/* Time fields */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Время реакции (мин) <span className="text-red-500">*</span>
              </label>
              <input
                type="number"
                required
                min={1}
                value={form.responseTimeMinutes}
                onChange={e => set({ responseTimeMinutes: e.target.value })}
                className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
              {responseMin > 0 && (
                <p className="text-xs text-gray-400 mt-1">{fmtMinutes(responseMin)}</p>
              )}
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-700 mb-1">
                Время решения (мин) <span className="text-red-500">*</span>
              </label>
              <input
                type="number"
                required
                min={1}
                value={form.resolutionTimeMinutes}
                onChange={e => set({ resolutionTimeMinutes: e.target.value })}
                className="w-full px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
              {resolutionMin > 0 && (
                <p className="text-xs text-gray-400 mt-1">{fmtMinutes(resolutionMin)}</p>
              )}
            </div>
          </div>

          {timeError && (
            <p className="text-xs text-red-500">{timeError}</p>
          )}

          {error && (
            <div className="flex items-start gap-2 px-3 py-2.5 bg-red-50 border border-red-200 rounded-lg">
              <svg className="w-4 h-4 text-red-500 shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                  d="M12 9v2m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <p className="text-xs text-red-700">{error}</p>
            </div>
          )}

          {/* Actions */}
          <div className="flex justify-end gap-3 pt-1">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm text-gray-600 hover:text-gray-800 transition-colors"
            >
              Отмена
            </button>
            <button
              type="submit"
              disabled={saving || !!timeError}
              className="px-5 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              {saving ? 'Сохранение…' : 'Сохранить'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

// ─── confirm dialog ───────────────────────────────────────────────────────────

interface ConfirmProps {
  name: string
  onConfirm: () => void
  onCancel: () => void
}

function DeleteConfirm({ name, onConfirm, onCancel }: ConfirmProps) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <div className="w-full max-w-sm bg-white rounded-2xl shadow-xl px-6 py-5">
        <h3 className="text-base font-semibold text-gray-900 mb-2">Удалить политику?</h3>
        <p className="text-sm text-gray-500 mb-5">
          Политика <span className="font-medium text-gray-700">«{name}»</span> будет удалена безвозвратно.
        </p>
        <div className="flex justify-end gap-3">
          <button
            onClick={onCancel}
            className="px-4 py-2 text-sm text-gray-600 hover:text-gray-800 transition-colors"
          >
            Отмена
          </button>
          <button
            onClick={onConfirm}
            className="px-5 py-2 text-sm font-medium text-white bg-red-600 rounded-lg hover:bg-red-700 transition-colors"
          >
            Удалить
          </button>
        </div>
      </div>
    </div>
  )
}

// ─── skeleton ─────────────────────────────────────────────────────────────────

function RowSkeleton() {
  return (
    <tr className="border-t border-gray-100 animate-pulse">
      <td className="px-4 py-3"><div className="h-4 w-40 bg-gray-100 rounded" /></td>
      <td className="px-4 py-3"><div className="h-5 w-20 bg-gray-100 rounded-full" /></td>
      <td className="px-4 py-3"><div className="h-4 w-16 bg-gray-100 rounded" /></td>
      <td className="px-4 py-3"><div className="h-4 w-16 bg-gray-100 rounded" /></td>
      <td className="px-4 py-3"><div className="h-5 w-16 bg-gray-100 rounded-full" /></td>
      <td className="px-4 py-3" />
    </tr>
  )
}

// ─── page ─────────────────────────────────────────────────────────────────────

type Modal =
  | { kind: 'create' }
  | { kind: 'edit'; policy: SlaPolicyResponse }
  | { kind: 'delete'; policy: SlaPolicyResponse }

export default function SlaPoliciesPage() {
  const { user } = useAuth()
  const canEdit = (user?.role ?? 0) >= 2

  const [policies, setPolicies] = useState<SlaPolicyResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)

  const [modal, setModal] = useState<Modal | null>(null)
  const [saving, setSaving] = useState(false)
  const [modalError, setModalError] = useState<string | null>(null)

  function load() {
    setLoading(true)
    setLoadError(null)
    getSlaPolicies()
      .then(r => setPolicies(r.items))
      .catch(() => setLoadError('Не удалось загрузить политики.'))
      .finally(() => setLoading(false))
  }

  useEffect(() => { load() }, [])

  function openCreate() {
    setModalError(null)
    setModal({ kind: 'create' })
  }

  function openEdit(p: SlaPolicyResponse) {
    setModalError(null)
    setModal({ kind: 'edit', policy: p })
  }

  function openDelete(p: SlaPolicyResponse) {
    setModal({ kind: 'delete', policy: p })
  }

  async function handleSave(form: FormState) {
    const req: CreateSlaPolicyRequest | UpdateSlaPolicyRequest = {
      name: form.name.trim(),
      description: form.description.trim() || undefined,
      minPriorityLevel: form.minPriorityLevel,
      responseTimeMinutes: parseInt(form.responseTimeMinutes),
      resolutionTimeMinutes: parseInt(form.resolutionTimeMinutes),
    }

    setSaving(true)
    setModalError(null)
    try {
      if (modal?.kind === 'create') {
        await createSlaPolicy(req)
      } else if (modal?.kind === 'edit') {
        await updateSlaPolicy(modal.policy.id, req)
      }
      setModal(null)
      load()
    } catch (e: unknown) {
      const err = e as ApiError
      setModalError(err?.detail ?? 'Ошибка при сохранении')
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete() {
    if (modal?.kind !== 'delete') return
    try {
      await deleteSlaPolicy(modal.policy.id)
      setModal(null)
      load()
    } catch {
      setModal(null)
      load()
    }
  }

  const editInitial = (p: SlaPolicyResponse): FormState => ({
    name: p.name,
    description: p.description ?? '',
    minPriorityLevel: p.minPriorityLevel,
    responseTimeMinutes: String(p.responseTimeMinutes),
    resolutionTimeMinutes: String(p.resolutionTimeMinutes),
  })

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />

      <main className="pt-14">
        <div className="max-w-5xl mx-auto px-6 py-8">
          {/* Header */}
          <div className="flex items-center justify-between mb-6">
            <div>
              <h1 className="text-xl font-bold text-gray-900">SLA-политики</h1>
              {!loading && (
                <p className="text-sm text-gray-500 mt-0.5">
                  {policies.length === 0
                    ? 'Политик пока нет'
                    : `${policies.length} ${pluralize(policies.length, 'политика', 'политики', 'политик')}`}
                </p>
              )}
            </div>

            {canEdit && (
              <button
                onClick={openCreate}
                className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 transition-colors"
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                </svg>
                Создать политику
              </button>
            )}
          </div>

          {/* Error */}
          {loadError && (
            <div className="py-16 text-center text-sm text-gray-400">{loadError}</div>
          )}

          {/* Table */}
          {!loadError && (
            <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-gray-50 border-b border-gray-100">
                    <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Название
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Мин. приоритет
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Реакция
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Решение
                    </th>
                    <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Статус
                    </th>
                    {canEdit && <th className="px-4 py-3 w-24" />}
                  </tr>
                </thead>
                <tbody>
                  {loading
                    ? Array.from({ length: 4 }).map((_, i) => <RowSkeleton key={i} />)
                    : policies.map(p => (
                        <tr key={p.id} className="border-t border-gray-100 hover:bg-gray-50 transition-colors">
                          <td className="px-4 py-3">
                            <p className="font-medium text-gray-900">{p.name}</p>
                            {p.description && (
                              <p className="text-xs text-gray-400 mt-0.5 line-clamp-1">{p.description}</p>
                            )}
                          </td>
                          <td className="px-4 py-3">
                            <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${PRIORITY_COLORS[p.minPriorityLevel]}`}>
                              {PRIORITY_LABELS[p.minPriorityLevel]}
                            </span>
                          </td>
                          <td className="px-4 py-3 text-gray-700">{fmtMinutes(p.responseTimeMinutes)}</td>
                          <td className="px-4 py-3 text-gray-700">{fmtMinutes(p.resolutionTimeMinutes)}</td>
                          <td className="px-4 py-3">
                            <span className={`inline-flex items-center gap-1 text-xs font-medium px-2 py-0.5 rounded-full ${
                              p.isActive ? 'bg-green-50 text-green-700' : 'bg-gray-100 text-gray-500'
                            }`}>
                              <span className={`w-1.5 h-1.5 rounded-full ${p.isActive ? 'bg-green-500' : 'bg-gray-400'}`} />
                              {p.isActive ? 'Активна' : 'Неактивна'}
                            </span>
                          </td>
                          {canEdit && (
                            <td className="px-4 py-3">
                              <div className="flex items-center justify-end gap-2">
                                <button
                                  onClick={() => openEdit(p)}
                                  title="Редактировать"
                                  className="p-1.5 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                                >
                                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                                      d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                                  </svg>
                                </button>
                                <button
                                  onClick={() => openDelete(p)}
                                  title="Удалить"
                                  className="p-1.5 text-gray-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors"
                                >
                                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                                      d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                                  </svg>
                                </button>
                              </div>
                            </td>
                          )}
                        </tr>
                      ))}
                </tbody>
              </table>

              {/* Empty state */}
              {!loading && policies.length === 0 && (
                <div className="text-center py-16">
                  <svg className="w-10 h-10 mx-auto mb-3 text-gray-200" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                      d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                  </svg>
                  <p className="text-sm text-gray-400">SLA-политик пока нет</p>
                  {canEdit && (
                    <button
                      onClick={openCreate}
                      className="mt-3 text-sm text-blue-600 hover:underline"
                    >
                      Создать первую политику
                    </button>
                  )}
                </div>
              )}
            </div>
          )}
        </div>
      </main>

      {/* Modals */}
      {modal?.kind === 'create' && (
        <SlaPolicyModal
          title="Новая SLA-политика"
          initial={EMPTY_FORM}
          saving={saving}
          error={modalError}
          onSave={handleSave}
          onClose={() => setModal(null)}
        />
      )}

      {modal?.kind === 'edit' && (
        <SlaPolicyModal
          title="Редактировать политику"
          initial={editInitial(modal.policy)}
          saving={saving}
          error={modalError}
          onSave={handleSave}
          onClose={() => setModal(null)}
        />
      )}

      {modal?.kind === 'delete' && (
        <DeleteConfirm
          name={modal.policy.name}
          onConfirm={handleDelete}
          onCancel={() => setModal(null)}
        />
      )}
    </div>
  )
}

function pluralize(n: number, one: string, few: string, many: string) {
  const mod10 = n % 10
  const mod100 = n % 100
  if (mod10 === 1 && mod100 !== 11) return one
  if (mod10 >= 2 && mod10 <= 4 && (mod100 < 10 || mod100 >= 20)) return few
  return many
}
