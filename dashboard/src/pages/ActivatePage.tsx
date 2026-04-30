import { useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import axios from 'axios'

function ActivatePage() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token') ?? ''

  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [fieldErrors, setFieldErrors] = useState<{ password?: string; confirmPassword?: string }>({})
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const validate = (): boolean => {
    const errors: { password?: string; confirmPassword?: string } = {}
    if (!password) {
      errors.password = 'Lozinka je obavezna.'
    } else if (password.length < 8) {
      errors.password = 'Lozinka mora imati najmanje 8 karaktera.'
    }
    if (!confirmPassword) {
      errors.confirmPassword = 'Potvrda lozinke je obavezna.'
    } else if (password !== confirmPassword) {
      errors.confirmPassword = 'Lozinke se ne poklapaju.'
    }
    setFieldErrors(errors)
    return Object.keys(errors).length === 0
  }

  const handleSubmit = async () => {
    if (!validate()) return
    setLoading(true)
    setError('')
    try {
      await axios.post('http://localhost:5108/api/Auth/activate', {
        token,
        password,
        confirmPassword,
      })
      navigate('/login')
    } catch {
      setError('Token nije validan ili je istekao.')
    } finally {
      setLoading(false)
    }
  }

  if (!token) {
    return (
      <div className="min-h-screen bg-slate-950 flex items-center justify-center">
        <div className="bg-slate-900 p-8 rounded-xl shadow-xl w-full max-w-md">
          <p className="text-red-400 text-center">Link za aktivaciju nije validan.</p>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center">
      <div className="bg-slate-900 p-8 rounded-xl shadow-xl w-full max-w-md">
        <h1 className="text-2xl font-bold text-white mb-2">Aktivacija naloga</h1>
        <p className="text-slate-400 text-sm mb-6">Unesite lozinku za vaš novi nalog.</p>

        {error && (
          <div className="bg-red-900/50 border border-red-700 text-red-300 px-4 py-3 rounded mb-4 text-sm">
            {error}
          </div>
        )}

        <div className="mb-4">
          <label className="block text-slate-400 text-sm mb-2">Lozinka</label>
          <input
            type="password"
            value={password}
            onChange={(e) => {
              setPassword(e.target.value)
              setFieldErrors({ ...fieldErrors, password: undefined })
            }}
            className={`w-full bg-slate-800 text-white border rounded px-4 py-3 focus:outline-none focus:border-yellow-500 ${
              fieldErrors.password ? 'border-red-500' : 'border-slate-700'
            }`}
            placeholder="••••••••"
          />
          {fieldErrors.password && (
            <p className="text-red-400 text-xs mt-1">{fieldErrors.password}</p>
          )}
        </div>

        <div className="mb-6">
          <label className="block text-slate-400 text-sm mb-2">Potvrdi lozinku</label>
          <input
            type="password"
            value={confirmPassword}
            onChange={(e) => {
              setConfirmPassword(e.target.value)
              setFieldErrors({ ...fieldErrors, confirmPassword: undefined })
            }}
            className={`w-full bg-slate-800 text-white border rounded px-4 py-3 focus:outline-none focus:border-yellow-500 ${
              fieldErrors.confirmPassword ? 'border-red-500' : 'border-slate-700'
            }`}
            placeholder="••••••••"
          />
          {fieldErrors.confirmPassword && (
            <p className="text-red-400 text-xs mt-1">{fieldErrors.confirmPassword}</p>
          )}
        </div>

        <button
          onClick={handleSubmit}
          disabled={loading}
          className="w-full bg-yellow-500 hover:bg-yellow-400 text-slate-950 font-bold py-3 rounded transition-colors disabled:opacity-50"
        >
          {loading ? 'Aktivacija...' : 'Aktiviraj nalog'}
        </button>
      </div>
    </div>
  )
}

export default ActivatePage