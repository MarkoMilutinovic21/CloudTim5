import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import axios from 'axios'

function LoginPage() {
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    localStorage.clear()
  }, [])

  const handleLogin = async () => {
    setLoading(true)
    setError('')
    try {
      const response = await axios.post('http://localhost:5108/api/Auth/login', {
        email,
        password,
      })
      localStorage.setItem('token', response.data.token)
      localStorage.setItem('role', response.data.role)
      localStorage.setItem('name', response.data.fullName)
      window.location.href = '/'
    } catch {
      setError('Pogrešan email ili lozinka.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center">
      <div className="bg-slate-900 p-8 rounded-xl shadow-xl w-full max-w-md">
        <h1 className="text-3xl font-black text-center mb-2 bg-gradient-to-r from-yellow-400 to-orange-400 bg-clip-text text-transparent">
          Smart Apiary
        </h1>
        <p className="text-slate-400 text-center text-sm mb-8">Prijavite se na platformu</p>

        {error && (
          <div className="bg-red-900/50 border border-red-700 text-red-300 px-4 py-3 rounded mb-4 text-sm">
            {error}
          </div>
        )}

        <div className="mb-4">
          <label className="block text-slate-400 text-sm mb-2">Email</label>
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
            placeholder="admin@smartapiary.com"
          />
        </div>

        <div className="mb-4">
          <label className="block text-slate-400 text-sm mb-2">Lozinka</label>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
            placeholder="••••••••"
            onKeyDown={(e) => e.key === 'Enter' && handleLogin()}
          />
        </div>

        <div className="mb-6 text-right">
          <button
            onClick={() => navigate('/forgot-password')}
            className="text-yellow-500 hover:text-yellow-400 text-sm transition-colors"
          >
            Zaboravili ste lozinku?
          </button>
        </div>

        <button
          onClick={handleLogin}
          disabled={loading}
          className="w-full bg-yellow-500 hover:bg-yellow-400 text-slate-950 font-bold py-3 rounded transition-colors disabled:opacity-50"
        >
          {loading ? 'Prijava...' : 'Prijavi se'}
        </button>
      </div>
    </div>
  )
}

export default LoginPage