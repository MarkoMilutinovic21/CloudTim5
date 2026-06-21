import type { ReactNode } from 'react'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import LoginPage from './pages/LoginPage'
import DashboardPage from './pages/DashboardPage'
import CreateUserPage from './pages/CreateUserPage'
import UsersPage from './pages/UsersPage'
import ActivatePage from './pages/ActivatePage'
import ForgotPasswordPage from './pages/ForgotPasswordPage'
import ResetPasswordPage from './pages/ResetPasswordPage'
import ParcelsPage from './pages/ParcelsPage'
import PesticideTreatmentsPage from './pages/PesticideTreatmentsPage'
import TelemetryPage from './pages/TelemetryPage'
import NotificationStatusPage from './pages/NotificationStatusPage'
import NearbyCropsPage from './pages/NearbyCropsPage'
import HiveJournalPage from './pages/HiveJournalPage'
import AlertsPage from './pages/AlertsPage'
import SprayingRecordsPage from './pages/SprayingRecordsPage'
import CropsPage from './pages/CropsPage'
import ApiariesPage from './pages/ApiariesPage'
import HivesPage from './pages/HivesPage'
import BeekeeperSettingsPage from './pages/BeekeeperSettingsPage'

function ProtectedRoute({ children, roles }: { children: ReactNode; roles?: string[] }) {
  const token = localStorage.getItem('token')
  const role = localStorage.getItem('role')

  if (!token) return <Navigate to="/login" replace />
  if (roles && (!role || !roles.includes(role))) return <Navigate to="/" replace />
  return children
}

const guarded = (page: ReactNode, roles?: string[]) => (
  <ProtectedRoute roles={roles}>{page}</ProtectedRoute>
)

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/activate" element={<ActivatePage />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />
        <Route path="/" element={guarded(<DashboardPage />)} />
        <Route path="/admin/create-user" element={guarded(<CreateUserPage />, ['Admin'])} />
        <Route path="/admin/users" element={guarded(<UsersPage />, ['Admin'])} />
        <Route path="/farmer/parcels" element={guarded(<ParcelsPage />, ['Farmer'])} />
        <Route path="/farmer/pesticide-treatments" element={guarded(<PesticideTreatmentsPage />, ['Farmer'])} />
        <Route path="/farmer/notification-status" element={guarded(<NotificationStatusPage />, ['Farmer'])} />
        <Route path="/farmer/spraying-records" element={guarded(<SprayingRecordsPage />, ['Farmer'])} />
        <Route path="/farmer/crops" element={guarded(<CropsPage />, ['Farmer'])} />
        <Route path="/beekeeper/telemetry" element={guarded(<TelemetryPage />, ['Beekeeper'])} />
        <Route path="/beekeeper/apiaries" element={guarded(<ApiariesPage />, ['Beekeeper'])} />
        <Route path="/beekeeper/hives" element={guarded(<HivesPage />, ['Beekeeper'])} />
        <Route path="/beekeeper/crops" element={guarded(<NearbyCropsPage />, ['Beekeeper'])} />
        <Route path="/beekeeper/journal" element={guarded(<HiveJournalPage />, ['Beekeeper'])} />
        <Route path="/beekeeper/alerts" element={guarded(<AlertsPage />, ['Beekeeper'])} />
        <Route path="/beekeeper/settings" element={guarded(<BeekeeperSettingsPage />, ['Beekeeper'])} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App
