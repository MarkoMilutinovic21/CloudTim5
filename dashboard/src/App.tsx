import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import LoginPage from './pages/LoginPage'
import DashboardPage from './pages/DashboardPage'
import CreateUserPage from './pages/CreateUserPage'
import UsersPage from './pages/UsersPage'
import ActivatePage from './pages/ActivatePage'

function App() {
  const token = localStorage.getItem('token')

  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/admin/create-user" element={<CreateUserPage />} />
        <Route
          path="/"
          element={token ? <DashboardPage /> : <Navigate to="/login" />}
        />
        <Route path="/admin/users" element={<UsersPage />} />
        <Route path="/activate" element={<ActivatePage />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App