import { useEffect, useRef, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import Navbar from '../components/Navbar'
import {
  getEscalationById, addComment, updateEscalation,
  EscalationDetail, CommentItem, CriterionItem, EscalationStatus,
  STATUS_LABELS, STATUS_COLORS,
  PRIORITY_LABELS, PRIORITY_COLORS,
  TYPE_LABELS,
} from '../api/escalations'
import { getUsers, UserProfile, WORK_TYPE_LABELS } from '../api/users'
import { useAuth } from '../context/AuthContext'
import { ApiError } from '../api/auth'
import EditEscalationModal from '../components/EditEscalationModal'

const PARTICIPANT_ROLE_LABELS: Record<string, string> = {
  assignee: 'Исполнитель',
  observer: 'Наблюдатель',
  approver: 'Согласующий',
}

// ── Скелетон ─────────────────────────────────────────────────────────────────
function Skeleton({ className }: { className: string }) {
  return <div className={`bg-gray-200 rounded animate-pulse ${className}`} />
}

// ── Аватар пользователя ───────────────────────────────────────────────────────
function UserAvatar({ user, size = 'md' }: { user?: UserProfile; size?: 'sm' | 'md' }) {
  const initials = user
    ? `${user.firstName[0]}${user.lastName[0]}`.toUpperCase()
    : '?'
  const sz = size === 'sm' ? 'w-7 h-7 text-xs' : 'w-8 h-8 text-sm'
  return (
    <div className={`${sz} rounded-full bg-blue-600 flex items-center justify-center text-white font-bold shrink-0`}>
      {initials}
    </div>
  )
}

// ── Один комментарий ─────────────────────────────────────────────────────────
function CommentBubble({ comment, usersMap }: { comment: CommentItem; usersMap: Map<string, UserProfile> }) {
  const author = usersMap.get(comment.userId)
  const name = author ? `${author.firstName} ${author.lastName}` : 'Неизвестный'
  const time = new Date(comment.createdAt).toLocaleString('ru-RU', {
    day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit',
  })

  if (comment.isSystem) {
    return (
      <div className="flex items-center gap-2 py-1.5">
        <div className="flex-1 h-px bg-gray-100" />
        <span className="text-xs text-gray-400 shrink-0">{comment.text}</span>
        <div className="flex-1 h-px bg-gray-100" />
      </div>
    )
  }

  return (
    <div className="flex gap-3">
      <UserAvatar user={author} size="sm" />
      <div className="flex-1 min-w-0">
        <div className="flex items-baseline gap-2 mb-1">
          <span className="text-xs font-semibold text-gray-800">{name}</span>
          <span className="text-xs text-gray-400">{time}</span>
        </div>
        <div className="bg-gray-50 border border-gray-100 rounded-xl rounded-tl-none px-3.5 py-2.5">
          <p className="text-sm text-gray-700 whitespace-pre-wrap break-words">{comment.text}</p>
        </div>
      </div>
    </div>
  )
}

export default function EscalationDetailPage() {
  const { escalationId } = useParams<{ escalationId: string }>()
  const navigate = useNavigate()
  const { user } = useAuth()
  const id = Number(escalationId)

  const [escalation, setEscalation] = useState<EscalationDetail | null>(null)
  const [usersMap, setUsersMap] = useState<Map<string, UserProfile>>(new Map())
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Локальный стейт критериев для мгновенного переключения галочек
  const [criteria, setCriteria] = useState<CriterionItem[]>([])
  useEffect(() => { if (escalation) setCriteria(escalation.criteria) }, [escalation])

  const [showEditModal, setShowEditModal] = useState(false)
  const [closing, setClosing] = useState(false)

  const [statusDropdownOpen, setStatusDropdownOpen] = useState(false)
  const [statusChanging, setStatusChanging] = useState(false)
  const statusDropRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    function handle(e: MouseEvent) {
      if (statusDropRef.current && !statusDropRef.current.contains(e.target as Node))
        setStatusDropdownOpen(false)
    }
    document.addEventListener('mousedown', handle)
    return () => document.removeEventListener('mousedown', handle)
  }, [])

  const [commentText, setCommentText] = useState('')
  const [commentLoading, setCommentLoading] = useState(false)
  const [commentError, setCommentError] = useState<string | null>(null)
  const commentsBottomRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!id) return

    Promise.all([
      getEscalationById(id),
      getUsers(1, 200),
    ])
      .then(([esc, usersPage]) => {
        setEscalation(esc)
        const map = new Map<string, UserProfile>()
        usersPage.items.forEach(u => map.set(u.id, u))
        setUsersMap(map)
      })
      .catch(() => setError('Не удалось загрузить эскалацию.'))
      .finally(() => setLoading(false))
  }, [id])

  // Скролл к последнему комментарию после загрузки
  useEffect(() => {
    if (escalation?.comments.length) {
      commentsBottomRef.current?.scrollIntoView({ behavior: 'smooth' })
    }
  }, [escalation?.comments.length])

  const isAuthor = !!user && !!escalation && user.id === escalation.authorId
  const isAssignee = !!user && !!escalation
    && escalation.participants.some(p => p.userId === user.id && p.role === 'assignee')

  // Статусы, доступные исполнителю (не автору)
  const ASSIGNEE_STATUSES: EscalationStatus[] = ['in_progress', 'waiting_for_response', 'resolved']

  async function handleStatusChange(newStatus: EscalationStatus) {
    if (!escalation || statusChanging) return
    setStatusDropdownOpen(false)
    setStatusChanging(true)
    try {
      await updateEscalation(escalation.id, {
        name: escalation.name,
        description: escalation.description,
        priority: escalation.priority,
        status: newStatus,
        type: escalation.type,
        firstResponseAt: escalation.firstResponseAt,
        resolvedAt: escalation.resolvedAt,
        participants: escalation.participants
          .filter(p => p.role !== 'approver')
          .map(p => ({ userId: p.userId, role: p.role as 'assignee' | 'observer' })),
        criteria: criteria.map(c => ({
          name: c.name, description: c.description, isCompleted: c.isCompleted,
        })),
      })
      const fresh = await getEscalationById(escalation.id)
      setEscalation(fresh)
    } finally {
      setStatusChanging(false)
    }
  }

  async function toggleCriterion(idx: number) {
    if (!escalation) return
    const prev = criteria
    const updated = criteria.map((c, i) => i === idx ? { ...c, isCompleted: !c.isCompleted } : c)
    setCriteria(updated) // оптимистично
    try {
      await updateEscalation(escalation.id, {
        name: escalation.name,
        description: escalation.description,
        priority: escalation.priority,
        status: escalation.status,
        type: escalation.type,
        firstResponseAt: escalation.firstResponseAt,
        resolvedAt: escalation.resolvedAt,
        participants: escalation.participants
          .filter(p => p.role !== 'approver')
          .map(p => ({ userId: p.userId, role: p.role as 'assignee' | 'observer' })),
        criteria: updated.map(c => ({
          name: c.name,
          description: c.description,
          isCompleted: c.isCompleted,
        })),
      })
    } catch {
      setCriteria(prev) // откат при ошибке
    }
  }

  async function handleCloseEscalation() {
    if (!escalation || closing) return
    setClosing(true)
    try {
      const now = new Date().toISOString()
      await updateEscalation(escalation.id, {
        name: escalation.name,
        description: escalation.description,
        priority: escalation.priority,
        status: 'closed',
        type: escalation.type,
        firstResponseAt: escalation.firstResponseAt,
        resolvedAt: escalation.resolvedAt ?? now,
        participants: escalation.participants
          .filter(p => p.role !== 'approver')
          .map(p => ({ userId: p.userId, role: p.role as 'assignee' | 'observer' })),
        criteria: criteria.map(c => ({
          name: c.name,
          description: c.description,
          isCompleted: c.isCompleted,
        })),
      })
      const fresh = await getEscalationById(escalation.id)
      setEscalation(fresh)
    } finally {
      setClosing(false)
    }
  }

  async function handleSendComment() {
    if (!commentText.trim() || !user) return
    setCommentLoading(true)
    setCommentError(null)
    try {
      await addComment(id, commentText.trim(), user.id)
      // Перезагружаем эскалацию чтобы получить новый комментарий
      const updated = await getEscalationById(id)
      setEscalation(updated)
      setCommentText('')
      setTimeout(() => commentsBottomRef.current?.scrollIntoView({ behavior: 'smooth' }), 50)
    } catch (err) {
      const apiErr = err as ApiError
      setCommentError(apiErr.detail ?? 'Не удалось отправить комментарий')
    } finally {
      setCommentLoading(false)
    }
  }

  // ── Скелетон загрузки ─────────────────────────────────────────────────────
  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50">
        <Navbar />
        <main className="pt-14">
          <div className="max-w-6xl mx-auto px-6 py-8 space-y-5">
            <Skeleton className="h-4 w-32" />
            <div className="bg-white rounded-xl border border-gray-200 p-6 space-y-4">
              <Skeleton className="h-7 w-1/2" />
              <Skeleton className="h-4 w-3/4" />
              <div className="flex gap-2">
                <Skeleton className="h-6 w-20 rounded-full" />
                <Skeleton className="h-6 w-24 rounded-full" />
                <Skeleton className="h-6 w-20 rounded-full" />
              </div>
            </div>
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">
              <div className="lg:col-span-2 space-y-5">
                <div className="bg-white rounded-xl border border-gray-200 p-6 space-y-3">
                  <Skeleton className="h-5 w-1/4" />
                  {[1,2,3].map(i => <Skeleton key={i} className="h-10 w-full rounded-lg" />)}
                </div>
              </div>
              <div className="bg-white rounded-xl border border-gray-200 p-6 space-y-3">
                <Skeleton className="h-5 w-1/3" />
                {[1,2,3,4].map(i => <Skeleton key={i} className="h-14 w-full rounded-lg" />)}
              </div>
            </div>
          </div>
        </main>
      </div>
    )
  }

  if (error || !escalation) {
    return (
      <div className="min-h-screen bg-gray-50">
        <Navbar />
        <main className="pt-14 flex items-center justify-center">
          <div className="text-center py-20">
            <p className="text-gray-400 text-sm mb-4">{error ?? 'Эскалация не найдена'}</p>
            <button
              onClick={() => navigate(-1)}
              className="text-sm text-blue-600 hover:underline"
            >
              Назад
            </button>
          </div>
        </main>
      </div>
    )
  }

  const createdDate = new Date(escalation.createdAt).toLocaleDateString('ru-RU', {
    day: 'numeric', month: 'long', year: 'numeric',
  })

  const completedCriteria = criteria.filter(c => c.isCompleted).length
  const totalCriteria = criteria.length

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />

      <main className="pt-14">
        <div className="max-w-6xl mx-auto px-6 py-8">

          {/* Back */}
          <button
            onClick={() => navigate(`/projects/${escalation.projectId}`)}
            className="flex items-center gap-1.5 text-sm text-gray-500 hover:text-gray-800 mb-6 transition-colors"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
            </svg>
            {escalation.projectName}
          </button>

          {/* Header card */}
          <div className="bg-white rounded-xl border border-gray-200 p-6 mb-5">
            <div className="flex items-start justify-between gap-4 mb-3">
              <h1 className="text-xl font-bold text-gray-900 leading-snug">{escalation.name}</h1>
              <div className="flex items-center gap-2 shrink-0">
                {/* Статус — для исполнителя (не автора) превращается в дропдаун */}
                {isAssignee && !isAuthor && !['closed', 'cancelled'].includes(escalation.status)
                  ? (
                    <div className="relative" ref={statusDropRef}>
                      <button
                        onClick={() => setStatusDropdownOpen(v => !v)}
                        disabled={statusChanging}
                        className={`flex items-center gap-1.5 text-xs font-semibold px-2.5 py-1 rounded-full transition-colors ${STATUS_COLORS[escalation.status]} hover:opacity-80`}
                      >
                        {statusChanging
                          ? <div className="w-3 h-3 border-2 border-current border-t-transparent rounded-full animate-spin" />
                          : null
                        }
                        {STATUS_LABELS[escalation.status]}
                        <svg className={`w-3 h-3 transition-transform ${statusDropdownOpen ? 'rotate-180' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                        </svg>
                      </button>
                      {statusDropdownOpen && (
                        <div className="absolute right-0 mt-1.5 w-48 bg-white rounded-xl shadow-lg border border-gray-100 py-1 z-20">
                          <p className="px-3 py-1.5 text-xs text-gray-400 font-medium">Сменить статус</p>
                          {ASSIGNEE_STATUSES.filter(s => s !== escalation.status).map(s => (
                            <button
                              key={s}
                              onClick={() => handleStatusChange(s)}
                              className="w-full text-left px-3 py-2 text-sm text-gray-700 hover:bg-gray-50 flex items-center gap-2 transition-colors"
                            >
                              <span className={`w-2 h-2 rounded-full shrink-0 ${STATUS_COLORS[s].split(' ')[0]}`} />
                              {STATUS_LABELS[s]}
                            </button>
                          ))}
                        </div>
                      )}
                    </div>
                  ) : (
                    <span className={`text-xs font-semibold px-2.5 py-1 rounded-full ${STATUS_COLORS[escalation.status]}`}>
                      {STATUS_LABELS[escalation.status]}
                    </span>
                  )
                }

                {isAuthor && (
                  <button
                    onClick={() => setShowEditModal(true)}
                    className="flex items-center gap-1.5 text-xs font-semibold text-gray-600 hover:text-gray-900 bg-gray-100 hover:bg-gray-200 px-3 py-1 rounded-lg transition-colors"
                  >
                    <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                        d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
                    </svg>
                    Редактировать
                  </button>
                )}
              </div>
            </div>

            {escalation.description && (
              <p className="text-sm text-gray-600 mb-4 leading-relaxed whitespace-pre-wrap break-words">{escalation.description}</p>
            )}

            {/* Meta row */}
            <div className="flex flex-wrap items-center gap-2">
              <span className={`text-xs font-medium px-2.5 py-1 rounded-full ${PRIORITY_COLORS[escalation.priority]}`}>
                {PRIORITY_LABELS[escalation.priority]}
              </span>
              <span className="text-xs font-medium px-2.5 py-1 rounded-full bg-purple-50 text-purple-700">
                {TYPE_LABELS[escalation.type]}
              </span>
              <span className="text-xs px-2.5 py-1 rounded-full bg-gray-100 text-gray-600">
                SLA: {escalation.slaPolicyName}
              </span>
              <span className="text-xs text-gray-400 ml-auto">{createdDate}</span>
            </div>
          </div>

          {/* Main grid */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">

            {/* Left: criteria + participants + metrics */}
            <div className="lg:col-span-2 space-y-5">

              {/* Criteria */}
              {totalCriteria > 0 && (
                <div className="bg-white rounded-xl border border-gray-200 p-5">
                  <div className="flex items-center justify-between mb-4">
                    <h2 className="text-sm font-semibold text-gray-800">Критерии выполнения</h2>
                    <span className="text-xs text-gray-400">
                      {completedCriteria} / {totalCriteria}
                    </span>
                  </div>

                  {/* Progress bar */}
                  {totalCriteria > 0 && (
                    <div className="h-1.5 bg-gray-100 rounded-full mb-4 overflow-hidden">
                      <div
                        className="h-full bg-blue-500 rounded-full transition-all"
                        style={{ width: `${(completedCriteria / totalCriteria) * 100}%` }}
                      />
                    </div>
                  )}

                  {/* Баннер закрытия — показывается только автору когда все галочки стоят */}
                  {isAuthor && totalCriteria > 0 && completedCriteria === totalCriteria &&
                    escalation.status !== 'closed' && escalation.status !== 'cancelled' && (
                    <div className="mb-4 bg-green-50 border border-green-200 rounded-xl px-4 py-3.5 flex items-center justify-between gap-3">
                      <div className="flex items-center gap-3 min-w-0">
                        <div className="w-8 h-8 bg-green-100 rounded-full flex items-center justify-center shrink-0">
                          <svg className="w-4 h-4 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M5 13l4 4L19 7" />
                          </svg>
                        </div>
                        <div className="min-w-0">
                          <p className="text-sm font-semibold text-green-800">Все критерии выполнены!</p>
                          <p className="text-xs text-green-600 mt-0.5">
                            {escalation.status === 'resolved'
                              ? 'Эскалация уже решена — можете закрыть её.'
                              : 'Хотите закрыть эскалацию?'}
                          </p>
                        </div>
                      </div>
                      <button
                        onClick={handleCloseEscalation}
                        disabled={closing}
                        className="shrink-0 flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-white bg-green-600 hover:bg-green-700 disabled:opacity-60 rounded-lg transition-colors"
                      >
                        {closing && (
                          <div className="w-3.5 h-3.5 border-2 border-white border-t-transparent rounded-full animate-spin" />
                        )}
                        Закрыть эскалацию
                      </button>
                    </div>
                  )}

                  <div className="space-y-2">
                    {criteria.map((c, idx) => (
                      <div
                        key={c.id}
                        className={`flex items-start gap-3 p-3 rounded-lg border border-gray-100 transition-colors ${
                          isAuthor ? 'cursor-pointer hover:bg-blue-50 hover:border-blue-100' : 'bg-gray-50'
                        }`}
                        onClick={() => isAuthor && toggleCriterion(idx)}
                      >
                        <div className={`w-4 h-4 rounded shrink-0 flex items-center justify-center border-2 mt-0.5 transition-colors ${
                          c.isCompleted
                            ? 'bg-green-500 border-green-500'
                            : isAuthor
                              ? 'border-gray-300 bg-white group-hover:border-blue-400'
                              : 'border-gray-300 bg-white'
                        }`}>
                          {c.isCompleted && (
                            <svg className="w-2.5 h-2.5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M5 13l4 4L19 7" />
                            </svg>
                          )}
                        </div>
                        <div className="flex-1 min-w-0">
                          <p className={`text-sm select-none ${c.isCompleted ? 'line-through text-gray-400' : 'text-gray-700'}`}>
                            {c.name}
                          </p>
                          {c.description && (
                            <p className="text-xs text-gray-400 mt-0.5 break-words select-none">{c.description}</p>
                          )}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {/* Participants */}
              {escalation.participants.length > 0 && (
                <div className="bg-white rounded-xl border border-gray-200 p-5">
                  <h2 className="text-sm font-semibold text-gray-800 mb-4">Участники</h2>
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                    {escalation.participants.map(p => {
                      const u = usersMap.get(p.userId)
                      const name = u ? `${u.firstName} ${u.lastName}` : p.userId.slice(0, 8) + '…'
                      return (
                        <div
                          key={`${p.userId}-${p.role}`}
                          className="flex items-center gap-2.5 bg-gray-50 border border-gray-100 rounded-lg px-3 py-2.5"
                        >
                          <UserAvatar user={u} size="sm" />
                          <div className="min-w-0">
                            <p className="text-xs font-medium text-gray-800 truncate">{name}</p>
                            <p className="text-xs text-gray-400">
                              {PARTICIPANT_ROLE_LABELS[p.role] ?? p.role}
                              {u && ` · ${WORK_TYPE_LABELS[u.workType] ?? '—'}`}
                            </p>
                          </div>
                        </div>
                      )
                    })}
                  </div>
                </div>
              )}

              {/* Metrics */}
              {escalation.metrics && (
                <div className="bg-white rounded-xl border border-gray-200 p-5">
                  <h2 className="text-sm font-semibold text-gray-800 mb-4">Метрики</h2>
                  <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
                    {[
                      { label: 'Время первого ответа', value: `${escalation.metrics.firstResponseTimeMinutes} мин` },
                      { label: 'Время решения', value: `${escalation.metrics.resolutionTimeMinutes} мин` },
                      { label: 'Время до закрытия', value: `${escalation.metrics.timeToCloseMinutes} мин` },
                      { label: 'Комментариев', value: String(escalation.metrics.totalComments) },
                      { label: 'Участников', value: String(escalation.metrics.totalParticipants) },
                      {
                        label: 'SLA',
                        value: escalation.metrics.slaViolated ? 'Нарушен' : 'Соблюден',
                        accent: escalation.metrics.slaViolated,
                      },
                    ].map(m => (
                      <div key={m.label} className="bg-gray-50 rounded-lg p-3">
                        <p className="text-xs text-gray-400 mb-1">{m.label}</p>
                        <p className={`text-sm font-semibold ${m.accent ? 'text-red-600' : 'text-gray-800'}`}>
                          {m.value}
                        </p>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>

            {/* Right: comments */}
            <div className="bg-white rounded-xl border border-gray-200 flex flex-col" style={{ minHeight: '480px' }}>
              <div className="px-5 py-4 border-b border-gray-100">
                <h2 className="text-sm font-semibold text-gray-800">
                  Комментарии
                  {escalation.comments.length > 0 && (
                    <span className="ml-2 text-xs bg-gray-100 text-gray-500 px-1.5 py-0.5 rounded-full font-normal">
                      {escalation.comments.length}
                    </span>
                  )}
                </h2>
              </div>

              {/* Scroll area */}
              <div className="flex-1 overflow-y-auto px-5 py-4 space-y-4">
                {escalation.comments.length === 0 ? (
                  <div className="flex flex-col items-center justify-center h-full py-10 text-center">
                    <svg className="w-8 h-8 text-gray-200 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                        d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
                    </svg>
                    <p className="text-xs text-gray-400">Комментариев пока нет</p>
                  </div>
                ) : (
                  escalation.comments.map(c => (
                    <CommentBubble key={c.id} comment={c} usersMap={usersMap} />
                  ))
                )}
                <div ref={commentsBottomRef} />
              </div>

              {/* Comment form */}
              <div className="px-5 py-4 border-t border-gray-100">
                {commentError && (
                  <p className="text-xs text-red-500 mb-2">{commentError}</p>
                )}
                <div className="flex gap-2 items-end">
                  <textarea
                    value={commentText}
                    onChange={e => setCommentText(e.target.value)}
                    onKeyDown={e => {
                      if (e.key === 'Enter' && !e.shiftKey) {
                        e.preventDefault()
                        handleSendComment()
                      }
                    }}
                    placeholder="Написать комментарий…"
                    rows={2}
                    className="flex-1 px-3 py-2 text-sm border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none"
                  />
                  <button
                    onClick={handleSendComment}
                    disabled={!commentText.trim() || commentLoading || !user}
                    className="shrink-0 w-9 h-9 flex items-center justify-center bg-blue-600 hover:bg-blue-700 disabled:opacity-40 disabled:cursor-not-allowed text-white rounded-lg transition-colors"
                    title="Отправить (Enter)"
                  >
                    {commentLoading ? (
                      <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                    ) : (
                      <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                          d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8" />
                      </svg>
                    )}
                  </button>
                </div>
                <p className="text-xs text-gray-400 mt-1.5">Enter — отправить · Shift+Enter — перенос</p>
              </div>
            </div>

          </div>
        </div>
      </main>

      {showEditModal && (
        <EditEscalationModal
          escalation={escalation}
          onClose={() => setShowEditModal(false)}
          onSaved={async () => {
            setShowEditModal(false)
            const updated = await getEscalationById(id)
            setEscalation(updated)
          }}
        />
      )}
    </div>
  )
}
