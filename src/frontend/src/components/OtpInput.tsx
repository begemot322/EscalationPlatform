import { useRef, KeyboardEvent, ClipboardEvent } from 'react'

interface OtpInputProps {
  value: string
  onChange: (value: string) => void
  length?: number
  disabled?: boolean
}

export default function OtpInput({ value, onChange, length = 6, disabled = false }: OtpInputProps) {
  const refs = useRef<Array<HTMLInputElement | null>>([])

  const digits = value.split('').concat(Array(length).fill('')).slice(0, length)

  function handleChange(index: number, char: string) {
    const digit = char.replace(/\D/g, '').slice(-1)
    const next = [...digits]
    next[index] = digit
    onChange(next.join(''))
    if (digit && index < length - 1) {
      refs.current[index + 1]?.focus()
    }
  }

  function handleKeyDown(index: number, e: KeyboardEvent<HTMLInputElement>) {
    if (e.key === 'Backspace') {
      if (digits[index]) {
        const next = [...digits]
        next[index] = ''
        onChange(next.join(''))
      } else if (index > 0) {
        refs.current[index - 1]?.focus()
      }
    } else if (e.key === 'ArrowLeft' && index > 0) {
      refs.current[index - 1]?.focus()
    } else if (e.key === 'ArrowRight' && index < length - 1) {
      refs.current[index + 1]?.focus()
    }
  }

  function handlePaste(e: ClipboardEvent<HTMLInputElement>) {
    e.preventDefault()
    const pasted = e.clipboardData.getData('text').replace(/\D/g, '').slice(0, length)
    if (!pasted) return
    const next = pasted.split('').concat(Array(length).fill('')).slice(0, length)
    onChange(next.join(''))
    const lastFilledIndex = Math.min(pasted.length, length - 1)
    refs.current[lastFilledIndex]?.focus()
  }

  return (
    <div className="flex gap-3 justify-center">
      {digits.map((digit, i) => (
        <input
          key={i}
          ref={(el) => { refs.current[i] = el }}
          type="text"
          inputMode="numeric"
          maxLength={1}
          value={digit}
          disabled={disabled}
          onChange={(e) => handleChange(i, e.target.value)}
          onKeyDown={(e) => handleKeyDown(i, e)}
          onPaste={handlePaste}
          onFocus={(e) => e.target.select()}
          className={[
            'w-12 h-14 text-center text-xl font-semibold rounded-lg border-2',
            'text-gray-900 bg-white',
            'focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500',
            digit
              ? 'border-blue-400 bg-blue-50'
              : 'border-gray-300',
            disabled ? 'opacity-50 cursor-not-allowed' : '',
            'transition-colors duration-150',
          ].join(' ')}
        />
      ))}
    </div>
  )
}
