import axios, { AxiosError } from 'axios'
import { ApiError } from './auth'
import { PagedResult } from './projects'

const api = axios.create({
  baseURL: 'http://localhost:5279',
  withCredentials: true,
})

function extractError(err: unknown): ApiError {
  const e = err as AxiosError<ApiError>
  if (e.response?.data) return e.response.data
  return { title: 'UNKNOWN', detail: 'Произошла неизвестная ошибка', status: 0 }
}

// Бэкенд возвращает snake_case через SnakeCaseEnumConverter
export type EscalationStatus =
  | 'new' | 'in_progress' | 'waiting_for_response'
  | 'resolved' | 'closed' | 'cancelled'

export type EscalationPriority = 'low' | 'medium' | 'high' | 'critical' | 'blocker'
export type EscalationType = 'incident' | 'service_request' | 'bug' | 'feature_request'

export interface EscalationSummary {
  id: number
  name: string
  status: EscalationStatus
  priority: EscalationPriority
  type: EscalationType
  authorId: string       // Guid
  slaPolicyId: number
  slaPolicyName: string
  projectId: number
  projectName: string
  closedAt?: string
  createdAt: string
  updatedAt: string
}

export type ParticipantRole = 'assignee' | 'observer' | 'approver'

export interface EscalationFilter {
  projectId?: number
  authorId?: string
  responsibleUserId?: string
  responsibleUserRole?: ParticipantRole
  status?: EscalationStatus
  priority?: EscalationPriority
}

export interface CreateEscalationDto {
  name: string
  description?: string
  priority: EscalationPriority
  type: EscalationType
  authorId: string          // Guid
  slaPolicyId: number
  projectId: number
  participants: Array<{ userId: string; role: 'assignee' | 'observer' }>
  criteria?: Array<{ name: string; description?: string }>
}

export const STATUS_LABELS: Record<EscalationStatus, string> = {
  new:                  'Новая',
  in_progress:          'В работе',
  waiting_for_response: 'Ожидает ответа',
  resolved:             'Решена',
  closed:               'Закрыта',
  cancelled:            'Отменена',
}

export const STATUS_COLORS: Record<EscalationStatus, string> = {
  new:                  'bg-gray-100 text-gray-600',
  in_progress:          'bg-blue-100 text-blue-700',
  waiting_for_response: 'bg-yellow-100 text-yellow-700',
  resolved:             'bg-green-100 text-green-700',
  closed:               'bg-gray-100 text-gray-500',
  cancelled:            'bg-red-100 text-red-600',
}

export const PRIORITY_LABELS: Record<EscalationPriority, string> = {
  low:      'Низкий',
  medium:   'Средний',
  high:     'Высокий',
  critical: 'Критический',
  blocker:  'Блокер',
}

export const PRIORITY_COLORS: Record<EscalationPriority, string> = {
  low:      'bg-gray-100 text-gray-500',
  medium:   'bg-blue-100 text-blue-600',
  high:     'bg-orange-100 text-orange-600',
  critical: 'bg-red-100 text-red-600',
  blocker:  'bg-purple-100 text-purple-700',
}

export const TYPE_LABELS: Record<EscalationType, string> = {
  incident:        'Инцидент',
  service_request: 'Запрос на обслуживание',
  bug:             'Баг',
  feature_request: 'Запрос функции',
}

// ASP.NET Core enum binding ожидает PascalCase в query-параметрах (in_progress → InProgress)
function toPascalCase(value: string): string {
  return value.split('_').map(w => w.charAt(0).toUpperCase() + w.slice(1)).join('')
}

export async function getEscalations(
  filter?: EscalationFilter,
  page = 1,
  pageSize = 50,
): Promise<PagedResult<EscalationSummary>> {
  try {
    const params: Record<string, string | number> = {
      'pageParams.page': page,
      'pageParams.pageSize': pageSize,
    }
    if (filter?.projectId)           params['filter.projectId'] = filter.projectId
    if (filter?.authorId)            params['filter.authorId'] = filter.authorId
    if (filter?.responsibleUserId)   params['filter.responsibleUserId'] = filter.responsibleUserId
    if (filter?.responsibleUserRole) params['filter.responsibleUserRole'] = toPascalCase(filter.responsibleUserRole)
    if (filter?.status)              params['filter.status'] = toPascalCase(filter.status)
    if (filter?.priority)            params['filter.priority'] = toPascalCase(filter.priority)

    const res = await api.get<PagedResult<EscalationSummary>>(
      '/escalation-service/api/v1/escalation',
      { params },
    )
    return res.data
  } catch (err) {
    throw extractError(err)
  }
}

