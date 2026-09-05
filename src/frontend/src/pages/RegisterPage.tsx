import { useState, FormEvent, useEffect } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { Button, Label, TextInput, Select, Alert, HelperText } from 'flowbite-react'
import { register, sendOtp, verifyOtp, ApiError, getApiErrors } from '../api/auth'
import { useAuth } from '../context/AuthContext'
import OtpInput from '../components/OtpInput'

type Step = 'form' | 'verify'

interface FormData {
  firstName: string
  lastName: string
  email: string
  password: string
  confirmPassword: string
  role: string
  workType: string
}

interface FieldErrors {
  firstName?: string
  lastName?: string
  email?: string
  password?: string
  confirmPassword?: string
}

// Требования к паролю — live-индикатор
const PASSWORD_RULES = [
  { label: 'Минимум 8 символов',   test: (p: string) => p.length >= 8 },
  { label: 'Заглавная буква (A–Z)', test: (p: string) => /[A-Z]/.test(p) },
  { label: 'Строчная буква (a–z)',  test: (p: string) => /[a-z]/.test(p) },
  { label: 'Цифра (0–9)',           test: (p: string) => /[0-9]/.test(p) },
]

function PasswordHint({ password }: { password: string }) {
  if (!password) return null
  return (
    <ul className="mt-2 flex flex-col gap-1">
      {PASSWORD_RULES.map((rule) => {
        const ok = rule.test(password)
        return (
          <li key={rule.label} className={`flex items-center gap-1.5 text-xs ${ok ? 'text-green-600' : 'text-gray-400'}`}>
            <svg className="w-3.5 h-3.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              {ok
                ? <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M5 13l4 4L19 7" />
                : <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />}
            </svg>
            {rule.label}
          </li>
        )
      })}
    </ul>
  )
}

const ROLE_OPTIONS = [
  { value: '1', label: 'Сотрудник' },
  { value: '2', label: 'Менеджер' },
  { value: '3', label: 'Администратор' },
]

const WORK_TYPE_OPTIONS = [
  { value: '1', label: 'Tech Lead' },
  { value: '2', label: 'Project Manager' },
  { value: '3', label: 'Backend Developer' },
  { value: '4', label: 'Frontend Developer' },
  { value: '5', label: 'QA Engineer' },
  { value: '6', label: 'DevOps' },
  { value: '7', label: 'Дизайнер' },
]

