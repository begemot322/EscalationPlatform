import axios, { AxiosError } from 'axios'
import { ApiError } from './auth'

const api = axios.create({
  baseURL: 'http://localhost:5279',
  withCredentials: true,
})

export type EscalationPriority = 'low' | 'medium' | 'high' | 'critical' | 'blocker'

export const PRIORITY_LABELS: Record<EscalationPriority, string> = {
  low: 'Низкий',
  medium: 'Средний',
  high: 'Высокий',
  critical: 'Критический',
  blocker: 'Блокер',
}

export const PRIORITY_COLORS: Record<EscalationPriority, string> = {
  low: 'bg-gray-100 text-gray-600',
  medium: 'bg-blue-50 text-blue-700',
  high: 'bg-yellow-50 text-yellow-700',
  critical: 'bg-orange-50 text-orange-700',
  blocker: 'bg-red-50 text-red-700',
}

export const PRIORITIES: EscalationPriority[] = ['low', 'medium', 'high', 'critical', 'blocker']

export interface SlaPolicyResponse {
  id: number
  name: string
  description?: string
  minPriorityLevel: EscalationPriority
  isActive: boolean
  responseTimeMinutes: number
  resolutionTimeMinutes: number
  createdAt: string
  updatedAt: string
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface CreateSlaPolicyRequest {
  name: string
  description?: string
  minPriorityLevel: EscalationPriority
  responseTimeMinutes: number
  resolutionTimeMinutes: number
}

export type UpdateSlaPolicyRequest = CreateSlaPolicyRequest

function extractError(err: unknown): ApiError {
  const axiosErr = err as AxiosError<ApiError>
  if (axiosErr.response?.data) return axiosErr.response.data
  return { title: 'UNKNOWN', detail: 'Произошла неизвестная ошибка', status: 0 }
}

export async function getSlaPolicies(page = 1, pageSize = 50): Promise<PagedResult<SlaPolicyResponse>> {
  try {
    const res = await api.get<PagedResult<SlaPolicyResponse>>(
      '/escalation-service/api/v1/sla-policy',
      { params: { 'pageParams.page': page, 'pageParams.pageSize': pageSize } },
    )
    return res.data
  } catch (err) {
    throw extractError(err)
  }
}

export async function createSlaPolicy(request: CreateSlaPolicyRequest): Promise<void> {
  try {
    await api.post('/escalation-service/api/v1/sla-policy', request)
  } catch (err) {
    throw extractError(err)
  }
}

export async function updateSlaPolicy(id: number, request: UpdateSlaPolicyRequest): Promise<void> {
  try {
    await api.put(`/escalation-service/api/v1/sla-policy/${id}`, request)
  } catch (err) {
    throw extractError(err)
  }
}

export async function deleteSlaPolicy(id: number): Promise<void> {
  try {
    await api.delete(`/escalation-service/api/v1/sla-policy/${id}`)
  } catch (err) {
    throw extractError(err)
  }
}
