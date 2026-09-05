import { useEffect, useState } from 'react'
import { useParams, useNavigate, Link } from 'react-router-dom'
import Navbar from '../components/Navbar'
import { getProjectById, ProjectResponse, PROJECT_ROLE_LABELS } from '../api/projects'
import {
  getEscalations, EscalationSummary,
  STATUS_LABELS, STATUS_COLORS,
  PRIORITY_LABELS, PRIORITY_COLORS,
  TYPE_LABELS,
} from '../api/escalations'
import { PagedResult } from '../api/projects'
import { getUsers, UserProfile, WORK_TYPE_LABELS } from '../api/users'
import CreateEscalationModal from '../components/CreateEscalationModal'
import { useAuth } from '../context/AuthContext'

type Tab = 'all' | 'assignee' | 'author' | 'observer'

const TABS: { key: Tab; label: string }[] = [
  { key: 'all',      label: 'Все эскалации' },
  { key: 'assignee', label: 'Я назначен' },
  { key: 'author',   label: 'Я создал' },
  { key: 'observer', label: 'Я наблюдатель' },
]

function EscalationRow({ e }: { e: EscalationSummary }) {
  const date = new Date(e.createdAt).toLocaleDateString('ru-RU', {
    day: 'numeric', month: 'short',
  })
  return (
    <Link
      to={`/escalations/${e.id}`}
      className="flex items-center gap-4 px-5 py-3.5 hover:bg-blue-50/60 transition-colors border-b border-gray-100 last:border-0 group"
    >
      <div className="flex-1 min-w-0">
        <p className="text-sm font-medium text-gray-900 truncate group-hover:text-blue-700 transition-colors">
          {e.name}
        </p>
        <p className="text-xs text-gray-400 mt-0.5">{e.slaPolicyName} · {TYPE_LABELS[e.type]}</p>
      </div>

      <span className={`shrink-0 text-xs font-medium px-2 py-0.5 rounded-full ${PRIORITY_COLORS[e.priority]}`}>
        {PRIORITY_LABELS[e.priority]}
      </span>

      <span className={`shrink-0 text-xs font-medium px-2 py-0.5 rounded-full ${STATUS_COLORS[e.status]}`}>
        {STATUS_LABELS[e.status]}
      </span>

      <span className="shrink-0 text-xs text-gray-400 w-20 text-right">{date}</span>

      <svg className="w-4 h-4 text-gray-300 group-hover:text-blue-400 shrink-0 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
      </svg>
    </Link>
  )
}

function EscalationRowSkeleton() {
  return (
    <div className="flex items-center gap-4 px-5 py-3.5 border-b border-gray-100 animate-pulse">
      <div className="flex-1">
        <div className="h-4 bg-gray-200 rounded w-2/3 mb-1.5" />
        <div className="h-3 bg-gray-100 rounded w-1/3" />
      </div>
      <div className="h-5 w-16 bg-gray-100 rounded-full" />
      <div className="h-5 w-20 bg-gray-100 rounded-full" />
      <div className="h-3 w-14 bg-gray-100 rounded" />
    </div>
  )
}

