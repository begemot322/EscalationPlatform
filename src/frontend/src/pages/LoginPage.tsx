import { useState, FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { Button, Label, TextInput, Alert } from 'flowbite-react'
import { login, sendOtp, ApiError } from '../api/auth'
import { useAuth } from '../context/AuthContext'

export default function LoginPage() {
  const navigate = useNavigate()
  const { loadUser } = useAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [notVerified, setNotVerified] = useState(false)

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setNotVerified(false)
    setLoading(true)
    try {
      await login({ email, password })
      await loadUser()
      navigate('/')
    } catch (err) {
      const apiErr = err as ApiError
      if (apiErr.title === 'EMAIL_NOT_VERIFIED') {
        setNotVerified(true)
      } else if (apiErr.title === 'INVALID_CREDENTIALS') {
        setError('Неверный email или пароль.')
      } else {
        setError(apiErr.detail || 'Не удалось войти. Попробуйте снова.')
      }
    } finally {
      setLoading(false)
    }
  }

  async function handleResendVerification() {
    setLoading(true)
    setError(null)
    try {
      await sendOtp(email)
      navigate(`/register?step=verify&email=${encodeURIComponent(email)}`)
    } catch (err) {
      const apiErr = err as ApiError
      setError(apiErr.detail || 'Не удалось отправить код.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex">
      {/* Left panel */}
      <div className="hidden lg:flex flex-col justify-between w-1/2 bg-blue-50 p-12">
        <div className="flex items-center gap-3">
          <span className="text-xl font-bold text-gray-900">Система управления эскалациями</span>
        </div>
        <div className="flex flex-col items-center gap-8">
          <img
            src="/illustration.png"
            alt="Team collaboration"
            className="w-full max-w-md select-none"
            draggable={false}
          />
          <div className="text-center">
            <p className="text-2xl font-semibold text-gray-800">Управляйте эскалациями</p>
            <p className="text-gray-500 mt-2 text-sm max-w-sm">
              Отслеживайте инциденты, расставляйте приоритеты и держите команду в курсе.
            </p>
          </div>
        </div>
        <p className="text-xs text-gray-400">© 2025 Система управления эскалациями</p>
      </div>

      {/* Right panel */}
      <div className="flex-1 flex flex-col justify-center items-center px-6 py-12 bg-white">
        <div className="w-full max-w-sm">
          {/* Mobile brand */}
          <div className="lg:hidden text-center mb-8">
            <p className="text-2xl font-bold text-gray-900">Система управления эскалациями</p>
          </div>

          <h2 className="text-2xl font-bold text-gray-900 mb-1">Добро пожаловать</h2>
          <p className="text-gray-500 text-sm mb-8">Войдите в свой аккаунт</p>

          {error && (
            <Alert color="failure" className="mb-5">
              {error}
            </Alert>
          )}

          {notVerified && (
            <Alert color="warning" className="mb-5">
              <p className="font-medium mb-1">Email не подтверждён</p>
              <p className="text-sm mb-3">Необходимо подтвердить email перед входом.</p>
              <button
                onClick={handleResendVerification}
                disabled={loading}
                className="text-sm font-medium underline hover:no-underline"
              >
                Отправить код подтверждения
              </button>
            </Alert>
          )}

          <form onSubmit={handleSubmit} className="flex flex-col gap-5">
            <div>
              <Label htmlFor="email" value="Email" className="mb-1.5 block" />
              <TextInput
                id="email"
                type="email"
                placeholder="you@example.com"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                autoComplete="email"
              />
            </div>

            <div>
              <Label htmlFor="password" value="Пароль" className="mb-1.5 block" />
              <TextInput
                id="password"
                type="password"
                placeholder=""
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                autoComplete="current-password"
              />
            </div>

            <Button type="submit" disabled={loading} isProcessing={loading}>
              Войти
            </Button>
          </form>

          <p className="text-center text-sm text-gray-500 mt-6">
            Нет аккаунта?{' '}
            <Link to="/register" className="text-blue-600 font-medium hover:underline">
              Зарегистрироваться
            </Link>
          </p>
        </div>
      </div>
    </div>
  )
}
