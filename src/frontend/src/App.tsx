import { Routes, Route, Navigate } from 'react-router-dom'
import { useAuth } from './context/AuthContext'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import HomePage from './pages/HomePage'
import DashboardPage from './pages/DashboardPage'
import ProjectDetailPage from './pages/ProjectDetailPage'
import EscalationDetailPage from './pages/EscalationDetailPage'
import EscalationsPage from './pages/EscalationsPage'
import TeamStatsPage from './pages/TeamStatsPage'
import SlaPoliciesPage from './pages/SlaPoliciesPage'
import { ReactNode } from 'react'

function ProtectedRoute({ children }: { children: ReactNode }) {
  const { user } = useAuth()
  return user ? <>{children}</> : <Navigate to="/login" replace />
}

function GuestRoute({ children }: { children: ReactNode }) {
  const { user } = useAuth()
  return user ? <Navigate to="/" replace /> : <>{children}</>
}

export default function App() {
  return (
    <Routes>
      <Route path="/login"    element={<GuestRoute><LoginPage /></GuestRoute>} />
      <Route path="/register" element={<GuestRoute><RegisterPage /></GuestRoute>} />

      <Route path="/"        element={<ProtectedRoute><HomePage /></ProtectedRoute>} />
      <Route path="/projects" element={<ProtectedRoute><DashboardPage /></ProtectedRoute>} />
      <Route path="/projects/:id" element={<ProtectedRoute><ProjectDetailPage /></ProtectedRoute>} />

      <Route path="/my-escalations" element={<ProtectedRoute><EscalationsPage mode="my" /></ProtectedRoute>} />
      <Route path="/assigned"       element={<ProtectedRoute><EscalationsPage mode="assigned" /></ProtectedRoute>} />
      <Route path="/observed"       element={<ProtectedRoute><EscalationsPage mode="observed" /></ProtectedRoute>} />

      <Route path="/escalations/:escalationId" element={<ProtectedRoute><EscalationDetailPage /></ProtectedRoute>} />
      <Route path="/team-stats" element={<ProtectedRoute><TeamStatsPage /></ProtectedRoute>} />
      <Route path="/sla-policies" element={<ProtectedRoute><SlaPoliciesPage /></ProtectedRoute>} />

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
