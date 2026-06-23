import axios, { AxiosError } from 'axios'
import { ApiError } from './auth'

const api = axios.create({
  baseURL: 'http://localhost:5279',
  withCredentials: true,
})

export interface ProjectOwnerResponse {
  userId: string   // Guid
  role: number     // 1=Owner, 2=Manager
  assignedAt: string
}

export const PROJECT_ROLE_LABELS: Record<number, string> = {
  1: 'Владелец',
  2: 'Менеджер',
}

export interface ProjectResponse {
  id: number
  name: string
  description?: string
  code: string
  isActive: boolean
  archivedAt?: string
  createdAt: string
  updatedAt: string
  owners: ProjectOwnerResponse[]
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

function extractError(err: unknown): ApiError {
  const axiosErr = err as AxiosError<ApiError>
  if (axiosErr.response?.data) return axiosErr.response.data
  return { title: 'UNKNOWN', detail: 'Произошла неизвестная ошибка', status: 0 }
}

export async function getProjectById(id: number): Promise<ProjectResponse> {
  try {
    const res = await api.get<ProjectResponse>(`/escalation-service/api/v1/project/${id}`)
    return res.data
  } catch (err) {
    throw extractError(err)
  }
}

export async function getProjects(page = 1, pageSize = 20): Promise<PagedResult<ProjectResponse>> {
  try {
    const res = await api.get<PagedResult<ProjectResponse>>(
      '/escalation-service/api/v1/project',
      { params: { 'pageParams.page': page, 'pageParams.pageSize': pageSize } },
    )
    return res.data
  } catch (err) {
    throw extractError(err)
  }
}

export interface ProjectImportResult {
  imported: number
  skippedDuplicateCodes: string[]
}

export async function importProjectsFromExcel(file: File): Promise<ProjectImportResult> {
  try {
    const form = new FormData()
    form.append('file', file)
    const res = await api.post<ProjectImportResult>(
      '/escalation-service/api/v1/project/import',
      form,
      { headers: { 'Content-Type': 'multipart/form-data' } },
    )
    return res.data
  } catch (err) {
    throw extractError(err)
  }
}
