import { useEffect, useMemo, useState } from 'react'
import axios from 'axios'
import ReturnToDashboardButton from '../components/ReturnToDashboardButton'

interface Parcel {
  id: string
  name: string
  area: number
  location: string
}

interface SprayingRecord {
  id: string
  startTime: string
  durationHours: number
  chemicalName: string
  parcelId: string
}

const apiBase = 'http://localhost:5108/api'

function SprayingRecordsPage() {
  const [parcels, setParcels] = useState<Parcel[]>([])
  const [records, setRecords] = useState<SprayingRecord[]>([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [windWarning, setWindWarning] = useState('')

  const [selectedParcelId, setSelectedParcelId] = useState('')
  const [startTime, setStartTime] = useState('')
  const [durationHours, setDurationHours] = useState('')
  const [chemicalName, setChemicalName] = useState('')

  const token = localStorage.getItem('token')
  const headers = useMemo(() => ({ Authorization: `Bearer ${token}` }), [token])

  const fetchParcels = async () => {
    try {
      const response = await axios.get(`${apiBase}/Parcels`, { headers })
      setParcels(response.data)
    } catch {
      setError('Greška pri učitavanju parcela.')
    } finally {
      setLoading(false)
    }
  }

  const fetchRecords = async (parcelId: string) => {
    try {
      const response = await axios.get(`${apiBase}/farmer/spraying-records/${parcelId}`, { headers })
      setRecords(response.data)
    } catch {
      setError('Greška pri učitavanju zapisa prskanja.')
    }
  }

  useEffect(() => {
    fetchParcels()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const createRecord = async () => {
    if (!selectedParcelId || !startTime || !durationHours || !chemicalName.trim()) {
      setError('Sva polja su obavezna.')
      return
    }

    setSaving(true)
    setError('')
    setSuccess('')
    setWindWarning('')

    try {
      const response = await axios.post(
        `${apiBase}/farmer/spraying-records`,
        {
          startTime: new Date(startTime).toISOString(),
          durationHours: Number(durationHours),
          chemicalName,
          parcelId: selectedParcelId,
        },
        { headers },
      )

      setSuccess('Zapis o prskanju uspješno sačuvan.')
      if (response.data.windWarning) {
        setWindWarning(response.data.windWarning)
      }
      setChemicalName('')
      setDurationHours('')
      setStartTime('')
      fetchRecords(selectedParcelId)
    } catch (err: any) {
      if (err.response?.status === 401 || err.response?.status === 403) {
        setError('Nemate pristup. Prijavite se kao farmer.')
      } else {
        setError('Greška pri čuvanju zapisa.')
      }
    } finally {
      setSaving(false)
    }
  }

  if (loading) {
    return (
      <div className="min-h-screen app-shell flex items-center justify-center text-white">
        Učitavanje...
      </div>
    )
  }

  return (
    <div className="min-h-screen app-shell p-8">
      <div className="max-w-4xl mx-auto">
        <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between mb-6">
          <div>
            <h1 className="text-2xl font-bold text-white mt-2">Digitalni karton prskanja</h1>
            <p className="text-slate-400 text-sm">Evidencija prskanja pesticidima po parcelama.</p>
          </div>
          <ReturnToDashboardButton />
        </div>

        {error && (
          <div className="bg-red-900/50 border border-red-700 text-red-300 px-4 py-3 rounded mb-4 text-sm">
            {error}
          </div>
        )}
        {success && (
          <div className="bg-green-900/50 border border-green-700 text-green-300 px-4 py-3 rounded mb-4 text-sm">
            {success}
          </div>
        )}
        {windWarning && (
          <div className="bg-yellow-900/50 border border-yellow-600 text-yellow-300 px-4 py-3 rounded mb-4 text-sm">
            ⚠ {windWarning}
          </div>
        )}

        <div className="bg-slate-900 border border-slate-800 rounded-xl p-5 mb-6">
          <h2 className="text-white font-bold text-lg mb-4">Novi zapis</h2>
          <div className="space-y-4">
            <div>
              <label className="block text-slate-400 text-sm mb-2">Parcela</label>
              <select
                value={selectedParcelId}
                onChange={(e) => {
                  const id = e.target.value
                  setSelectedParcelId(id)
                  if (id) fetchRecords(id)
                }}
                className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
              >
                <option value="">-- Izaberi parcelu --</option>
                {parcels.map((parcel) => (
                  <option key={parcel.id} value={parcel.id}>
                    {parcel.name}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-slate-400 text-sm mb-2">Datum i vreme prskanja</label>
              <input
                type="datetime-local"
                value={startTime}
                onChange={(e) => setStartTime(e.target.value)}
                className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
              />
            </div>
            <div>
              <label className="block text-slate-400 text-sm mb-2">Trajanje (sati)</label>
              <input
                type="number"
                min="0.5"
                step="0.5"
                value={durationHours}
                onChange={(e) => setDurationHours(e.target.value)}
                className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
              />
            </div>
            <div>
              <label className="block text-slate-400 text-sm mb-2">Preparat</label>
              <input
                type="text"
                value={chemicalName}
                onChange={(e) => setChemicalName(e.target.value)}
                className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                placeholder="Naziv pesticida ili preparata"
              />
            </div>
            <button
              onClick={createRecord}
              disabled={saving}
              className="w-full bg-yellow-500 hover:bg-yellow-400 text-slate-950 font-bold py-3 rounded transition-colors disabled:opacity-50"
            >
              {saving ? 'Čuvanje...' : 'Sačuvaj zapis'}
            </button>
          </div>
        </div>

        {records.length > 0 && (
          <section className="bg-slate-900 border border-slate-800 rounded-xl overflow-hidden">
            <div className="px-5 py-4 border-b border-slate-800">
              <h2 className="text-white font-bold">Istorija prskanja</h2>
              <p className="text-slate-500 text-sm">{records.length} zapisa</p>
            </div>
            <div className="divide-y divide-slate-800">
              {records.map((record) => (
                <div key={record.id} className="p-5 grid grid-cols-3 gap-4 text-sm">
                  <div>
                    <div className="text-slate-500 text-xs">Datum</div>
                    <div className="text-slate-200 font-semibold mt-1">
                      {new Date(record.startTime).toLocaleString('sr-RS')}
                    </div>
                  </div>
                  <div>
                    <div className="text-slate-500 text-xs">Trajanje</div>
                    <div className="text-slate-200 font-semibold mt-1">{record.durationHours} h</div>
                  </div>
                  <div>
                    <div className="text-slate-500 text-xs">Preparat</div>
                    <div className="text-slate-200 font-semibold mt-1">{record.chemicalName}</div>
                  </div>
                </div>
              ))}
            </div>
          </section>
        )}
      </div>
    </div>
  )
}

export default SprayingRecordsPage
