import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import Navbar from '../components/Navbar'
import {
  getEscalations,
  EscalationSummary, EscalationFilter,
  EscalationStatus, EscalationPriority,
  STATUS_LABELS, STATUS_COLORS,
  PRIORITY_LABELS, PRIORITY_COLORS,
  TYPE_LABELS,
} from '../api/escalations'
import { useAuth } from '../context/AuthContext'

export type EscalationsMode = 'my' | 'assigned' | 'observed'

const MODE_META: Record<EscalationsMode, { title: string; empty: string }> = {
  my:       { title: 'Мои эскалации',  empty: 'Вы пока не создавали эскалации' },
  assigned: { title: 'Назначенные',    empty: 'Нет эскалаций, где вы исполнитель' },
  observed: { title: 'Наблюдаемые',   empty: 'Нет эскалаций, за которыми вы наблюдаете' },
}

const ALL_STATUSES: EscalationStatus[] = [
  'new', 'in_progress', 'waiting_for_response', 'resolved', 'closed', 'cancelled',
]
const ALL_PRIORITIES: EscalationPriority[] = ['low', 'medium', 'high', 'critical', 'blocker']

// Цвета активных пилюль для статусов/приоритетов (насыщеннее, без bg-* prefix)
const STATUS_ACTIVE: Record<EscalationStatus, string> = {
  new:                  'bg-gray-600 text-white',
  in_progress:          'bg-blue-600 text-white',
  waiting_for_response: 'bg-yellow-500 text-white',
  resolved:             'bg-green-600 text-white',
  closed:               'bg-gray-500 text-white',
  cancelled:            'bg-red-600 text-white',
}
const PRIORITY_ACTIVE: Record<EscalationPriority, string> = {
  low:      'bg-gray-500 text-white',
  medium:   'bg-blue-600 text-white',
  high:     'bg-orange-500 text-white',
  critical: 'bg-red-600 text-white',
  blocker:  'bg-purple-700 text-white',
}

function EscalationRowSkeleton() {
  return (
    <div className="bg-white rounded-xl border border-gray-100 px-5 py-4 animate-pulse flex items-center gap-4">
      <div className="h-5 w-20 bg-gray-200 rounded-full" />
      <div className="flex-1 h-4 bg-gray-200 rounded" />
      <div className="h-5 w-16 bg-gray-100 rounded-full" />
      <div className="h-4 w-24 bg-gray-100 rounded hidden sm:block" />
    </div>
  )
}

interface Props {
  mode: EscalationsMode
}

