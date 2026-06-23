import { useState, useRef, useEffect } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { logout } from '../api/auth'
import { ROLE_LABELS, WORK_TYPE_LABELS } from '../api/users'

const NAV_LINKS = [
  { to: '/projects',        label: 'Проекты' },
  { to: '/my-escalations',  label: 'Мои' },
  { to: '/assigned',        label: 'Назначенные' },
  { to: '/observed',        label: 'Наблюдаемые' },
]

export default function Navbar() {
  const { user, clearUser } = useAuth()
  const navigate = useNavigate()
  const [open, setOpen] = useState(false)
  const ref = useRef<HTMLDivElement>(null)

  useEffect(() => {
    function handle(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) setOpen(false)
    }
    document.addEventListener('mousedown', handle)
    return () => document.removeEventListener('mousedown', handle)
  }, [])

  async function handleLogout() {
    await logout()
    clearUser()
    navigate('/login')
  }

  return (
    <header className="fixed top-0 left-0 right-0 z-30 h-14 bg-white border-b border-gray-200 flex items-center px-6 gap-6">
      {/* Logo */}
      <button
        onClick={() => navigate('/')}
        className="font-bold text-gray-900 text-base tracking-tight hover:text-blue-600 transition-colors shrink-0"
      >
        WaveAccess
      </button>

      {/* Nav links — desktop */}
      {user && (
        <nav className="hidden md:flex items-center gap-1">
          {NAV_LINKS.map(link => (
            <NavLink
              key={link.to}
              to={link.to}
              className={({ isActive }) =>
                `px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${
                  isActive
                    ? 'bg-blue-50 text-blue-600'
                    : 'text-gray-500 hover:text-gray-800 hover:bg-gray-100'
                }`
              }
            >
              {link.label}
            </NavLink>
          ))}
        </nav>
      )}

      <div className="flex-1" />

      {/* Avatar + dropdown */}
      {user && (
        <div className="relative" ref={ref}>
          <button
            onClick={() => setOpen((v) => !v)}
            className="flex items-center gap-2.5 rounded-lg px-2 py-1.5 hover:bg-gray-100 transition-colors"
          >
            <div className="w-8 h-8 rounded-full bg-blue-600 flex items-center justify-center text-white text-xs font-bold select-none shrink-0">
              {user.initials}
            </div>

            <div className="text-left hidden sm:block">
              <p className="text-sm font-medium text-gray-900 leading-tight">
                {user.firstName} {user.lastName}
              </p>
              <p className="text-xs text-gray-400 leading-tight">
                {WORK_TYPE_LABELS[user.workType] ?? ROLE_LABELS[user.role] ?? '—'}
              </p>
            </div>

            <svg
              className={`w-4 h-4 text-gray-400 transition-transform ${open ? 'rotate-180' : ''}`}
              fill="none" stroke="currentColor" viewBox="0 0 24 24"
            >
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
            </svg>
          </button>

          {open && (
            <div className="absolute right-0 mt-2 w-56 bg-white rounded-xl shadow-lg border border-gray-100 py-1 z-50">
              {/* Account info */}
              <div className="px-4 py-3 border-b border-gray-100">
                <p className="text-xs text-gray-400 mb-0.5">Аккаунт</p>
                <p className="text-sm font-medium text-gray-800 truncate">
                  {user.firstName} {user.lastName}
                </p>
                <p className="text-xs text-gray-500 truncate">{user.email}</p>
                <div className="flex items-center gap-1.5 mt-1.5">
                  <span className="text-xs bg-blue-50 text-blue-700 px-1.5 py-0.5 rounded font-medium">
                    {ROLE_LABELS[user.role] ?? '—'}
                  </span>
                  {user.workType > 0 && (
                    <span className="text-xs bg-gray-100 text-gray-600 px-1.5 py-0.5 rounded">
                      {WORK_TYPE_LABELS[user.workType]}
                    </span>
                  )}
                </div>
              </div>

              {/* Nav links — mobile / in-menu */}
              <div className="md:hidden border-b border-gray-100 py-1">
                {NAV_LINKS.map(link => (
                  <NavLink
                    key={link.to}
                    to={link.to}
                    onClick={() => setOpen(false)}
                    className={({ isActive }) =>
                      `w-full text-left px-4 py-2.5 text-sm flex items-center gap-2 transition-colors ${
                        isActive ? 'text-blue-600 bg-blue-50' : 'text-gray-700 hover:bg-gray-50'
                      }`
                    }
                  >
                    {link.label}
                  </NavLink>
                ))}
              </div>

              {/* Аналитика команды — только Manager (2) и Admin (3) */}
              {user.role >= 2 && (
                <NavLink
                  to="/team-stats"
                  onClick={() => setOpen(false)}
                  className={({ isActive }) =>
                    `w-full text-left px-4 py-2.5 text-sm flex items-center gap-2 border-t border-gray-100 transition-colors ${
                      isActive ? 'text-blue-600 bg-blue-50' : 'text-gray-700 hover:bg-gray-50'
                    }`
                  }
                >
                  <svg className="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                      d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                  </svg>
                  Аналитика команды
                </NavLink>
              )}

              {/* SLA-политики — только Manager (2) и Admin (3) */}
              {user.role >= 2 && (
                <NavLink
                  to="/sla-policies"
                  onClick={() => setOpen(false)}
                  className={({ isActive }) =>
                    `w-full text-left px-4 py-2.5 text-sm flex items-center gap-2 transition-colors ${
                      isActive ? 'text-blue-600 bg-blue-50' : 'text-gray-700 hover:bg-gray-50'
                    }`
                  }
                >
                  <svg className="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                      d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                  </svg>
                  SLA-политики
                </NavLink>
              )}

              {/* Profile (disabled) */}
              <button
                disabled
                className="w-full text-left px-4 py-2.5 text-sm text-gray-400 flex items-center gap-2 cursor-not-allowed"
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                    d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
                Мой профиль
                <span className="ml-auto text-xs bg-gray-100 text-gray-400 px-1.5 py-0.5 rounded">скоро</span>
              </button>

              {/* Logout */}
              <button
                onClick={handleLogout}
                className="w-full text-left px-4 py-2.5 text-sm text-red-600 hover:bg-red-50 flex items-center gap-2 transition-colors"
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                    d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                </svg>
                Выйти
              </button>
            </div>
          )}
        </div>
      )}
    </header>
  )
}
