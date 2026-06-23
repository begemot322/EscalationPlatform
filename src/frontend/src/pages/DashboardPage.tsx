import { useEffect, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import Navbar from '../components/Navbar'
import { getProjects, importProjectsFromExcel, ProjectImportResult, ProjectResponse, PagedResult } from '../api/projects'
import { useAuth } from '../context/AuthContext'

function ProjectCardSkeleton() {
  return (
    <div className="bg-white rounded-xl border border-gray-200 p-5 animate-pulse">
      <div className="flex items-start justify-between mb-3">
        <div className="h-5 w-16 bg-gray-200 rounded-full" />
        <div className="h-5 w-14 bg-gray-100 rounded-full" />
      </div>
      <div className="h-5 w-3/4 bg-gray-200 rounded mb-2" />
      <div className="h-3 w-full bg-gray-100 rounded mb-1" />
      <div className="h-3 w-2/3 bg-gray-100 rounded" />
    </div>
  )
}

function ProjectCard({ project, onClick }: { project: ProjectResponse; onClick: () => void }) {
  const date = new Date(project.createdAt).toLocaleDateString('ru-RU', {
    day: 'numeric', month: 'short', year: 'numeric',
  })

  return (
    <button
      onClick={onClick}
      className="text-left bg-white rounded-xl border border-gray-200 p-5 hover:border-blue-300 hover:shadow-md transition-all duration-150 group"
    >
      <div className="flex items-start justify-between mb-3">
        <span className="inline-flex items-center px-2 py-0.5 rounded-md bg-blue-50 text-blue-700 text-xs font-mono font-semibold">
          {project.code}
        </span>
        <span className={`inline-flex items-center gap-1 text-xs font-medium px-2 py-0.5 rounded-full ${
          project.isActive
            ? 'bg-green-50 text-green-700'
            : 'bg-gray-100 text-gray-500'
        }`}>
          <span className={`w-1.5 h-1.5 rounded-full ${project.isActive ? 'bg-green-500' : 'bg-gray-400'}`} />
          {project.isActive ? 'Активен' : 'Архив'}
        </span>
      </div>

      <h3 className="font-semibold text-gray-900 text-sm mb-1.5 group-hover:text-blue-600 transition-colors line-clamp-1">
        {project.name}
      </h3>

      {project.description ? (
        <p className="text-xs text-gray-500 line-clamp-2 mb-4 min-h-[2rem]">
          {project.description}
        </p>
      ) : (
        <p className="text-xs text-gray-300 italic mb-4 min-h-[2rem]">Описание не указано</p>
      )}

      <div className="flex items-center justify-between">
        <span className="text-xs text-gray-400">{date}</span>
        <span className="text-xs text-blue-600 opacity-0 group-hover:opacity-100 transition-opacity font-medium">
          Открыть →
        </span>
      </div>
    </button>
  )
}

export default function DashboardPage() {
  const navigate = useNavigate()
  const { user } = useAuth()
  const fileInputRef = useRef<HTMLInputElement>(null)

  const [data, setData] = useState<PagedResult<ProjectResponse> | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [importing, setImporting] = useState(false)
  const [importResult, setImportResult] = useState<ProjectImportResult | null>(null)
  const [importError, setImportError] = useState<string | null>(null)

  const isAdmin = user?.role === 3

  function loadProjects() {
    setLoading(true)
    getProjects(1, 24)
      .then(setData)
      .catch(() => setError('Не удалось загрузить проекты.'))
      .finally(() => setLoading(false))
  }

  useEffect(() => { loadProjects() }, [])

  async function handleFileSelect(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (!fileInputRef.current) return
    fileInputRef.current.value = ''
    if (!file) return

    setImporting(true)
    setImportResult(null)
    setImportError(null)

    try {
      const result = await importProjectsFromExcel(file)
      setImportResult(result)
      loadProjects()
    } catch (err: any) {
      const details: string[] | undefined =
        err?.errors ?? err?.extensions?.errors ?? err?.extensions?.Errors
      const msg = details && details.length > 0
        ? details.join('; ')
        : (err?.detail ?? 'Ошибка импорта')
      setImportError(msg)
    } finally {
      setImporting(false)
    }
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />

      <main className="pt-14">
        <div className="max-w-7xl mx-auto px-6 py-8">
          {/* Header */}
          <div className="flex items-center justify-between mb-6">
            <div>
              <h1 className="text-xl font-bold text-gray-900">Проекты</h1>
              {data && (
                <p className="text-sm text-gray-500 mt-0.5">
                  {pluralize(data.totalCount, 'проект', 'проекта', 'проектов')}
                </p>
              )}
            </div>

            {/* Импорт — только Admin */}
            {isAdmin && (
              <div>
                <input
                  ref={fileInputRef}
                  type="file"
                  accept=".xlsx"
                  className="hidden"
                  onChange={handleFileSelect}
                />
                <button
                  onClick={() => fileInputRef.current?.click()}
                  disabled={importing}
                  className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                >
                  {importing ? (
                    <>
                      <svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
                      </svg>
                      Импорт…
                    </>
                  ) : (
                    <>
                      <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                          d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12" />
                      </svg>
                      Импорт Excel
                    </>
                  )}
                </button>
              </div>
            )}
          </div>

          {/* Результат импорта */}
          {importResult && (
            <div className="mb-4 flex items-start gap-3 px-4 py-3 bg-green-50 border border-green-200 rounded-xl">
              <svg className="w-5 h-5 text-green-600 shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <div className="flex-1 text-sm">
                <p className="font-medium text-green-800">
                  Импортировано {pluralize(importResult.imported, 'проект', 'проекта', 'проектов')}
                </p>
                {importResult.skippedDuplicateCodes.length > 0 && (
                  <p className="text-green-700 mt-0.5">
                    Пропущено (уже существуют): {importResult.skippedDuplicateCodes.join(', ')}
                  </p>
                )}
              </div>
              <button onClick={() => setImportResult(null)} className="text-green-500 hover:text-green-700">
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
          )}

          {/* Ошибка импорта */}
          {importError && (
            <div className="mb-4 flex items-start gap-3 px-4 py-3 bg-red-50 border border-red-200 rounded-xl">
              <svg className="w-5 h-5 text-red-500 shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <div className="flex-1 text-sm">
                <p className="font-medium text-red-800">Ошибка импорта</p>
                <p className="text-red-700 mt-0.5">{importError}</p>
              </div>
              <button onClick={() => setImportError(null)} className="text-red-400 hover:text-red-600">
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
          )}

          {/* Error */}
          {error && (
            <div className="text-center py-16 text-gray-400">
              <svg className="w-10 h-10 mx-auto mb-3 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                  d="M12 9v2m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <p className="text-sm">{error}</p>
            </div>
          )}

          {/* Grid */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
            {loading
              ? Array.from({ length: 8 }).map((_, i) => <ProjectCardSkeleton key={i} />)
              : data?.items.map((p) => (
                  <ProjectCard
                    key={p.id}
                    project={p}
                    onClick={() => navigate(`/projects/${p.id}`)}
                  />
                ))}
          </div>

          {/* Empty state */}
          {!loading && !error && data?.items.length === 0 && (
            <div className="text-center py-20">
              <svg className="w-12 h-12 mx-auto mb-4 text-gray-200" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5}
                  d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z" />
              </svg>
              <p className="text-gray-400 text-sm">Проектов пока нет</p>
            </div>
          )}
        </div>
      </main>
    </div>
  )
}

function pluralize(n: number, one: string, few: string, many: string) {
  const mod10 = n % 10
  const mod100 = n % 100
  if (mod10 === 1 && mod100 !== 11) return `${n} ${one}`
  if (mod10 >= 2 && mod10 <= 4 && (mod100 < 10 || mod100 >= 20)) return `${n} ${few}`
  return `${n} ${many}`
}
