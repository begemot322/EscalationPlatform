import { createContext, useContext, useState, ReactNode, useCallback } from 'react'
import { getMe, UserProfile } from '../api/users'

export interface AuthUser {
  id: string
  email: string
  firstName: string
  lastName: string
  role: number
  workType: number
  profileImageFileName: string | null
  initials: string
}

interface AuthContextType {
  user: AuthUser | null
  loadUser: () => Promise<void>   // вызываем после логина — грузит /me
  clearUser: () => void
}

const AuthContext = createContext<AuthContextType | null>(null)

const STORAGE_KEY = 'ep_user'

function toAuthUser(profile: UserProfile): AuthUser {
  const initials =
    `${profile.firstName[0] ?? ''}${profile.lastName[0] ?? ''}`.toUpperCase() ||
    profile.email[0]?.toUpperCase() ||
    '?'
  return {
    id: profile.id,
    email: profile.email,
    firstName: profile.firstName,
    lastName: profile.lastName,
    role: profile.role,
    workType: profile.workType,
    profileImageFileName: profile.profileImageFileName,
    initials,
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(() => {
    try {
      const raw = localStorage.getItem(STORAGE_KEY)
      return raw ? (JSON.parse(raw) as AuthUser) : null
    } catch {
      return null
    }
  })

  const loadUser = useCallback(async () => {
    const profile = await getMe()
    const u = toAuthUser(profile)
    setUser(u)
    localStorage.setItem(STORAGE_KEY, JSON.stringify(u))
  }, [])

  const clearUser = useCallback(() => {
    setUser(null)
    localStorage.removeItem(STORAGE_KEY)
  }, [])

  return (
    <AuthContext.Provider value={{ user, loadUser, clearUser }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider')
  return ctx
}
