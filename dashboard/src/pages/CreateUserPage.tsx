import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import axios from 'axios'

const ROLES = ['Beekeeper', 'Farmer']

interface FormErrors {
  firstName?: string
  lastName?: string
  phone?: string
  email?: string
}

function CreateUserPage() {
  const navigate = useNavigate()
  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    phone: '',
    email: '',
    role: 'Beekeeper',
  })
  const [success, setSuccess] = useState(false)
  const [error, setError] = useState('')
  const [fieldErrors, setFieldErrors] = useState<FormErrors>({})
  const [phoneWarning, setPhoneWarning] = useState('')
  const [loading, setLoading] = useState(false)

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value })
    setFieldErrors({ ...fieldErrors, [e.target.name]: undefined })
  }

  const validate = (): boolean => {
    const errors: FormErrors = {}
    if (!form.firstName.trim()) errors.firstName = 'Ime je obavezno.'
    if (!form.lastName.trim()) errors.lastName = 'Prezime je obavezno.'
    if (!form.phone.trim()) {
      errors.phone = 'Telefon je obavezan.'
    } else if (!/^\d{6,15}$/.test(form.phone)) {
      errors.phone = 'Telefon mora sadržati samo cifre (6-15 cifara).'
    }
    if (!form.email.trim()) {
      errors.email = 'Email je obavezan.'
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
      errors.email = 'Email adresa nije validna.'
    }
    setFieldErrors(errors)
    return Object.keys(errors).length === 0
  }

  const handleSubmit = async () => {
    if (!validate()) return
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
      setPhoneWarning('')
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
            className={`w-full bg-slate-800 text-white border rounded px-4 py-3 focus:outline-none focus:border-yellow-500 ${
              fieldErrors.firstName ? 'border-red-500' : 'border-slate-700'
            }`}
          />
          {fieldErrors.firstName && (
            <p className="text-red-400 text-xs mt-1">{fieldErrors.firstName}</p>
          )}
        </div>

        <div className="mb-4">
          <label className="block text-slate-400 text-sm mb-2">Prezime</label>
          <input
            name="lastName"
            value={form.lastName}
            onChange={handleChange}
            className={`w-full bg-slate-800 text-white border rounded px-4 py-3 focus:outline-none focus:border-yellow-500 ${
              fieldErrors.lastName ? 'border-red-500' : 'border-slate-700'
            }`}
          />
          {fieldErrors.lastName && (
            <p className="text-red-400 text-xs mt-1">{fieldErrors.lastName}</p>
          )}
        </div>

        <div className="mb-4">
          <label className="block text-slate-400 text-sm mb-2">Telefon</label>
          <input
            name="phone"
            value={form.phone}
            maxLength={15}
            onChange={(e) => {
              handleChange(e)
              if (e.target.value.length >= 15) {
                setPhoneWarning('Dostignuti maksimalan broj cifara (15).')
              } else {
                setPhoneWarning('')
              }
            }}
            onKeyDown={(e) => {
              if (
                !/[\d]/.test(e.key) &&
                !['Backspace', 'Delete', 'ArrowLeft', 'ArrowRight', 'Tab'].includes(e.key)
              ) {
                e.preventDefault()
                setPhoneWarning('Dozvoljeno je uneti samo cifre.')
              }
            }}
            className={`w-full bg-slate-800 text-white border rounded px-4 py-3 focus:outline-none focus:border-yellow-500 ${
              fieldErrors.phone ? 'border-red-500' : 'border-slate-700'
            }`}
          />
          {fieldErrors.phone && (
            <p className="text-red-400 text-xs mt-1">{fieldErrors.phone}</p>
          )}
          {phoneWarning && !fieldErrors.phone && (
            <p className="text-yellow-400 text-xs mt-1">{phoneWarning}</p>
          )}
        </div>

        <div className="mb-4">
          <label className="block text-slate-400 text-sm mb-2">Email</label>
          <input
            name="email"
            type="email"
            value={form.email}
            onChange={handleChange}
            className={`w-full bg-slate-800 text-white border rounded px-4 py-3 focus:outline-none focus:border-yellow-500 ${
              fieldErrors.email ? 'border-red-500' : 'border-slate-700'
            }`}
          />
          {fieldErrors.email && (
            <p className="text-red-400 text-xs mt-1">{fieldErrors.email}</p>
          )}
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

        <div className="flex gap-3">
          <button
            onClick={handleSubmit}
            disabled={loading}
            className="flex-1 bg-yellow-500 hover:bg-yellow-400 text-slate-950 font-bold py-3 rounded transition-colors disabled:opacity-50"
          >
            {loading ? 'Kreiranje...' : 'Kreiraj korisnika'}
          </button>
          <button
            onClick={() => navigate('/admin/users')}
            className="flex-1 bg-slate-700 hover:bg-slate-600 text-white font-bold py-3 rounded transition-colors"
          >
            Odustani
          </button>
        </div>
      </div>
    </div>
  )
}

export default CreateUserPage