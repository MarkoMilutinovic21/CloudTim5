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

function App() {
  const token = localStorage.getItem('token')

  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/activate" element={<ActivatePage />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />
        <Route path="/admin/create-user" element={<CreateUserPage />} />
        <Route path="/admin/users" element={<UsersPage />} />
        <Route
          path="/"
          element={token ? <DashboardPage /> : <Navigate to="/login" />}
        />
        <Route
          path="/farmer/parcels"
          element={token ? <ParcelsPage /> : <Navigate to="/login" />}
        />
        <Route
          path="/farmer/pesticide-treatments"
          element={token ? <PesticideTreatmentsPage /> : <Navigate to="/login" />}
        />
        <Route
          path="/farmer/notification-status"
          element={token ? <NotificationStatusPage /> : <Navigate to="/login" />}
        />
        <Route
          path="/beekeeper/telemetry"
          element={token ? <TelemetryPage /> : <Navigate to="/login" />}
        />
        <Route
          path="/beekeeper/crops"
          element={token ? <NearbyCropsPage /> : <Navigate to="/login" />}
        />
        <Route
          path="/beekeeper/journal"
          element={token ? <HiveJournalPage /> : <Navigate to="/login" />}
        />
        <Route
          path="/beekeeper/alerts"
          element={token ? <AlertsPage /> : <Navigate to="/login" />}
        />
        <Route
          path="/farmer/spraying-records"
          element={token ? <SprayingRecordsPage /> : <Navigate to="/login" />}
        />
        <Route
          path="/farmer/crops"
          element={token ? <CropsPage /> : <Navigate to="/login" />}
        />
      </Routes>
    </BrowserRouter>
  )
}

export default App