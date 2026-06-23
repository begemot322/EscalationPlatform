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

export interface UserProfile {
  id: string
  firstName: string
  lastName: string
  email: string
  role: number         // 1=Employee, 2=Manager, 3=Admin
  workType: number     // 1=TechLead … 7=Designer
  profileImageFileName: string | null
  isEmailVerified: boolean
  createdAt: string
}

export const ROLE_LABELS: Record<number, string> = {
  1: 'Сотрудник',
  2: 'Менеджер',
  3: 'Администратор',
}

export const WORK_TYPE_LABELS: Record<number, string> = {
  1: 'Tech Lead',
  2: 'Project Manager',
  3: 'Backend Developer',
  4: 'Frontend Developer',
  5: 'QA Engineer',
  6: 'DevOps',
  7: 'Дизайнер',
}

export async function getMe(): Promise<UserProfile> {
  try {
    const res = await api.get<UserProfile>('/user-service/api/v1/user/me')
    return res.data
  } catch (err) {
    throw extractError(err)
  }
}

export async function getUsers(page = 1, pageSize = 100): Promise<PagedResult<UserProfile>> {
  try {
    const res = await api.get<PagedResult<UserProfile>>('/user-service/api/v1/user', {
      params: { 'pageParams.page': page, 'pageParams.pageSize': pageSize },
    })
    return res.data
  } catch (err) {
    throw extractError(err)
  }
}
