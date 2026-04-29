import { useState } from 'react'
import axios from 'axios'

const ROLES = ['Beekeeper', 'Farmer']

function CreateUserPage() {
  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    phone: '',
    email: '',
    role: 'Beekeeper',
  })
  const [success, setSuccess] = useState(false)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value })
  }

  const handleSubmit = async () => {
    setLoading(true)
    setError('')
    setSuccess(false)
    try {
      const token = localStorage.getItem('token')
      await axios.post('http://localhost:5108/api/Users', form, {
        headers: { Authorization: `Bearer ${token}` },
      })
      setSuccess(true)
      setForm({ firstName: '', lastName: '', phone: '', email: '', role: 'Beekeeper' })
    } catch {
      setError('Greška pri kreiranju korisnika.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center">
      <div className="bg-slate-900 p-8 rounded-xl shadow-xl w-full max-w-md">
        <h1 className="text-2xl font-bold text-white mb-6">Kreiranje korisnika</h1>

        {success && (
          <div className="bg-green-900/50 border border-green-700 text-green-300 px-4 py-3 rounded mb-4 text-sm">
            Korisnik uspešno kreiran! Email za aktivaciju je poslat.
          </div>
        )}

        {error && (
          <div className="bg-red-900/50 border border-red-700 text-red-300 px-4 py-3 rounded mb-4 text-sm">
            {error}
          </div>
        )}

        <div className="mb-4">
          <label className="block text-slate-400 text-sm mb-2">Ime</label>
          <input
            name="firstName"
            value={form.firstName}
            onChange={handleChange}
            className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
          />
        </div>

        <div className="mb-4">
          <label className="block text-slate-400 text-sm mb-2">Prezime</label>
          <input
            name="lastName"
            value={form.lastName}
            onChange={handleChange}
            className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
          />
        </div>

        <div className="mb-4">
          <label className="block text-slate-400 text-sm mb-2">Telefon</label>
          <input
            name="phone"
            value={form.phone}
            onChange={handleChange}
            className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
          />
        </div>

        <div className="mb-4">
          <label className="block text-slate-400 text-sm mb-2">Email</label>
          <input
            name="email"
            type="email"
            value={form.email}
            onChange={handleChange}
            className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
          />
        </div>

        <div className="mb-6">
          <label className="block text-slate-400 text-sm mb-2">Uloga</label>
          <select
            name="role"
            value={form.role}
            onChange={handleChange}
            className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
          >
            {ROLES.map(role => (
              <option key={role} value={role}>
                {role === 'Beekeeper' ? 'Pčelar' : 'Poljoprivrednik'}
              </option>
            ))}
          </select>
        </div>

        <button
          onClick={handleSubmit}
          disabled={loading}
          className="w-full bg-yellow-500 hover:bg-yellow-400 text-slate-950 font-bold py-3 rounded transition-colors disabled:opacity-50"
        >
          {loading ? 'Kreiranje...' : 'Kreiraj korisnika'}
        </button>
      </div>
    </div>
  )
}

export default CreateUserPage