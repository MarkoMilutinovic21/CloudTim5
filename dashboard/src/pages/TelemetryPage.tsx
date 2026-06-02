import { useEffect, useMemo, useState } from 'react'
import axios from 'axios'
import ReturnToDashboardButton from '../components/ReturnToDashboardButton'

interface TelemetryMeasurement {
  id: string
  deviceId: string
  hiveId: string
  deviceUuid: string
  weightKg: number
  temperatureC: number
  humidityPercent: number
  batteryPercent: number
  measuredAt: string
  receivedAt: string
}

interface DailyWeightDelta {
  date: string
  startWeightKg: number | null
  endWeightKg: number | null
  deltaKg: number | null
}

const apiBaseUrl = 'http://localhost:5108/api'

const defaultFromDate = () => {
  const date = new Date()
  date.setDate(date.getDate() - 7)
  return date.toISOString().slice(0, 10)
}

const todayDate = () => new Date().toISOString().slice(0, 10)

function TelemetryPage() {
  const [hiveId, setHiveId] = useState(localStorage.getItem('telemetryHiveId') ?? '')
  const [from, setFrom] = useState(defaultFromDate())
  const [to, setTo] = useState(todayDate())
  const [latest, setLatest] = useState<TelemetryMeasurement | null>(null)
  const [measurements, setMeasurements] = useState<TelemetryMeasurement[]>([])
  const [dailyDeltas, setDailyDeltas] = useState<DailyWeightDelta[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const token = localStorage.getItem('token')
  const headers = useMemo(() => ({ Authorization: `Bearer ${token}` }), [token])

  const fetchTelemetry = async () => {
    const trimmedHiveId = hiveId.trim()

    if (!trimmedHiveId) {
      setError('HiveId je obavezan.')
      return
    }

    setLoading(true)
    setError('')
    localStorage.setItem('telemetryHiveId', trimmedHiveId)

    try {
      const [latestResponse, measurementsResponse, deltaResponse] = await Promise.all([
        axios.get<TelemetryMeasurement>(
          `${apiBaseUrl}/hives/${trimmedHiveId}/telemetry/latest`,
          { headers },
        ),
        axios.get<TelemetryMeasurement[]>(
          `${apiBaseUrl}/hives/${trimmedHiveId}/telemetry/measurements`,
          { headers, params: { from, to } },
        ),
        axios.get<DailyWeightDelta[]>(
          `${apiBaseUrl}/hives/${trimmedHiveId}/telemetry/daily-weight-delta`,
          { headers, params: { from, to } },
        ),
      ])

      setLatest(latestResponse.data)
      setMeasurements(measurementsResponse.data)
      setDailyDeltas(deltaResponse.data)
    } catch (err: any) {
      console.error('Telemetry load failed:', err)

      if (err.response?.status === 404) {
        setLatest(null)
        setMeasurements([])
        setDailyDeltas([])
        setError('Nema merenja za ovu kosnicu.')
      } else if (err.response?.status === 401 || err.response?.status === 403) {
        setError('Nemate pristup telemetriji. Prijavite se kao pcelar.')
      } else if (err.response) {
        setError(`Greska pri ucitavanju telemetrije. Status: ${err.response.status}`)
      } else {
        setError('Backend nije dostupan.')
      }
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    if (hiveId.trim()) {
      fetchTelemetry()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  return (
    <div className="min-h-screen app-shell p-8">
      <div className="max-w-7xl mx-auto">
        <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between mb-6">
          <div>
            <h1 className="text-2xl font-bold text-white mt-2">Telemetrija kosnice</h1>
            <p className="text-slate-400 text-sm">
              Pregled merenja pametne vage i dnevnog unosa/potrosnje.
            </p>
          </div>
          <ReturnToDashboardButton />
        </div>

        <div className="bg-slate-900 border border-slate-800 rounded-xl p-5 mb-6">
          <div className="grid grid-cols-1 lg:grid-cols-[1.5fr_1fr_1fr_auto] gap-4 items-end">
            <div>
              <label className="block text-slate-400 text-sm mb-2">HiveId</label>
              <input
                value={hiveId}
                onChange={(event) => setHiveId(event.target.value)}
                className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                placeholder="2db9dabe-0023-4ae1-bb44-dcaa0cd95cf7"
              />
            </div>
            <div>
              <label className="block text-slate-400 text-sm mb-2">Od</label>
              <input
                type="date"
                value={from}
                onChange={(event) => setFrom(event.target.value)}
                className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
              />
            </div>
            <div>
              <label className="block text-slate-400 text-sm mb-2">Do</label>
              <input
                type="date"
                value={to}
                onChange={(event) => setTo(event.target.value)}
                className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
              />
            </div>
            <button
              onClick={fetchTelemetry}
              disabled={loading}
              className="bg-yellow-500 hover:bg-yellow-400 text-slate-950 font-bold px-6 py-3 rounded transition-colors disabled:opacity-50"
            >
              {loading ? 'Ucitavanje...' : 'Osvezi'}
            </button>
          </div>
        </div>

        {error && (
          <div className="bg-red-900/50 border border-red-700 text-red-300 px-4 py-3 rounded mb-6 text-sm">
            {error}
          </div>
        )}

        <StatusCards latest={latest} />

        <div className="grid grid-cols-1 xl:grid-cols-2 gap-6 mt-6">
          <section className="bg-slate-900 border border-slate-800 rounded-xl p-5">
            <div className="flex items-center justify-between mb-5">
              <h2 className="text-white font-bold text-lg">Temperatura i vlaznost</h2>
              <span className="text-slate-500 text-sm">{measurements.length} merenja</span>
            </div>
            <TrendChart measurements={measurements} />
          </section>

          <section className="bg-slate-900 border border-slate-800 rounded-xl p-5">
            <div className="flex items-center justify-between mb-5">
              <h2 className="text-white font-bold text-lg">Dnevni delta tezine</h2>
              <span className="text-slate-500 text-sm">{dailyDeltas.length} dana</span>
            </div>
            <DeltaChart deltas={dailyDeltas} />
          </section>
        </div>
      </div>
    </div>
  )
}

function StatusCards({ latest }: { latest: TelemetryMeasurement | null }) {
  const cards = [
    { label: 'Tezina', value: latest ? `${latest.weightKg.toFixed(2)} kg` : '-' },
    { label: 'Temperatura', value: latest ? `${latest.temperatureC.toFixed(2)} C` : '-' },
    { label: 'Vlaznost', value: latest ? `${latest.humidityPercent.toFixed(2)} %` : '-' },
    { label: 'Baterija', value: latest ? `${latest.batteryPercent.toFixed(2)} %` : '-' },
  ]

  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
      {cards.map((card) => (
        <div key={card.label} className="bg-slate-900 border border-slate-800 rounded-xl p-5">
          <p className="text-slate-400 text-sm">{card.label}</p>
          <p className="text-white text-2xl font-bold mt-2">{card.value}</p>
        </div>
      ))}
    </div>
  )
}

function TrendChart({ measurements }: { measurements: TelemetryMeasurement[] }) {
  if (measurements.length === 0) {
    return <EmptyChart message="Nema merenja za izabrani period." />
  }

  const sorted = [...measurements].sort(
    (a, b) => new Date(a.measuredAt).getTime() - new Date(b.measuredAt).getTime(),
  )

  const temperatures = sorted.map((item) => item.temperatureC)
  const humidities = sorted.map((item) => item.humidityPercent)
  const allValues = [...temperatures, ...humidities]
  const min = Math.min(...allValues)
  const max = Math.max(...allValues)

  const temperaturePoints = buildPolyline(temperatures, min, max)
  const humidityPoints = buildPolyline(humidities, min, max)

  return (
    <div>
      <svg viewBox="0 0 640 260" className="w-full h-72 bg-slate-950 rounded-lg">
        <line x1="40" y1="220" x2="620" y2="220" stroke="#334155" />
        <line x1="40" y1="24" x2="40" y2="220" stroke="#334155" />
        <polyline points={temperaturePoints} fill="none" stroke="#facc15" strokeWidth="3" />
        <polyline points={humidityPoints} fill="none" stroke="#38bdf8" strokeWidth="3" />
      </svg>
      <div className="flex gap-5 mt-3 text-sm">
        <span className="text-yellow-300">Temperatura</span>
        <span className="text-sky-300">Vlaznost</span>
      </div>
    </div>
  )
}

function DeltaChart({ deltas }: { deltas: DailyWeightDelta[] }) {
  const visible = deltas.filter((item) => item.deltaKg !== null)

  if (visible.length === 0) {
    return <EmptyChart message="Nema dovoljno merenja za dnevni delta." />
  }

  const maxAbs = Math.max(...visible.map((item) => Math.abs(item.deltaKg ?? 0)), 1)

  return (
    <div className="h-72 bg-slate-950 rounded-lg p-4 flex items-end gap-3 overflow-x-auto">
      {visible.map((item) => {
        const delta = item.deltaKg ?? 0
        const height = Math.max(12, (Math.abs(delta) / maxAbs) * 190)
        const isPositive = delta >= 0

        return (
          <div key={item.date} className="h-full min-w-16 flex flex-col justify-end items-center gap-2">
            <span className={isPositive ? 'text-green-300 text-xs' : 'text-red-300 text-xs'}>
              {delta.toFixed(2)}
            </span>
            <div
              className={isPositive ? 'w-8 bg-green-500 rounded-t' : 'w-8 bg-red-500 rounded-t'}
              style={{ height }}
              title={`${formatDate(item.date)}: ${delta.toFixed(2)} kg`}
            />
            <span className="text-slate-500 text-xs">{formatDate(item.date)}</span>
          </div>
        )
      })}
    </div>
  )
}

function EmptyChart({ message }: { message: string }) {
  return (
    <div className="h-72 bg-slate-950 rounded-lg flex items-center justify-center text-slate-500">
      {message}
    </div>
  )
}

function buildPolyline(values: number[], min: number, max: number) {
  const chartWidth = 580
  const chartHeight = 196
  const left = 40
  const top = 24
  const range = max - min || 1

  return values
    .map((value, index) => {
      const x = left + (values.length === 1 ? 0 : (index / (values.length - 1)) * chartWidth)
      const y = top + chartHeight - ((value - min) / range) * chartHeight
      return `${x.toFixed(1)},${y.toFixed(1)}`
    })
    .join(' ')
}

function formatDate(value: string) {
  return new Date(value).toLocaleDateString('sr-RS', {
    day: '2-digit',
    month: '2-digit',
  })
}

export default TelemetryPage
