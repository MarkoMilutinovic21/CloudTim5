import { useEffect, useMemo, useState } from 'react'
import axios from 'axios'
import ReturnToDashboardButton from '../components/ReturnToDashboardButton'

interface Parcel { id: string; name: string }
interface SprayingRecord {
  id: string
  startTime: string
  endTime: string
  durationHours: number
  chemicalName: string
  parcelId: string
  parcelName: string
  cropName: string
  weatherDescription: string
  windSpeedMs: number | null
  hadPrecipitation: boolean
}

const apiBase = 'http://localhost:5108/api'
const dateValue = (offsetDays = 0) => {
  const date = new Date()
  date.setDate(date.getDate() + offsetDays)
  return date.toISOString().slice(0, 10)
}

export default function SprayingRecordsPage() {
  const [parcels, setParcels] = useState<Parcel[]>([])
  const [records, setRecords] = useState<SprayingRecord[]>([])
  const [selectedParcelId, setSelectedParcelId] = useState('')
  const [from, setFrom] = useState(dateValue(-30))
  const [to, setTo] = useState(dateValue())
  const [loading, setLoading] = useState(true)
  const [exporting, setExporting] = useState(false)
  const [error, setError] = useState('')
  const token = localStorage.getItem('token')
  const headers = useMemo(() => ({ Authorization: `Bearer ${token}` }), [token])

  useEffect(() => {
    let active = true
    axios.get<Parcel[]>(`${apiBase}/Parcels`, { headers })
      .then(response => {
        if (!active) return
        setParcels(response.data)
        setSelectedParcelId(response.data[0]?.id ?? '')
      })
      .catch(() => { if (active) setError('Parcele nisu mogle da se učitaju.') })
      .finally(() => { if (active) setLoading(false) })
    return () => { active = false }
  }, [headers])

  useEffect(() => {
    if (!selectedParcelId) {
      setRecords([])
      return
    }

    let active = true
    axios.get<SprayingRecord[]>(`${apiBase}/farmer/spraying-records/${selectedParcelId}`, {
      headers,
      params: { from, to },
    })
      .then(response => { if (active) setRecords(response.data) })
      .catch(() => { if (active) setError('Karton prskanja nije mogao da se učita.') })
    return () => { active = false }
  }, [selectedParcelId, from, to, headers])

  const exportPdf = async () => {
    if (!selectedParcelId) return
    setExporting(true)
    setError('')
    try {
      const response = await axios.get(
        `${apiBase}/farmer/spraying-records/${selectedParcelId}/export-pdf`,
        { headers, params: { from, to }, responseType: 'blob' },
      )
      const url = URL.createObjectURL(new Blob([response.data], { type: 'application/pdf' }))
      const link = document.createElement('a')
      link.href = url
      link.download = `karton-prskanja-${dateValue()}.pdf`
      link.click()
      URL.revokeObjectURL(url)
    } catch {
      setError('PDF nije mogao da se generiše.')
    } finally {
      setExporting(false)
    }
  }

  return (
    <div className="min-h-screen app-shell p-8">
      <div className="mx-auto max-w-6xl">
        <div className="mb-6 flex items-start justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-white">Digitalni karton prskanja</h1>
            <p className="mt-2 text-sm text-slate-400">
              Izvršeni, neotkazani tretmani automatski se evidentiraju nakon isteka termina.
            </p>
          </div>
          <ReturnToDashboardButton />
        </div>

        {error && <div className="mb-4 rounded border border-red-700 bg-red-900/50 p-3 text-red-300">{error}</div>}

        <section className="mb-6 grid gap-4 rounded-xl border border-slate-800 bg-slate-900 p-5 md:grid-cols-4">
          <label className="text-sm text-slate-400">
            Parcela
            <select value={selectedParcelId} onChange={event => setSelectedParcelId(event.target.value)}
              className="mt-2 w-full rounded border border-slate-700 bg-slate-800 px-3 py-2 text-white">
              <option value="">Izaberite parcelu</option>
              {parcels.map(parcel => <option key={parcel.id} value={parcel.id}>{parcel.name}</option>)}
            </select>
          </label>
          <label className="text-sm text-slate-400">Od
            <input type="date" value={from} onChange={event => setFrom(event.target.value)}
              className="mt-2 w-full rounded border border-slate-700 bg-slate-800 px-3 py-2 text-white" />
          </label>
          <label className="text-sm text-slate-400">Do
            <input type="date" value={to} onChange={event => setTo(event.target.value)}
              className="mt-2 w-full rounded border border-slate-700 bg-slate-800 px-3 py-2 text-white" />
          </label>
          <button type="button" onClick={exportPdf} disabled={exporting || records.length === 0}
            className="self-end rounded bg-yellow-500 px-4 py-2 font-bold text-slate-950 disabled:opacity-50">
            {exporting ? 'Generisanje...' : 'Izvezi PDF'}
          </button>
        </section>

        <section className="overflow-hidden rounded-xl border border-slate-800 bg-slate-900">
          {loading ? <p className="p-5 text-slate-400">Učitavanje...</p> : records.length === 0 ? (
            <p className="p-5 text-slate-400">Nema izvršenih tretmana za izabrani period.</p>
          ) : records.map(record => (
            <article key={record.id} className="grid gap-4 border-b border-slate-800 p-5 text-sm md:grid-cols-3">
              <div><span className="text-slate-500">Početak</span><p className="text-white">{new Date(record.startTime).toLocaleString('sr-RS')}</p></div>
              <div><span className="text-slate-500">Završetak</span><p className="text-white">{new Date(record.endTime).toLocaleString('sr-RS')}</p></div>
              <div><span className="text-slate-500">Preparat</span><p className="text-white">{record.chemicalName || 'Nije naveden'}</p></div>
              <div><span className="text-slate-500">Kultura</span><p className="text-white">{record.cropName || 'Nije evidentirana'}</p></div>
              <div><span className="text-slate-500">Vreme</span><p className="text-white">{record.weatherDescription || 'Nema podataka'}</p></div>
              <div><span className="text-slate-500">Vetar/padavine</span><p className="text-white">{record.windSpeedMs?.toFixed(1) ?? '—'} m/s · {record.hadPrecipitation ? 'da' : 'ne'}</p></div>
            </article>
          ))}
        </section>
      </div>
    </div>
  )
}
