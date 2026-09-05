import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import Navbar from '../components/Navbar'
import { useAuth } from '../context/AuthContext'
import {
  getEscalations,
  EscalationSummary, EscalationFilter,
  STATUS_COLORS, STATUS_LABELS,
} from '../api/escalations'

// ── Типы ──────────────────────────────────────────────────────────────────────
interface SectionStats {
  total: number
  active: number       // new | in_progress | waiting_for_response
  done: number         // resolved | closed
  cancelled: number
  criticalActive: number   // critical/blocker + активный
  slaAtRisk: number    // active + high/critical/blocker priority
  completionRate: number   // done / (total - cancelled) * 100
  byStatus: Partial<Record<string, number>>
}

function computeStats(items: EscalationSummary[]): SectionStats {
  const ACTIVE   = new Set(['new', 'in_progress', 'waiting_for_response'])
  const DONE     = new Set(['resolved', 'closed'])
  const HIGH_PRI = new Set(['critical', 'blocker'])

  const active    = items.filter(e => ACTIVE.has(e.status)).length
  const done      = items.filter(e => DONE.has(e.status)).length
  const cancelled = items.filter(e => e.status === 'cancelled').length
  const criticalActive = items.filter(e => ACTIVE.has(e.status) && HIGH_PRI.has(e.priority)).length
  const slaAtRisk = items.filter(e =>
    ACTIVE.has(e.status) && ['high', 'critical', 'blocker'].includes(e.priority)
  ).length

  const effective = items.length - cancelled
  const completionRate = effective > 0 ? Math.round((done / effective) * 100) : 0

  const byStatus: Partial<Record<string, number>> = {}
  for (const e of items) byStatus[e.status] = (byStatus[e.status] ?? 0) + 1

  return { total: items.length, active, done, cancelled, criticalActive, slaAtRisk, completionRate, byStatus }
}

// ── Мини-бар статусов ─────────────────────────────────────────────────────────
const STATUS_BAR_COLORS: Partial<Record<string, string>> = {
  new:                  'bg-gray-400',
  in_progress:          'bg-blue-500',
  waiting_for_response: 'bg-yellow-400',
  resolved:             'bg-green-500',
  closed:               'bg-gray-300',
  cancelled:            'bg-red-400',
}

function StatusBar({ byStatus, total }: { byStatus: Partial<Record<string, number>>; total: number }) {
  if (total === 0) return null
  const order = ['new', 'in_progress', 'waiting_for_response', 'resolved', 'closed', 'cancelled']
  return (
    <div className="flex h-1.5 rounded-full overflow-hidden gap-px mt-3">
      {order.map(s => {
        const count = byStatus[s] ?? 0
        if (!count) return null
        const pct = (count / total) * 100
        return (
          <div
            key={s}
            className={`${STATUS_BAR_COLORS[s]} transition-all`}
            style={{ width: `${pct}%` }}
            title={`${STATUS_LABELS[s as keyof typeof STATUS_LABELS]}: ${count}`}
          />
        )
      })}
    </div>
  )
}

// ── Скелетон карточки ─────────────────────────────────────────────────────────
function StatCardSkeleton() {
  return (
    <div className="bg-white rounded-xl border border-gray-200 p-5 animate-pulse">
      <div className="h-3 w-24 bg-gray-200 rounded mb-4" />
      <div className="h-8 w-12 bg-gray-200 rounded mb-2" />
      <div className="h-2 w-full bg-gray-100 rounded-full mt-3" />
      <div className="flex gap-2 mt-3">
        <div className="h-3 w-16 bg-gray-100 rounded" />
        <div className="h-3 w-16 bg-gray-100 rounded" />
      </div>
    </div>
  )
}

// ── Карточка секции ───────────────────────────────────────────────────────────
interface StatCardProps {
  title: string
  to: string
  stats: SectionStats
  accentColor: string
}

function StatCard({ title, to, stats, accentColor }: StatCardProps) {
  return (
    <Link
      to={to}
      className="group bg-white rounded-xl border border-gray-200 p-5 hover:border-blue-200 hover:shadow-md transition-all duration-150 block"
    >
      <div className="flex items-center justify-between mb-1">
        <span className="text-xs font-medium text-gray-400">{title}</span>
        <svg className="w-3.5 h-3.5 text-gray-300 group-hover:text-blue-400 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
        </svg>
      </div>

      {/* Big number */}
      <p className="text-3xl font-bold text-gray-900 mt-1">{stats.total}</p>
      <p className="text-xs text-gray-400 -mt-0.5">эскалаций всего</p>

      {/* Completion rate */}
      {stats.total > 0 && (
        <>
          <div className="flex items-center justify-between mt-3 mb-1">
            <span className="text-xs text-gray-400">Выполнено</span>
            <span className={`text-xs font-semibold ${stats.completionRate >= 70 ? 'text-green-600' : stats.completionRate >= 40 ? 'text-yellow-600' : 'text-gray-500'}`}>
              {stats.completionRate}%
            </span>
          </div>
          <div className="h-1.5 bg-gray-100 rounded-full overflow-hidden">
            <div
              className={`h-full rounded-full transition-all ${accentColor}`}
              style={{ width: `${stats.completionRate}%` }}
            />
          </div>
        </>
      )}

      {/* Status bar */}
      {stats.total > 0 && <StatusBar byStatus={stats.byStatus} total={stats.total} />}

      {/* Meta */}
      {stats.total > 0 && (
        <div className="flex items-center gap-3 mt-3 flex-wrap">
          {stats.active > 0 && (
            <span className="text-xs text-blue-600 bg-blue-50 px-2 py-0.5 rounded-md font-medium">
              {stats.active} активных
            </span>
          )}
          {stats.done > 0 && (
            <span className="text-xs text-green-600 bg-green-50 px-2 py-0.5 rounded-md font-medium">
              {stats.done} закрытых
            </span>
          )}
          {stats.criticalActive > 0 && (
            <span className="text-xs text-red-600 bg-red-50 px-2 py-0.5 rounded-md font-medium">
              ⚡ {stats.criticalActive} крит.
            </span>
          )}
        </div>
      )}

      {stats.total === 0 && (
        <p className="text-xs text-gray-300 mt-3 italic">Нет эскалаций</p>
      )}
    </Link>
  )
}

