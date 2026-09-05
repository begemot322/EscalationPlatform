import axios, { AxiosError } from 'axios'

const api = axios.create({
  baseURL: 'http://localhost:5279',
  withCredentials: true,
})

export interface ApiError {
  title: string
  detail: string
  status: number
  errors?: string[]        // ASP.NET Core кладёт extensions на корневой уровень
  Errors?: string[]        // PascalCase вариант
  extensions?: {
    errors?: string[]
    Errors?: string[]
  }
}

export function getApiErrors(err: ApiError): string[] {
  return err.extensions?.errors ?? err.extensions?.Errors ?? []
}

function extractError(err: unknown): ApiError {
  const axiosErr = err as AxiosError<ApiError>
  if (axiosErr.response?.data) {
    return axiosErr.response.data
  }
  return { title: 'UNKNOWN', detail: 'Произошла неизвестная ошибка', status: 0 }
}

export interface RegisterRequest {
  firstName: string
  lastName: string
  email: string
  password: string
  confirmPassword: string
  role: number
  workType: number
}

export interface LoginRequest {
  email: string
  password: string
}

export interface OtpSentResponse {
  canResendAt: string
}

export interface VerifyOtpRequest {
  email: string
  code: string
}

export async function register(data: RegisterRequest): Promise<void> {
  try {
    await api.post('/user-service/api/v1/auth/register', data)
  } catch (err) {
    throw extractError(err)
  }
}

export async function login(data: LoginRequest): Promise<string> {
  try {
    const res = await api.post<{ token: string }>('/user-service/api/v1/auth/login', data)
    return res.data.token
  } catch (err) {
    throw extractError(err)
  }
}

export async function sendOtp(email: string): Promise<OtpSentResponse> {
  try {
    const res = await api.post<OtpSentResponse>(
      `/user-service/api/v1/otp/send?email=${encodeURIComponent(email)}`,
    )
    return res.data
  } catch (err) {
    throw extractError(err)
  }
}

export async function verifyOtp(data: VerifyOtpRequest): Promise<string> {
  try {
    const res = await api.post<{ token: string }>('/user-service/api/v1/otp/verify', data)
    return res.data.token
  } catch (err) {
    throw extractError(err)
  }
}

export async function logout(): Promise<void> {
  try {
    await api.post('/user-service/api/v1/auth/logout')
  } catch {
    // ignore
  }
}
