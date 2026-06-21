import { useEffect, useMemo, useRef, useState } from 'react'
import axios from 'axios'
import * as signalR from '@microsoft/signalr'
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

interface ApiaryOption {
  id: string
  name: string
}

interface HiveOption {
  id: string
  name: string
  apiaryId: string
}

const apiBaseUrl = 'http://localhost:5108/api'
const hubUrl = 'http://localhost:5108/telemetryhub'

const defaultFromDate = () => {
  const date = new Date()
  date.setDate(date.getDate() - 7)
  return date.toISOString().slice(0, 10)
}

const todayDate = () => new Date().toISOString().slice(0, 10)

function TelemetryPage() {
  const [apiaries, setApiaries] = useState<ApiaryOption[]>([])
  const [hives, setHives] = useState<HiveOption[]>([])
  const [apiaryId, setApiaryId] = useState(localStorage.getItem('telemetryApiaryId') ?? '')
  const [hiveId, setHiveId] = useState(localStorage.getItem('telemetryHiveId') ?? '')
  const [from, setFrom] = useState(defaultFromDate())
  const [to, setTo] = useState(todayDate())
  const [latest, setLatest] = useState<TelemetryMeasurement | null>(null)
  const [measurements, setMeasurements] = useState<TelemetryMeasurement[]>([])
  const [dailyDeltas, setDailyDeltas] = useState<DailyWeightDelta[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const connectionRef = useRef<signalR.HubConnection | null>(null)
  const joinedHiveRef = useRef<string>('')
  const [signalRReady, setSignalRReady] = useState(false)

  const token = localStorage.getItem('token')
  const headers = useMemo(() => ({ Authorization: `Bearer ${token}` }), [token])

  // Pokreni SignalR konekciju jednom
  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => token ?? '' })
      .withAutomaticReconnect()
      .build()

    connection.on('NewMeasurement', (m: TelemetryMeasurement) => {
      setLatest(m)
      setMeasurements(prev => prev.some(x => x.id === m.id) ? prev : [m, ...prev])
    })

    connection.start()
      .then(() => {
        connectionRef.current = connection
        setSignalRReady(true)
      })
      .catch((err: unknown) => console.warn('[SignalR] Konekcija nije uspostavljena:', err))

    return () => {
      connection.stop()
    }
  }, [token])

  // Promijeni grupu kada se promijeni kosnica
  useEffect(() => {
    if (!signalRReady || !hiveId.trim()) return

    const conn = connectionRef.current
    if (!conn) return

    const changeGroup = async () => {
      if (joinedHiveRef.current && joinedHiveRef.current !== hiveId) {
        await conn.invoke('LeaveHiveGroup', joinedHiveRef.current).catch(() => {})
      }
      await conn.invoke('JoinHiveGroup', hiveId).catch(() => {})
      joinedHiveRef.current = hiveId
    }

    changeGroup()
  }, [hiveId, signalRReady])

  const fetchApiaries = async () => {
    const response = await axios.get<ApiaryOption[]>(`${apiBaseUrl}/Apiaries`, { headers })
    setApiaries(response.data)

    const savedApiaryId = localStorage.getItem('telemetryApiaryId')
    const initialApiaryId = savedApiaryId || response.data[0]?.id || ''

    if (initialApiaryId) {
      setApiaryId(initialApiaryId)
      localStorage.setItem('telemetryApiaryId', initialApiaryId)
      await fetchHives(initialApiaryId)
    }
  }

  const fetchHives = async (selectedApiaryId: string) => {
    const response = await axios.get<HiveOption[]>(`${apiBaseUrl}/Hives/${selectedApiaryId}`, { headers })
    setHives(response.data)

    const savedHiveId = localStorage.getItem('telemetryHiveId')
    const savedHiveExists = response.data.some((hive) => hive.id === savedHiveId)
    const initialHiveId = savedHiveExists ? savedHiveId! : response.data[0]?.id || ''

    setHiveId(initialHiveId)
    if (initialHiveId) localStorage.setItem('telemetryHiveId', initialHiveId)
  }

  const fetchTelemetry = async () => {
    const trimmedHiveId = hiveId.trim()

    if (!trimmedHiveId) {
      setError('Izaberite kosnicu.')
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
    } catch (err: unknown) {
      const response = axios.isAxiosError(err) ? err.response : undefined
      console.error('Telemetry load failed:', err)

      if (response?.status === 404) {
        setLatest(null)
        setMeasurements([])
        setDailyDeltas([])
        setError('Nema merenja za ovu kosnicu.')
      } else if (response?.status === 401 || response?.status === 403) {
        setError('Nemate pristup telemetriji. Prijavite se kao pcelar.')
      } else if (response) {
        setError(`Greska pri ucitavanju telemetrije. Status: ${response.status}`)
      } else {
        setError('Backend nije dostupan.')
      }
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchApiaries().catch((err) => {
      console.error('Hive options load failed:', err)
      setError('Greska pri ucitavanju pcelinjaka i kosnica.')
    })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  useEffect(() => {
    if (!hiveId.trim()) return
    fetchTelemetry()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [hiveId])

  const handleApiaryChange = async (nextApiaryId: string) => {
    setApiaryId(nextApiaryId)
    setHiveId('')
    setHives([])
    setLatest(null)
    setMeasurements([])
    setDailyDeltas([])
    localStorage.setItem('telemetryApiaryId', nextApiaryId)
    localStorage.removeItem('telemetryHiveId')

    if (nextApiaryId) {
      await fetchHives(nextApiaryId)
    }
  }

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
          <div className="grid grid-cols-1 lg:grid-cols-[1.2fr_1.2fr_1fr_1fr_auto] gap-4 items-end">
            <div className="lg:col-span-2">
              <label className="block text-slate-400 text-sm mb-2">Pčelinjak</label>
              <div className="flex flex-wrap gap-2">
                {apiaries.map((apiary) => (
                  <button
                    key={apiary.id}
                    type="button"
                    onClick={() => handleApiaryChange(apiary.id)}
                    className={apiaryId === apiary.id
                      ? 'rounded bg-yellow-500 px-4 py-3 font-bold text-slate-950'
                      : 'rounded border border-slate-700 bg-slate-800 px-4 py-3 text-slate-200'}
                  >
                    {apiary.name}
                  </button>
                ))}
              </div>
            </div>
            <div>
              <label className="block text-slate-400 text-sm mb-2">Kosnica</label>
              <select
                value={hiveId}
                onChange={(event) => setHiveId(event.target.value)}
                className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
              >
                <option value="">Izaberite kosnicu</option>
                {hives.map((hive) => (
                  <option key={hive.id} value={hive.id}>
                    {hive.name}
                  </option>
                ))}
              </select>
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
    <div className="relative h-72 bg-slate-950 rounded-lg p-4 flex gap-3 overflow-x-auto">
      <div className="pointer-events-none absolute left-4 right-4 top-1/2 border-t border-slate-600" />
      {visible.map((item) => {
        const delta = item.deltaKg ?? 0
        const height = Math.max(12, (Math.abs(delta) / maxAbs) * 190)
        const isPositive = delta >= 0

        return (
          <div key={item.date} className="relative h-full min-w-16">
            <div
              className={isPositive
                ? 'absolute bottom-1/2 left-1/2 w-8 -translate-x-1/2 bg-green-500 rounded-t'
                : 'absolute top-1/2 left-1/2 w-8 -translate-x-1/2 bg-red-500 rounded-b'}
              style={{ height: height / 2 }}
              title={`${formatDate(item.date)}: ${delta.toFixed(2)} kg`}
            />
            <span className={isPositive
              ? 'absolute bottom-[52%] left-1/2 -translate-x-1/2 text-green-300 text-xs'
              : 'absolute top-[52%] left-1/2 -translate-x-1/2 text-red-300 text-xs'}>
              {delta.toFixed(2)}
            </span>
            <span className="absolute bottom-0 left-1/2 -translate-x-1/2 text-slate-500 text-xs">
              {formatDate(item.date)}
            </span>
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