export default function RegisterPage() {
  const navigate = useNavigate()
  const { loadUser } = useAuth()
  const [searchParams] = useSearchParams()

  const [step, setStep] = useState<Step>(() =>
    searchParams.get('step') === 'verify' ? 'verify' : 'form',
  )
  const [form, setForm] = useState<FormData>({
    firstName: '',
    lastName: '',
    email: searchParams.get('email') ?? '',
    password: '',
    confirmPassword: '',
    role: '1',
    workType: '3',
  })
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({})
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const [otp, setOtp] = useState('')
  const [canResendAt, setCanResendAt] = useState<Date | null>(null)
  const [resendCountdown, setResendCountdown] = useState(0)

  useEffect(() => {
    if (!canResendAt) return
    const tick = () => {
      const diff = Math.ceil((canResendAt.getTime() - Date.now()) / 1000)
      setResendCountdown(Math.max(0, diff))
    }
    tick()
    const id = setInterval(tick, 1000)
    return () => clearInterval(id)
  }, [canResendAt])

  function setField(key: keyof FormData, value: string) {
    setForm((prev) => ({ ...prev, [key]: value }))
    setFieldErrors((prev) => ({ ...prev, [key]: undefined }))
  }

  function translateError(msg: string): string {
    const l = msg.toLowerCase()
    if (l.includes('must not be empty') || l.includes('required')) return 'Поле обязательно для заполнения'
    if (l.includes('maximum length')) {
      const m = l.match(/(\d+)/)
      return m ? `Максимальная длина — ${m[1]} символов` : 'Слишком длинное значение'
    }
    if (l.includes('minimum length') || (l.includes('at least') && l.includes('character'))) {
      const m = l.match(/(\d+)/)
      return m ? `Минимальная длина — ${m[1]} символов` : 'Слишком короткое значение'
    }
    if (l.includes('valid email') || l.includes('email address')) return 'Некорректный адрес email'
    if (l.includes('uppercase')) return 'Нужна хотя бы одна заглавная буква (A–Z)'
    if (l.includes('lowercase')) return 'Нужна хотя бы одна строчная буква (a–z)'
    if (l.includes('digit')) return 'Нужна хотя бы одна цифра (0–9)'
    if (l.includes('equal') || l.includes('match') || l.includes('confirm')) return 'Пароли не совпадают'
    if (l.includes('invalid') || l.includes('valid value')) return 'Некорректное значение'
    return msg
  }

  function parseValidationErrors(errors: string[]): { fields: FieldErrors; unmatched: string[] } {
    const passwordMsgs: string[] = []
    const fields: FieldErrors = {}
    const unmatched: string[] = []

    for (const msg of errors) {
      const lower = msg.toLowerCase()

      if (lower.includes('firstname') || lower.includes('first name')) {
        fields.firstName = translateError(msg)
      } else if (lower.includes('lastname') || lower.includes('last name')) {
        fields.lastName = translateError(msg)
      } else if (lower.includes('email')) {
        fields.email = translateError(msg)
      } else if (lower.includes('confirmpassword') || lower.includes('confirm')) {
        fields.confirmPassword = 'Пароли не совпадают'
      } else if (lower.includes('password')) {
        passwordMsgs.push(translateError(msg))
      } else if (lower.includes('role')) {
        // роль выбирается из списка, не показываем
      } else {
        unmatched.push(translateError(msg))
      }
    }

    if (passwordMsgs.length) {
      fields.password = passwordMsgs.join(' • ')
    }

    return { fields, unmatched }
  }

  async function handleRegister(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setFieldErrors({})

    // Client-side проверки до запроса
    const clientErrors: FieldErrors = {}
    if (!form.firstName.trim()) clientErrors.firstName = 'Поле обязательно для заполнения'
    if (!form.lastName.trim()) clientErrors.lastName = 'Поле обязательно для заполнения'
    if (form.password !== form.confirmPassword) clientErrors.confirmPassword = 'Пароли не совпадают'
    if (!PASSWORD_RULES.every((r) => r.test(form.password))) {
      const failed = PASSWORD_RULES.filter((r) => !r.test(form.password)).map((r) => r.label)
      clientErrors.password = failed.join(' • ')
    }
    if (Object.keys(clientErrors).length) {
      setFieldErrors(clientErrors)
      return
    }

    setLoading(true)
    try {
      await register({
        firstName: form.firstName,
        lastName: form.lastName,
        email: form.email,
        password: form.password,
        confirmPassword: form.confirmPassword,
        role: Number(form.role),
        workType: Number(form.workType),
      })
      const res = await sendOtp(form.email)
      setCanResendAt(new Date(res.canResendAt))
      setStep('verify')
    } catch (err) {
      const apiErr = err as ApiError
      const errors = getApiErrors(apiErr)

      if (apiErr.title === 'VALIDATION_FAILED') {
        if (errors.length) {
          const { fields, unmatched } = parseValidationErrors(errors)
          setFieldErrors(fields)
          const hasFieldErrors = Object.keys(fields).length > 0
          if (unmatched.length && !hasFieldErrors) {
            setError('Проверьте правильность заполненных полей.')
          }
        } else {
          setError('Проверьте правильность заполненных полей.')
        }
      } else if (apiErr.title === 'DUPLICATE') {
        setFieldErrors({ email: 'Этот email уже зарегистрирован.' })
      } else {
        setError(apiErr.detail || 'Не удалось создать аккаунт.')
      }
    } finally {
      setLoading(false)
    }
  }

  async function handleVerify(e: FormEvent) {
    e.preventDefault()
    if (otp.length < 6) return
    setError(null)
    setLoading(true)
    try {
      await verifyOtp({ email: form.email, code: otp })
      await loadUser()
      navigate('/')
    } catch (err) {
      const apiErr = err as ApiError
      if (apiErr.title === 'OTP_INVALID_OR_EXPIRED') {
        setError('Неверный или истёкший код. Попробуйте снова или запросите новый.')
      } else {
        setError(apiErr.detail || 'Не удалось подтвердить код.')
      }
      setOtp('')
    } finally {
      setLoading(false)
    }
  }

  async function handleResend() {
    setError(null)
    setLoading(true)
    try {
      const res = await sendOtp(form.email)
      setCanResendAt(new Date(res.canResendAt))
      setOtp('')
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
            <p className="text-2xl font-semibold text-gray-800">Работайте вместе</p>
            <p className="text-gray-500 mt-2 text-sm max-w-sm">
              Создайте аккаунт и присоединитесь к команде для управления эскалациями.
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

          {/* Step indicator */}
          <div className="flex items-center gap-3 mb-8">
            <div className={`flex items-center gap-2 text-sm font-medium ${step === 'form' ? 'text-blue-600' : 'text-gray-400'}`}>
              <span className={`w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold
                ${step === 'verify' ? 'bg-green-500 text-white' : 'bg-blue-600 text-white'}`}>
                {step === 'verify' ? (
                  <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M5 13l4 4L19 7" />
                  </svg>
                ) : '1'}
              </span>
              Регистрация
            </div>
            <div className="flex-1 h-px bg-gray-200" />
            <div className={`flex items-center gap-2 text-sm font-medium ${step === 'verify' ? 'text-blue-600' : 'text-gray-400'}`}>
              <span className={`w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold
                ${step === 'verify' ? 'bg-blue-600 text-white' : 'bg-gray-200 text-gray-500'}`}>
                2
              </span>
              Подтверждение
            </div>
          </div>

          {step === 'form' ? (
            <>
              <h2 className="text-2xl font-bold text-gray-900 mb-1">Создайте аккаунт</h2>
              <p className="text-gray-500 text-sm mb-8">Заполните данные для регистрации</p>

              {error && <Alert color="failure" className="mb-5">{error}</Alert>}

              <form onSubmit={handleRegister} className="flex flex-col gap-4">
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <Label htmlFor="firstName" value="Имя" className="mb-1.5 block" />
                    <TextInput
                      id="firstName"
                      type="text"
                      placeholder="Иван"
                      required
                      value={form.firstName}
                      onChange={(e) => setField('firstName', e.target.value)}
                      color={fieldErrors.firstName ? 'failure' : undefined}
                    />
                    {fieldErrors.firstName && <HelperText color="failure">{fieldErrors.firstName}</HelperText>}
                  </div>
                  <div>
                    <Label htmlFor="lastName" value="Фамилия" className="mb-1.5 block" />
                    <TextInput
                      id="lastName"
                      type="text"
                      placeholder="Иванов"
                      required
                      value={form.lastName}
                      onChange={(e) => setField('lastName', e.target.value)}
                      color={fieldErrors.lastName ? 'failure' : undefined}
                    />
                    {fieldErrors.lastName && <HelperText color="failure">{fieldErrors.lastName}</HelperText>}
                  </div>
                </div>

                <div>
                  <Label htmlFor="email" value="Email" className="mb-1.5 block" />
                  <TextInput
                    id="email"
                    type="email"
                    placeholder="you@example.com"
                    required
                    value={form.email}
                    onChange={(e) => setField('email', e.target.value)}
                    autoComplete="email"
                    color={fieldErrors.email ? 'failure' : undefined}
                  />
                  {fieldErrors.email && <HelperText color="failure">{fieldErrors.email}</HelperText>}
                </div>

                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <Label htmlFor="role" value="Роль" className="mb-1.5 block" />
                    <Select
                      id="role"
                      required
                      value={form.role}
                      onChange={(e) => setField('role', e.target.value)}
                    >
                      {ROLE_OPTIONS.map((opt) => (
                        <option key={opt.value} value={opt.value}>{opt.label}</option>
                      ))}
                    </Select>
                  </div>
                  <div>
                    <Label htmlFor="workType" value="Должность" className="mb-1.5 block" />
                    <Select
                      id="workType"
                      required
                      value={form.workType}
                      onChange={(e) => setField('workType', e.target.value)}
                    >
                      {WORK_TYPE_OPTIONS.map((opt) => (
                        <option key={opt.value} value={opt.value}>{opt.label}</option>
                      ))}
                    </Select>
                  </div>
                </div>

                <div>
                  <Label htmlFor="password" value="Пароль" className="mb-1.5 block" />
                  <TextInput
                    id="password"
                    type="password"
                    placeholder=""
                    required
                    value={form.password}
                    onChange={(e) => setField('password', e.target.value)}
                    autoComplete="new-password"
                    color={fieldErrors.password ? 'failure' : undefined}
                  />
                  {fieldErrors.password
                    ? <HelperText color="failure">{fieldErrors.password}</HelperText>
                    : <PasswordHint password={form.password} />}
                </div>

                <div>
                  <Label htmlFor="confirmPassword" value="Подтвердите пароль" className="mb-1.5 block" />
                  <TextInput
                    id="confirmPassword"
                    type="password"
                    placeholder=""
                    required
                    value={form.confirmPassword}
                    onChange={(e) => setField('confirmPassword', e.target.value)}
                    autoComplete="new-password"
                    color={fieldErrors.confirmPassword ? 'failure' : undefined}
                  />
                  {fieldErrors.confirmPassword && <HelperText color="failure">{fieldErrors.confirmPassword}</HelperText>}
                </div>

                <Button type="submit" disabled={loading} isProcessing={loading} className="mt-1">
                  Создать аккаунт
                </Button>
              </form>

              <p className="text-center text-sm text-gray-500 mt-6">
                Уже есть аккаунт?{' '}
                <Link to="/login" className="text-blue-600 font-medium hover:underline">
                  Войти
                </Link>
              </p>
            </>
          ) : (
            <>
              <div className="text-center mb-8">
                <div className="inline-flex items-center justify-center w-14 h-14 rounded-full bg-blue-50 mb-4">
                  <svg className="w-7 h-7 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                      d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                  </svg>
                </div>
                <h2 className="text-2xl font-bold text-gray-900 mb-1">Подтвердите email</h2>
                <p className="text-gray-500 text-sm">
                  Мы отправили 6-значный код на{' '}
                  <span className="font-semibold text-gray-700">{form.email}</span>
                </p>
                <p className="text-gray-400 text-xs mt-1">Код действителен 3 минуты</p>
              </div>

              {error && <Alert color="failure" className="mb-5">{error}</Alert>}

              <form onSubmit={handleVerify} className="flex flex-col items-center gap-6">
                <OtpInput value={otp} onChange={setOtp} disabled={loading} />

                <Button
                  type="submit"
                  disabled={loading || otp.length < 6}
                  isProcessing={loading}
                  className="w-full"
                >
                  Подтвердить
                </Button>

                <p className="text-sm text-gray-500">
                  {resendCountdown > 0 ? (
                    <span>Повторная отправка через {resendCountdown} сек.</span>
                  ) : (
                    <button
                      type="button"
                      onClick={handleResend}
                      disabled={loading}
                      className="text-blue-600 font-medium hover:underline disabled:opacity-50"
                    >
                      Отправить код повторно
                    </button>
                  )}
                </p>
              </form>
            </>
          )}
        </div>
      </div>
    </div>
  )
}
