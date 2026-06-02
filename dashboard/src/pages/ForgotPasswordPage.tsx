import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import axios from 'axios'

function ForgotPasswordPage() {
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [success, setSuccess] = useState(false)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const handleSubmit = async () => {
    if (!email.trim()) {
      setError('Email je obavezan.')
      return
    }
    setLoading(true)
    setError('')
    try {
      await axios.post('http://localhost:5108/api/Auth/forgot-password', { email })
      setSuccess(true)
    } catch {
      setError('Greška pri slanju zahteva.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen app-shell flex items-center justify-center">
      <div className="bg-slate-900 p-8 rounded-xl shadow-xl w-full max-w-md">
        <h1 className="text-2xl font-bold text-white mb-2">Zaboravljena lozinka</h1>
        <p className="text-slate-400 text-sm mb-6">Unesite vaš email i poslaćemo vam link za resetovanje.</p>

        {success ? (
          <div className="bg-green-900/50 border border-green-700 text-green-300 px-4 py-3 rounded mb-4 text-sm">
            Ukoliko nalog postoji, poslaćemo email sa linkom za resetovanje.
          </div>
        ) : (
          <>
            {error && (
              <div className="bg-red-900/50 border border-red-700 text-red-300 px-4 py-3 rounded mb-4 text-sm">
                {error}
              </div>
            )}

            <div className="mb-6">
              <label className="block text-slate-400 text-sm mb-2">Email</label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                placeholder="vas@email.com"
              />
            </div>

            <div className="flex gap-3">
              <button
                onClick={handleSubmit}
                disabled={loading}
                className="flex-1 bg-yellow-500 hover:bg-yellow-400 text-slate-950 font-bold py-3 rounded transition-colors disabled:opacity-50"
              >
                {loading ? 'Slanje...' : 'Pošalji link'}
              </button>
              <button
                onClick={() => navigate('/login')}
                className="flex-1 bg-slate-700 hover:bg-slate-600 text-white font-bold py-3 rounded transition-colors"
              >
                Nazad
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  )
}

export default ForgotPasswordPage
