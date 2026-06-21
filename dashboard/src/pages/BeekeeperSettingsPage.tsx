import { useEffect, useMemo, useState } from 'react'
import axios from 'axios'
import ReturnToDashboardButton from '../components/ReturnToDashboardButton'

const apiUrl = 'http://localhost:5108/api/beekeeper/settings'

export default function BeekeeperSettingsPage() {
  const [threshold, setThreshold] = useState('10')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const token = localStorage.getItem('token')
  const headers = useMemo(() => ({ Authorization: `Bearer ${token}` }), [token])

  useEffect(() => {
    let active = true
    axios.get<{ weightDropThresholdKg: number }>(apiUrl, { headers })
      .then(response => {
        if (active) setThreshold(String(response.data.weightDropThresholdKg))
      })
      .catch(() => {
        if (active) setError('Podešavanja nisu mogla da se učitaju.')
      })
      .finally(() => {
        if (active) setLoading(false)
      })
    return () => { active = false }
  }, [headers])

  const save = async () => {
    const value = Number(threshold)
    if (!Number.isFinite(value) || value <= 0 || value > 100) {
      setError('Prag mora biti između 0 i 100 kg.')
      return
    }

    setSaving(true)
    setError('')
    setMessage('')
    try {
      await axios.put(apiUrl, { weightDropThresholdKg: value }, { headers })
      setMessage('Podešavanja su sačuvana.')
    } catch {
      setError('Podešavanja nisu sačuvana.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="min-h-screen app-shell p-8">
      <div className="mx-auto max-w-2xl">
        <div className="mb-6 flex items-start justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-white">Podešavanja upozorenja</h1>
            <p className="mt-2 text-sm text-slate-400">
              Sistem šalje hitno upozorenje kada težina između dva merenja opadne za ovaj iznos.
            </p>
          </div>
          <ReturnToDashboardButton />
        </div>

        {error && <div className="mb-4 rounded border border-red-700 bg-red-900/50 p-3 text-red-300">{error}</div>}
        {message && <div className="mb-4 rounded border border-green-700 bg-green-900/50 p-3 text-green-300">{message}</div>}

        <section className="rounded-xl border border-slate-800 bg-slate-900 p-6">
          <label className="mb-2 block text-sm text-slate-300" htmlFor="weight-threshold">
            Prag naglog pada težine (kg)
          </label>
          <input
            id="weight-threshold"
            type="number"
            min="0.1"
            max="100"
            step="0.1"
            disabled={loading}
            value={threshold}
            onChange={event => setThreshold(event.target.value)}
            className="w-full rounded border border-slate-700 bg-slate-800 px-4 py-3 text-white"
          />
          <button
            type="button"
            disabled={loading || saving}
            onClick={save}
            className="mt-5 w-full rounded bg-yellow-500 py-3 font-bold text-slate-950 disabled:opacity-50"
          >
            {saving ? 'Čuvanje...' : 'Sačuvaj'}
          </button>
        </section>
      </div>
    </div>
  )
}
