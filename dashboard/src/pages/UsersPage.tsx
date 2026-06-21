import { useEffect, useState } from 'react'
import axios from 'axios'
import ReturnToDashboardButton from '../components/ReturnToDashboardButton'

interface User {
  id: string
  firstName: string
  lastName: string
  email: string
  phone: string
  role: string
  isActive: boolean
  createdAt: string
}

function UsersPage() {
  const [users, setUsers] = useState<User[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const token = localStorage.getItem('token')
  const headers = { Authorization: `Bearer ${token}` }

  const fetchUsers = async () => {
    try {
      const response = await axios.get('http://localhost:5108/api/Users', { headers })
      setUsers(response.data)
    } catch {
      setError('Greška pri učitavanju korisnika.')
    } finally {
      setLoading(false)
    }
  }

  const handleSuspend = async (id: string) => {
    try {
      await axios.put(`http://localhost:5108/api/Users/${id}/suspend`, {}, { headers })
      fetchUsers()
    } catch {
      alert('Greška pri suspenziji korisnika.')
    }
  }

  const handleDelete = async (id: string) => {
    if (!confirm('Da li ste sigurni da želite obrisati korisnika?')) return
    try {
      await axios.delete(`http://localhost:5108/api/Users/${id}`, { headers })
      fetchUsers()
    } catch {
      alert('Greška pri brisanju korisnika.')
    }
  }

  useEffect(() => {
    fetchUsers()
    // Initial load only.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  if (loading) {
    return (
      <div className="min-h-screen app-shell flex items-center justify-center text-white">
        Učitavanje...
      </div>
    )
  }

  return (
    <div className="min-h-screen app-shell p-8">
      <div className="max-w-5xl mx-auto">
        <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between mb-6">
          <h1 className="text-2xl font-bold text-white">Upravljanje korisnicima</h1>
          <div className="flex flex-wrap items-center gap-3">
            <ReturnToDashboardButton />
            <a
              href="/admin/create-user"
              className="bg-yellow-500 hover:bg-yellow-400 text-slate-950 font-bold px-4 py-2 rounded transition-colors"
            >
              + Novi korisnik
            </a>
          </div>
        </div>

        {error && (
          <div className="bg-red-900/50 border border-red-700 text-red-300 px-4 py-3 rounded mb-4">
            {error}
          </div>
        )}

        <div className="bg-slate-900 rounded-xl overflow-hidden">
          <table className="w-full">
            <thead>
              <tr className="border-b border-slate-700">
                <th className="text-left text-slate-400 px-6 py-4 font-medium">Ime i prezime</th>
                <th className="text-left text-slate-400 px-6 py-4 font-medium">Email</th>
                <th className="text-left text-slate-400 px-6 py-4 font-medium">Telefon</th>
                <th className="text-left text-slate-400 px-6 py-4 font-medium">Uloga</th>
                <th className="text-left text-slate-400 px-6 py-4 font-medium">Status</th>
                <th className="text-left text-slate-400 px-6 py-4 font-medium">Akcije</th>
              </tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr key={user.id} className="border-b border-slate-800 hover:bg-slate-800/50">
                  <td className="px-6 py-4 text-white">{user.firstName} {user.lastName}</td>
                  <td className="px-6 py-4 text-slate-300">{user.email}</td>
                  <td className="px-6 py-4 text-slate-300">{user.phone || '-'}</td>
                  <td className="px-6 py-4">
                    <span className={`px-2 py-1 rounded text-xs font-medium ${
                      user.role === 'Admin'
                        ? 'bg-purple-900/50 text-purple-300'
                        : user.role === 'Beekeeper'
                        ? 'bg-yellow-900/50 text-yellow-300'
                        : 'bg-green-900/50 text-green-300'
                    }`}>
                      {user.role === 'Admin'
                        ? 'Administrator'
                        : user.role === 'Beekeeper'
                        ? 'Pčelar'
                        : 'Poljoprivrednik'}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <span className={`px-2 py-1 rounded text-xs font-medium ${
                      user.isActive
                        ? 'bg-green-900/50 text-green-300'
                        : 'bg-red-900/50 text-red-300'
                    }`}>
                      {user.isActive ? 'Aktivan' : 'Neaktivan'}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    {user.role !== 'Admin' && (
                      <div className="flex gap-2">
                        <button
                          onClick={() => handleSuspend(user.id)}
                          className="bg-orange-600 hover:bg-orange-500 text-white text-xs px-3 py-1 rounded transition-colors"
                        >
                          Suspenduj
                        </button>
                        <button
                          onClick={() => handleDelete(user.id)}
                          className="bg-red-600 hover:bg-red-500 text-white text-xs px-3 py-1 rounded transition-colors"
                        >
                          Obriši
                        </button>
                      </div>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}

export default UsersPage