export default function EscalationsPage({ mode }: Props) {
  const { user } = useAuth()
  const meta = MODE_META[mode]

  const [items, setItems] = useState<EscalationSummary[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [total, setTotal] = useState(0)

  const [statusFilter, setStatusFilter]     = useState<EscalationStatus | null>(null)
  const [priorityFilter, setPriorityFilter] = useState<EscalationPriority | null>(null)

  const hasFilters = statusFilter !== null || priorityFilter !== null

  useEffect(() => {
    if (!user) return
    setLoading(true)
    setError(null)

    const base: EscalationFilter =
      mode === 'my'
        ? { authorId: user.id }
        : mode === 'assigned'
        ? { responsibleUserId: user.id, responsibleUserRole: 'assignee' }
        : { responsibleUserId: user.id, responsibleUserRole: 'observer' }

    const filter: EscalationFilter = {
      ...base,
      ...(statusFilter   ? { status: statusFilter }     : {}),
      ...(priorityFilter ? { priority: priorityFilter } : {}),
    }

    getEscalations(filter, 1, 100)
      .then(res => { setItems(res.items); setTotal(res.totalCount) })
      .catch(() => setError('Не удалось загрузить эскалации.'))
      .finally(() => setLoading(false))
  }, [mode, user, statusFilter, priorityFilter])

  // Сбрасываем фильтры при смене режима
  useEffect(() => {
    setStatusFilter(null)
    setPriorityFilter(null)
  }, [mode])

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />

      <main className="pt-14">
        <div className="max-w-5xl mx-auto px-6 py-8">

          {/* Header */}
          <div className="flex items-center justify-between mb-5">
            <div>
              <h1 className="text-xl font-bold text-gray-900">{meta.title}</h1>
              {!loading && !error && (
                <p className="text-sm text-gray-400 mt-0.5">
                  {total === 0 ? 'Нет эскалаций' : `${total} ${pluralEsc(total)}`}
                  {hasFilters && <span className="text-blue-500"> · фильтр активен</span>}
                </p>
              )}
            </div>
            {hasFilters && (
              <button
                onClick={() => { setStatusFilter(null); setPriorityFilter(null) }}
                className="text-xs text-gray-400 hover:text-gray-700 flex items-center gap-1 transition-colors"
              >
                <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
                Сбросить фильтры
              </button>
            )}
          </div>

          {/* Filters */}
          <div className="space-y-2.5 mb-5">
            {/* Status pills */}
            <div className="flex items-center gap-1.5 overflow-x-auto pb-0.5 scrollbar-none">
              <span className="text-xs text-gray-400 shrink-0 mr-1">Статус</span>
              <button
                onClick={() => setStatusFilter(null)}
                className={`shrink-0 px-3 py-1 rounded-full text-xs font-medium transition-colors ${
                  statusFilter === null
                    ? 'bg-gray-800 text-white'
                    : 'bg-white border border-gray-200 text-gray-500 hover:border-gray-300'
                }`}
              >
                Все
              </button>
              {ALL_STATUSES.map(s => (
                <button
                  key={s}
                  onClick={() => setStatusFilter(prev => prev === s ? null : s)}
                  className={`shrink-0 px-3 py-1 rounded-full text-xs font-medium transition-colors ${
                    statusFilter === s
                      ? STATUS_ACTIVE[s]
                      : `bg-white border border-gray-200 text-gray-500 hover:border-gray-300`
                  }`}
                >
                  {STATUS_LABELS[s]}
                </button>
              ))}
            </div>

            {/* Priority pills */}
            <div className="flex items-center gap-1.5 overflow-x-auto pb-0.5 scrollbar-none">
              <span className="text-xs text-gray-400 shrink-0 mr-1">Приоритет</span>
              <button
                onClick={() => setPriorityFilter(null)}
                className={`shrink-0 px-3 py-1 rounded-full text-xs font-medium transition-colors ${
                  priorityFilter === null
                    ? 'bg-gray-800 text-white'
                    : 'bg-white border border-gray-200 text-gray-500 hover:border-gray-300'
                }`}
              >
                Все
              </button>
              {ALL_PRIORITIES.map(p => (
                <button
                  key={p}
                  onClick={() => setPriorityFilter(prev => prev === p ? null : p)}
                  className={`shrink-0 px-3 py-1 rounded-full text-xs font-medium transition-colors ${
                    priorityFilter === p
                      ? PRIORITY_ACTIVE[p]
                      : 'bg-white border border-gray-200 text-gray-500 hover:border-gray-300'
                  }`}
                >
                  {PRIORITY_LABELS[p]}
                </button>
              ))}
            </div>
          </div>

          {/* Error */}
          {error && (
            <div className="text-center py-16 text-gray-400">
              <p className="text-sm">{error}</p>
            </div>
          )}

          {/* List */}
          <div className="space-y-2">
            {loading
              ? Array.from({ length: 6 }).map((_, i) => <EscalationRowSkeleton key={i} />)
              : items.map(e => <EscalationRow key={e.id} escalation={e} />)
            }
          </div>

          {/* Empty */}
          {!loading && !error && items.length === 0 && (
            <div className="text-center py-20">
              <svg className="w-12 h-12 mx-auto mb-4 text-gray-200" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                  d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
              </svg>
              <p className="text-gray-400 text-sm">
                {hasFilters ? 'Нет эскалаций с такими фильтрами' : meta.empty}
              </p>
              {hasFilters && (
                <button
                  onClick={() => { setStatusFilter(null); setPriorityFilter(null) }}
                  className="mt-3 text-sm text-blue-500 hover:underline"
                >
                  Сбросить фильтры
                </button>
              )}
            </div>
          )}

        </div>
      </main>
    </div>
  )
}

function EscalationRow({ escalation: e }: { escalation: EscalationSummary }) {
  const date = new Date(e.createdAt).toLocaleDateString('ru-RU', {
    day: 'numeric', month: 'short',
  })

  return (
    <Link
      to={`/escalations/${e.id}`}
      className="block bg-white rounded-xl border border-gray-100 px-5 py-4 hover:border-blue-200 hover:shadow-sm transition-all duration-150 group"
    >
      <div className="flex items-center gap-3 flex-wrap">
        <span className={`text-xs font-semibold px-2 py-0.5 rounded-full shrink-0 ${PRIORITY_COLORS[e.priority]}`}>
          {PRIORITY_LABELS[e.priority]}
        </span>
        <span className={`text-xs font-medium px-2 py-0.5 rounded-full shrink-0 ${STATUS_COLORS[e.status]}`}>
          {STATUS_LABELS[e.status]}
        </span>
        <span className="flex-1 min-w-0 text-sm font-medium text-gray-800 truncate group-hover:text-blue-600 transition-colors">
          {e.name}
        </span>
        <span className="text-xs text-gray-400 shrink-0 hidden sm:inline">
          {TYPE_LABELS[e.type]}
        </span>
        <span className="text-xs text-gray-500 bg-gray-50 border border-gray-100 px-2 py-0.5 rounded-md shrink-0 hidden md:inline">
          {e.projectName}
        </span>
        <span className="text-xs text-gray-400 shrink-0">{date}</span>
        <svg className="w-4 h-4 text-gray-300 group-hover:text-blue-400 transition-colors shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
        </svg>
      </div>
      <p className="text-xs text-gray-400 mt-1.5 md:hidden">{e.projectName}</p>
    </Link>
  )
}

function pluralEsc(n: number) {
  const mod10 = n % 10
  const mod100 = n % 100
  if (mod10 === 1 && mod100 !== 11) return 'эскалация'
  if (mod10 >= 2 && mod10 <= 4 && (mod100 < 10 || mod100 >= 20)) return 'эскалации'
  return 'эскалаций'
}