export type CommentType = 'user' | 'system' | 'note'

export interface CommentItem {
  id: number
  text: string
  isSystem: boolean
  type: CommentType
  userId: string
  createdAt: string
}

export interface CriterionItem {
  id: number
  name: string
  description?: string
  isCompleted: boolean
}

export interface ParticipantItem {
  userId: string
  role: ParticipantRole
  assignedAt: string
}

export interface EscalationMetricsItem {
  firstResponseTimeMinutes: number
  resolutionTimeMinutes: number
  timeToCloseMinutes: number
  slaViolated: boolean
  totalComments: number
  totalParticipants: number
}

export interface EscalationDetail {
  id: number
  name: string
  description?: string
  status: EscalationStatus
  priority: EscalationPriority
  type: EscalationType
  authorId: string
  slaPolicyId: number
  slaPolicyName: string
  projectId: number
  projectName: string
  firstResponseAt?: string
  resolvedAt?: string
  closedAt?: string
  participants: ParticipantItem[]
  criteria: CriterionItem[]
  comments: CommentItem[]
  metrics?: EscalationMetricsItem
  createdAt: string
  updatedAt: string
}

export interface SlaPolicy {
  id: number
  name: string
  description?: string
  isActive: boolean
  responseTimeMinutes: number
  resolutionTimeMinutes: number
}

export async function getSlaPolicies(): Promise<SlaPolicy[]> {
  try {
    const res = await api.get<PagedResult<SlaPolicy>>(
      '/escalation-service/api/v1/sla-policy',
      { params: { 'pageParams.page': 1, 'pageParams.pageSize': 100 } },
    )
    return res.data.items
  } catch (err) {
    throw extractError(err)
  }
}

export interface CriterionUpdateItem {
  name: string
  description?: string
  isCompleted: boolean
}

export interface UpdateEscalationDto {
  name: string
  description?: string
  priority: EscalationPriority
  status: EscalationStatus
  type: EscalationType
  firstResponseAt?: string
  resolvedAt?: string
  participants?: Array<{ userId: string; role: 'assignee' | 'observer' }>
  criteria?: CriterionUpdateItem[]
}

export async function updateEscalation(id: number, dto: UpdateEscalationDto): Promise<void> {
  try {
    await api.put(`/escalation-service/api/v1/escalation/${id}`, dto)
  } catch (err) {
    throw extractError(err)
  }
}

export async function getEscalationById(id: number): Promise<EscalationDetail> {
  try {
    const res = await api.get<EscalationDetail>(
      `/escalation-service/api/v1/escalation/${id}`,
    )
    return res.data
  } catch (err) {
    throw extractError(err)
  }
}

export async function addComment(
  escalationId: number,
  text: string,
  authorId: string,
): Promise<void> {
  try {
    await api.post(
      `/escalation-service/api/v1/escalation/${escalationId}/comment`,
      { text, authorId },
    )
  } catch (err) {
    throw extractError(err)
  }
}

export interface UserStatsItem {
  userId: string
  total: number
  active: number
  closed: number
  cancelled: number
  closedWithMetrics: number
  avgFirstResponseMinutes: number | null
  avgResolutionMinutes: number | null
  slaViolated: number
  slaCompliant: number
  completionPercent: number   // closed / (total - cancelled) * 100
  efficiencyScore: number     // 0-100: 50% completion + 50% SLA compliance
}

export async function getUsersStats(): Promise<UserStatsItem[]> {
  try {
    const res = await api.get<UserStatsItem[]>(
      '/escalation-service/api/v1/escalation/users-stats',
    )
    return res.data
  } catch (err) {
    throw extractError(err)
  }
}

export async function createEscalation(dto: CreateEscalationDto): Promise<void> {
  try {
    await api.post('/escalation-service/api/v1/escalation', {
      name: dto.name,
      description: dto.description || null,
      priority: dto.priority,
      type: dto.type,
      authorId: dto.authorId,
      slaPolicyId: dto.slaPolicyId,
      projectId: dto.projectId,
      participants: dto.participants.map(p => ({ userId: p.userId, role: p.role })),
      criteria: (dto.criteria ?? []).map(c => ({ name: c.name, description: c.description || null })),
    })
  } catch (err) {
    throw extractError(err)
  }
}