export default function ProjectDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { user } = useAuth()
  const projectId = Number(id)

  const [project, setProject] = useState<ProjectResponse | null>(null)
  const [projectLoading, setProjectLoading] = useState(true)
  const [usersMap, setUsersMap] = useState<Map<string, UserProfile>>(new Map())

  const [tab, setTab] = useState<Tab>('all')
  const [escalations, setEscalations] = useState<PagedResult<EscalationSummary> | null>(null)
  const [escLoading, setEscLoading] = useState(true)
  const [escError, setEscError] = useState<string | null>(null)
  const [showCreateModal, setShowCreateModal] = useState(false)

  // Загружаем проект и пользователей параллельно
  useEffect(() => {
    if (!projectId) return

    Promise.all([
      getProjectById(projectId),
      getUsers(1, 200),
    ])
      .then(([proj, usersPage]) => {
        setProject(proj)
        const map = new Map<string, UserProfile>()
        usersPage.items.forEach(u => map.set(u.id, u))
        setUsersMap(map)
      })
      .catch(() => navigate('/projects'))
      .finally(() => setProjectLoading(false))
  }, [projectId])

  // Загружаем эскалации при смене таба
  useEffect(() => {
    if (!projectId) return
    setEscLoading(true)
    setEscError(null)

    const filter = {
      projectId,
      ...(tab === 'author'   && user ? { authorId: user.id } : {}),
      ...(tab === 'assignee' && user ? { responsibleUserId: user.id, responsibleUserRole: 'assignee' as const } : {}),
      ...(tab === 'observer' && user ? { responsibleUserId: user.id, responsibleUserRole: 'observer' as const } : {}),
    }

    getEscalations(filter)
      .then(setEscalations)
      .catch(() => setEscError('Не удалось загрузить эскалации.'))
      .finally(() => setEscLoading(false))
  }, [projectId, tab, user])

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />

      <main className="pt-14">
        <div className="max-w-5xl mx-auto px-6 py-8">

          {/* Back */}
          <button
            onClick={() => navigate('/projects')}
            className="flex items-center gap-1.5 text-sm text-gray-500 hover:text-gray-800 mb-6 transition-colors"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
            </svg>
            Все проекты
          </button>

          {/* Project header */}
          {projectLoading ? (
            <div className="bg-white rounded-xl border border-gray-200 p-6 mb-6 animate-pulse">
              <div className="h-6 bg-gray-200 rounded w-1/3 mb-3" />
              <div className="h-4 bg-gray-100 rounded w-2/3" />
            </div>
          ) : project && (
            <div className="bg-white rounded-xl border border-gray-200 p-6 mb-6">
              <div className="flex items-start justify-between gap-4 mb-4">
                <div className="flex items-center gap-3">
                  <span className="text-xs font-mono font-semibold px-2.5 py-1 bg-blue-50 text-blue-700 rounded-md">
                    {project.code}
                  </span>
                  <h1 className="text-xl font-bold text-gray-900">{project.name}</h1>
                </div>
                <span className={`shrink-0 flex items-center gap-1.5 text-xs font-medium px-2.5 py-1 rounded-full ${
                  project.isActive ? 'bg-green-50 text-green-700' : 'bg-gray-100 text-gray-500'
                }`}>
                  <span className={`w-1.5 h-1.5 rounded-full ${project.isActive ? 'bg-green-500' : 'bg-gray-400'}`} />
                  {project.isActive ? 'Активен' : 'Архив'}
                </span>
              </div>

              {project.description && (
                <p className="text-sm text-gray-600 mb-5">{project.description}</p>
              )}

              {/* Owners */}
              {(project.owners?.length ?? 0) > 0 && (
                <div>
                  <p className="text-xs font-medium text-gray-400 uppercase tracking-wide mb-2">
                    Руководство проекта
                  </p>
                  <div className="flex flex-wrap gap-2">
                    {project.owners?.map((o) => {
                      const u = usersMap.get(o.userId)
                      const initials = u
                        ? `${u.firstName[0]}${u.lastName[0]}`.toUpperCase()
                        : '?'
                      const name = u ? `${u.firstName} ${u.lastName}` : o.userId.slice(0, 8) + '…'
                      const subtitle = u
                        ? (WORK_TYPE_LABELS[u.workType] ?? '—')
                        : PROJECT_ROLE_LABELS[o.role] ?? 'Участник'
                      return (
                        <div
                          key={o.userId}
                          className="flex items-center gap-2 bg-gray-50 border border-gray-200 rounded-lg px-3 py-1.5"
                        >
                          <div className="w-7 h-7 rounded-full bg-blue-600 flex items-center justify-center text-white text-xs font-bold shrink-0">
                            {initials}
                          </div>
                          <div>
                            <p className="text-xs font-medium text-gray-800">{name}</p>
                            <p className="text-xs text-gray-400">
                              {PROJECT_ROLE_LABELS[o.role] ?? 'Участник'} · {subtitle}
                            </p>
                          </div>
                        </div>
                      )
                    })}
                  </div>
                </div>
              )}
            </div>
          )}

          {/* Escalations */}
          <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
            {/* Tabs + Create button */}
            <div className="flex items-center border-b border-gray-200 px-5">
              {TABS.map((t) => (
                <button
                  key={t.key}
                  onClick={() => setTab(t.key)}
                  className={`py-3.5 px-1 mr-6 text-sm font-medium border-b-2 transition-colors ${
                    tab === t.key
                      ? 'border-blue-600 text-blue-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700'
                  }`}
                >
                  {t.label}
                  {tab === t.key && escalations && (
                    <span className="ml-2 text-xs bg-blue-50 text-blue-600 px-1.5 py-0.5 rounded-full font-normal">
                      {escalations.totalCount}
                    </span>
                  )}
                </button>
              ))}
              <div className="ml-auto pl-4 py-2.5">
                <button
                  onClick={() => setShowCreateModal(true)}
                  className="flex items-center gap-1.5 text-xs font-semibold text-white bg-blue-600 hover:bg-blue-700 px-3 py-1.5 rounded-lg transition-colors"
                >
                  <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                  </svg>
                  Новая эскалация
                </button>
              </div>
            </div>

            {/* Content */}
            {escError ? (
              <div className="py-16 text-center text-gray-400 text-sm">{escError}</div>
            ) : escLoading ? (
              Array.from({ length: 5 }).map((_, i) => <EscalationRowSkeleton key={i} />)
            ) : escalations?.items.length === 0 ? (
              <div className="py-16 text-center">
                <svg className="w-10 h-10 mx-auto mb-3 text-gray-200" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                    d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                </svg>
                <p className="text-sm text-gray-400">Эскалаций пока нет</p>
              </div>
            ) : (
              escalations?.items.map((e) => <EscalationRow key={e.id} e={e} />)
            )}
          </div>

        </div>
      </main>

      {showCreateModal && (
        <CreateEscalationModal
          projectId={projectId}
          onClose={() => setShowCreateModal(false)}
          onCreated={() => {
            setShowCreateModal(false)
            // Перезагружаем список эскалаций
            setEscLoading(true)
            getEscalations({ projectId })
              .then(setEscalations)
              .catch(() => setEscError('Не удалось загрузить эскалации.'))
              .finally(() => setEscLoading(false))
          }}
        />
      )}
    </div>
  )
}
