import { useEffect, useState } from 'react'
import Navbar from '../components/Navbar'
import { getUsersStats, UserStatsItem } from '../api/escalations'
import { getUsers, UserProfile, WORK_TYPE_LABELS } from '../api/users'

// ── Утилиты ───────────────────────────────────────────────────────────────────
function formatMinutes(minutes: number | null | undefined): string {
  if (minutes == null || minutes === 0) return '—'
  if (minutes < 60) return `${Math.round(minutes)} мин`
  const h = Math.floor(minutes / 60)
  const m = Math.round(minutes % 60)
  return m > 0 ? `${h} ч ${m} м` : `${h} ч`
}

function scoreColor(score: number) {
  if (score >= 75) return 'text-green-600'
  if (score >= 50) return 'text-yellow-600'
  return 'text-red-600'
}

function scoreBar(score: number) {
  if (score >= 75) return 'bg-green-500'
  if (score >= 50) return 'bg-yellow-400'
  return 'bg-red-500'
}

// ── Скелетон строки ───────────────────────────────────────────────────────────
function RowSkeleton() {
  return (
    <div className="bg-white rounded-xl border border-gray-100 px-5 py-4 animate-pulse flex items-center gap-4">
      <div className="w-9 h-9 bg-gray-200 rounded-full shrink-0" />
      <div className="flex-1 space-y-1.5">
        <div className="h-4 w-32 bg-gray-200 rounded" />
        <div className="h-3 w-20 bg-gray-100 rounded" />
      </div>
      <div className="h-4 w-10 bg-gray-200 rounded hidden sm:block" />
      <div className="h-4 w-10 bg-gray-200 rounded hidden sm:block" />
      <div className="h-4 w-10 bg-gray-200 rounded hidden sm:block" />
      <div className="h-6 w-24 bg-gray-200 rounded-full" />
    </div>
  )
}

// ── Строка пользователя ───────────────────────────────────────────────────────
interface RowProps {
  profile: UserProfile | undefined
  stats: UserStatsItem
  rank: number
}

function UserRow({ profile, stats, rank }: RowProps) {
  const [expanded, setExpanded] = useState(false)

  const name = profile
    ? `${profile.firstName} ${profile.lastName}`
    : stats.userId.slice(0, 8) + '…'

  const slaRate = stats.closedWithMetrics > 0
    ? Math.round((stats.slaCompliant / stats.closedWithMetrics) * 100)
    : null

  return (
    <div className="bg-white rounded-xl border border-gray-100 overflow-hidden transition-shadow hover:shadow-sm">
      {/* Main row */}
      <button
        onClick={() => setExpanded(v => !v)}
        className="w-full text-left px-5 py-4 flex items-center gap-4"
      >
        {/* Rank */}
        <span className="text-xs font-bold text-gray-300 w-5 shrink-0 text-right">{rank}</span>

        {/* Avatar */}
        <div className="w-9 h-9 rounded-full bg-blue-600 flex items-center justify-center text-white text-xs font-bold shrink-0 select-none">
          {profile ? `${profile.firstName[0]}${profile.lastName[0]}` : '?'}
        </div>

        {/* Name + role */}
        <div className="flex-1 min-w-0">
          <p className="text-sm font-semibold text-gray-800 truncate">{name}</p>
          {profile && (
            <p className="text-xs text-gray-400 truncate">{WORK_TYPE_LABELS[profile.workType] ?? '—'}</p>
          )}
        </div>

        {/* Quick stats */}
        <div className="hidden sm:flex items-center gap-6 text-center shrink-0">
          <div>
            <p className="text-sm font-semibold text-gray-700">{stats.total}</p>
            <p className="text-xs text-gray-400">всего</p>
          </div>
          <div>
            <p className="text-sm font-semibold text-blue-600">{stats.active}</p>
            <p className="text-xs text-gray-400">активных</p>
          </div>
          <div>
            <p className="text-sm font-semibold text-green-600">{stats.closed}</p>
            <p className="text-xs text-gray-400">закрытых</p>
          </div>
        </div>

        {/* Efficiency score */}
        <div className="shrink-0 flex flex-col items-end gap-1 min-w-[80px]">
          <span className={`text-base font-bold ${scoreColor(stats.efficiencyScore)}`}>
            {stats.efficiencyScore}%
          </span>
          <div className="w-20 h-1.5 bg-gray-100 rounded-full overflow-hidden">
            <div
              className={`h-full rounded-full transition-all ${scoreBar(stats.efficiencyScore)}`}
              style={{ width: `${stats.efficiencyScore}%` }}
            />
          </div>
        </div>

        {/* Chevron */}
        <svg
          className={`w-4 h-4 text-gray-300 shrink-0 transition-transform ${expanded ? 'rotate-180' : ''}`}
          fill="none" stroke="currentColor" viewBox="0 0 24 24"
        >
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
        </svg>
      </button>

      {/* Expanded details */}
      {expanded && (
        <div className="border-t border-gray-50 px-5 py-4 grid grid-cols-2 sm:grid-cols-4 gap-3 bg-gray-50">
          <Detail label="Выполнение" value={`${stats.completionPercent}%`} accent={stats.completionPercent} />
          <Detail
            label="Соблюдение SLA"
            value={slaRate !== null ? `${slaRate}%` : '—'}
            accent={slaRate ?? undefined}
          />
          <Detail label="Ср. первый ответ" value={formatMinutes(stats.avgFirstResponseMinutes)} />
          <Detail label="Ср. время решения" value={formatMinutes(stats.avgResolutionMinutes)} />
          <Detail label="Нарушений SLA" value={String(stats.slaViolated)} bad={stats.slaViolated > 0} />
          <Detail label="SLA соблюдено" value={String(stats.slaCompliant)} />
          <Detail label="Отменено" value={String(stats.cancelled)} />
          <Detail label="С метриками" value={`${stats.closedWithMetrics} закрытых`} />
        </div>
      )}
    </div>
  )
}