// ── Главная страница ──────────────────────────────────────────────────────────
const EMPTY_STATS: SectionStats = {
  total: 0, active: 0, done: 0, cancelled: 0,
  criticalActive: 0, slaAtRisk: 0, completionRate: 0, byStatus: {},
}

export default function HomePage() {
  const { user } = useAuth()

  const [myStats,       setMyStats]       = useState<SectionStats>(EMPTY_STATS)
  const [assignedStats, setAssignedStats] = useState<SectionStats>(EMPTY_STATS)
  const [observedStats, setObservedStats] = useState<SectionStats>(EMPTY_STATS)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!user) return

    const fetchSection = (filter: EscalationFilter) =>
      getEscalations(filter, 1, 500).then(r => computeStats(r.items)).catch(() => EMPTY_STATS)

    Promise.all([
      fetchSection({ authorId: user.id }),
      fetchSection({ responsibleUserId: user.id, responsibleUserRole: 'assignee' }),
      fetchSection({ responsibleUserId: user.id, responsibleUserRole: 'observer' }),
    ]).then(([my, assigned, observed]) => {
      setMyStats(my)
      setAssignedStats(assigned)
      setObservedStats(observed)
    }).finally(() => setLoading(false))
  }, [user])

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />

      <main className="pt-14">

        {/* Hero */}
        <div className="bg-white border-b border-gray-100">
          <div className="max-w-5xl mx-auto px-6 py-10">
            <div className="flex items-start justify-between gap-6 flex-wrap">
              <div>
                <div className="flex items-center gap-3 mb-2">
                  <div className="w-10 h-10 rounded-xl bg-blue-600 flex items-center justify-center text-white font-black text-base select-none shrink-0">
                    W
                  </div>
                  <div>
                    <h1 className="text-xl font-bold text-gray-900 leading-tight">
                      Добро пожаловать{user ? `, ${user.firstName}` : ''}!
                    </h1>
                    <p className="text-xs text-gray-400">WaveAccess Escalation Platform</p>
                  </div>
                </div>
              </div>

            </div>
          </div>
        </div>

        <div className="max-w-5xl mx-auto px-6 py-8 space-y-8">

          {/* Статистика по секциям */}
          <div>
            <h2 className="text-xs font-semibold text-gray-400 uppercase tracking-widest mb-4">
              Ваша статистика
            </h2>
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
              {loading ? (
                <>
                  <StatCardSkeleton />
                  <StatCardSkeleton />
                  <StatCardSkeleton />
                </>
              ) : (
                <>
                  <StatCard
                    title="Мои эскалации"
                    to="/my-escalations"
                    stats={myStats}
                    accentColor="bg-violet-500"
                  />
                  <StatCard
                    title="Назначенные"
                    to="/assigned"
                    stats={assignedStats}
                    accentColor="bg-green-500"
                  />
                  <StatCard
                    title="Наблюдаемые"
                    to="/observed"
                    stats={observedStats}
                    accentColor="bg-orange-400"
                  />
                </>
              )}
            </div>
          </div>

          {/* Навигация */}
          <div>
            <h2 className="text-xs font-semibold text-gray-400 uppercase tracking-widest mb-4">
              Разделы
            </h2>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              {NAV_CARDS.map(card => (
                <Link
                  key={card.to}
                  to={card.to}
                  className="group bg-white rounded-xl border border-gray-200 p-4 flex items-center gap-3 hover:border-blue-200 hover:shadow-sm transition-all duration-150"
                >
                  <div className={`w-9 h-9 rounded-lg ${card.iconBg} ${card.iconColor} flex items-center justify-center shrink-0`}>
                    {card.icon}
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-semibold text-gray-900 group-hover:text-blue-600 transition-colors">{card.title}</p>
                    <p className="text-xs text-gray-400 truncate">{card.subtitle}</p>
                  </div>
                  <svg className="w-4 h-4 text-gray-300 group-hover:text-gray-500 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                  </svg>
                </Link>
              ))}
            </div>
          </div>

        </div>
      </main>
    </div>
  )
}

// ── Навигационные карточки ────────────────────────────────────────────────────
const NAV_CARDS = [
  {
    to: '/projects',
    title: 'Проекты',
    subtitle: 'Все проекты платформы',
    iconBg: 'bg-blue-50', iconColor: 'text-blue-600',
    icon: (
      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
          d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z" />
      </svg>
    ),
  },
  {
    to: '/my-escalations',
    title: 'Мои эскалации',
    subtitle: 'Созданные вами',
    iconBg: 'bg-violet-50', iconColor: 'text-violet-600',
    icon: (
      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
          d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
      </svg>
    ),
  },
  {
    to: '/assigned',
    title: 'Назначенные',
    subtitle: 'Вы исполнитель',
    iconBg: 'bg-green-50', iconColor: 'text-green-600',
    icon: (
      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
          d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" />
      </svg>
    ),
  },
  {
    to: '/observed',
    title: 'Наблюдаемые',
    subtitle: 'За которыми наблюдаете',
    iconBg: 'bg-orange-50', iconColor: 'text-orange-500',
    icon: (
      <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
          d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
          d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
      </svg>
    ),
  },
]