function Detail({ label, value, accent, bad }: {
  label: string; value: string; accent?: number; bad?: boolean
}) {
  return (
    <div className="bg-white rounded-lg border border-gray-100 p-3">
      <p className="text-xs text-gray-400 mb-1">{label}</p>
      <p className={`text-sm font-semibold ${bad ? 'text-red-600' : 'text-gray-800'}`}>{value}</p>
      {accent !== undefined && (
        <div className="h-1 bg-gray-100 rounded-full mt-1.5 overflow-hidden">
          <div
            className={`h-full rounded-full ${scoreBar(accent)}`}
            style={{ width: `${Math.min(accent, 100)}%` }}
          />
        </div>
      )}
    </div>
  )
}

// ── Страница ─────────────────────────────────────────────────────────────────
export default function TeamStatsPage() {
  const [stats,    setStats]    = useState<UserStatsItem[]>([])
  const [profiles, setProfiles] = useState<Map<string, UserProfile>>(new Map())
  const [loading,  setLoading]  = useState(true)
  const [error,    setError]    = useState<string | null>(null)
  const [search,   setSearch]   = useState('')

  useEffect(() => {
    Promise.all([
      getUsersStats(),
      getUsers(1, 200),
    ])
      .then(([statsData, usersPage]) => {
        // Сортируем по efficiencyScore убыванию
        setStats([...statsData].sort((a, b) => b.efficiencyScore - a.efficiencyScore))
        const map = new Map<string, UserProfile>()
        usersPage.items.forEach(u => map.set(u.id, u))
        setProfiles(map)
      })
      .catch(() => setError('Не удалось загрузить статистику.'))
      .finally(() => setLoading(false))
  }, [])

  const filtered = stats.filter(s => {
    if (!search.trim()) return true
    const profile = profiles.get(s.userId)
    const name = profile ? `${profile.firstName} ${profile.lastName}`.toLowerCase() : ''
    return name.includes(search.toLowerCase())
  })

  // Сводные цифры
  const avgScore = stats.length > 0
    ? Math.round(stats.reduce((sum, s) => sum + s.efficiencyScore, 0) / stats.length)
    : 0

  const totalActive = stats.reduce((sum, s) => sum + s.active, 0)
  const slaRisk = stats.filter(s => s.slaViolated > s.slaCompliant).length

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />

      <main className="pt-14">
        <div className="max-w-4xl mx-auto px-6 py-8">

          {/* Header */}
          <div className="mb-6">
            <h1 className="text-xl font-bold text-gray-900">Аналитика команды</h1>
            <p className="text-sm text-gray-400 mt-0.5">
              Эффективность исполнителей на основе метрик закрытых эскалаций
            </p>
          </div>

          {/* Summary tiles */}
          {!loading && stats.length > 0 && (
            <div className="grid grid-cols-3 gap-3 mb-6">
              <div className="bg-white rounded-xl border border-gray-200 p-4 text-center">
                <p className="text-2xl font-bold text-gray-900">{stats.length}</p>
                <p className="text-xs text-gray-400 mt-0.5">исполнителей</p>
              </div>
              <div className="bg-white rounded-xl border border-gray-200 p-4 text-center">
                <p className={`text-2xl font-bold ${scoreColor(avgScore)}`}>{avgScore}%</p>
                <p className="text-xs text-gray-400 mt-0.5">средний балл</p>
              </div>
              <div className="bg-white rounded-xl border border-gray-200 p-4 text-center">
                <p className={`text-2xl font-bold ${slaRisk > 0 ? 'text-red-600' : 'text-green-600'}`}>
                  {slaRisk > 0 ? slaRisk : totalActive}
                </p>
                <p className="text-xs text-gray-400 mt-0.5">
                  {slaRisk > 0 ? 'с нарушениями SLA' : 'активных задач'}
                </p>
              </div>
            </div>
          )}

          {/* Легенда алгоритма */}
          <div className="bg-blue-50 border border-blue-100 rounded-xl px-4 py-3 mb-5 flex items-start gap-3">
            <svg className="w-4 h-4 text-blue-500 shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <p className="text-xs text-blue-700 leading-relaxed">
              <span className="font-semibold">Балл эффективности</span> = 50% скорость закрытия + 50% соблюдение SLA.
              Если метрик пока нет — учитывается только процент закрытых эскалаций.
              Клик на строку раскрывает детали.
            </p>
          </div>

          {/* Search */}
          <div className="relative mb-4">
            <svg className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
            <input
              type="text"
              value={search}
              onChange={e => setSearch(e.target.value)}
              placeholder="Поиск по имени…"
              className="w-full pl-9 pr-4 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
            />
          </div>

          {/* Error */}
          {error && <p className="text-sm text-gray-400 text-center py-16">{error}</p>}

          {/* List */}
          <div className="space-y-2">
            {loading
              ? Array.from({ length: 5 }).map((_, i) => <RowSkeleton key={i} />)
              : filtered.map((s, i) => (
                  <UserRow
                    key={s.userId}
                    rank={i + 1}
                    stats={s}
                    profile={profiles.get(s.userId)}
                  />
                ))
            }
          </div>

          {!loading && !error && filtered.length === 0 && (
            <div className="text-center py-20">
              <p className="text-gray-400 text-sm">
                {search ? 'Никого не нашли' : 'Нет данных — закройте первые эскалации, чтобы появилась статистика'}
              </p>
            </div>
          )}

        </div>
      </main>
    </div>
  )
}
