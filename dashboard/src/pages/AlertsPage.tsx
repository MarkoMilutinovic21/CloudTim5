import { useEffect, useMemo, useState } from 'react'
import axios from 'axios'
import ReturnToDashboardButton from '../components/ReturnToDashboardButton'

interface BeekeeperAlert {
  id: string
  type: string
  title: string
  message: string
  createdAt: string
}

function AlertsPage() {
  const [alerts, setAlerts] = useState<BeekeeperAlert[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const token = localStorage.getItem('token')
  const headers = useMemo(() => ({ Authorization: `Bearer ${token}` }), [token])

  const fetchAlerts = async () => {
    setLoading(true)
    setError('')

    try {
      const response = await axios.get<BeekeeperAlert[]>(
        'http://localhost:5108/api/Alerts',
        { headers },
      )

      setAlerts(response.data)
    } catch (err: any) {
      console.error('Greska pri ucitavanju upozorenja:', err)

      if (err.response?.status === 401 || err.response?.status === 403) {
        setError('Nemate pristup upozorenjima. Prijavite se kao pcelar.')
      } else if (err.response) {
        setError(`Greska pri ucitavanju upozorenja. Status: ${err.response.status}`)
      } else {
        setError('Backend nije dostupan.')
      }
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchAlerts()
  }, [])

  const formatDateTime = (value: string) =>
    new Date(value).toLocaleString('sr-RS', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    })

  const getTypeLabel = (type: string) => {
    if (type === 'WeightDrop') return 'Pad tezine'
    if (type === 'LowBattery') return 'Niska baterija'
    if (type === 'PesticideTreatment') return 'Pesticidi u blizini'
    return type
  }

  const getTypeClasses = (type: string) => {
    if (type === 'WeightDrop') return 'bg-red-900/50 text-red-300 border-red-700'
    if (type === 'LowBattery') return 'bg-yellow-900/50 text-yellow-200 border-yellow-700'
    if (type === 'PesticideTreatment') return 'bg-orange-900/50 text-orange-300 border-orange-700'
    return 'bg-slate-900 text-slate-300 border-slate-700'
  }

  if (loading) {
    return (
      <div className="min-h-screen app-shell flex items-center justify-center text-white">
        Ucitavanje upozorenja...
      </div>
    )
  }

  return (
    <div className="min-h-screen app-shell p-8">
      <div className="max-w-5xl mx-auto">
        <div className="mb-6 flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
          <div>
            <h1 className="text-3xl font-bold text-white mt-2">Hitna upozorenja</h1>
            <p className="text-slate-400 text-sm mt-1">
              Pregled rizika za pcelinjak i kosnice na osnovu telemetrije i najava tretiranja.
            </p>
          </div>
          <ReturnToDashboardButton />
        </div>

        {error && (
          <div className="bg-red-900/50 border border-red-700 text-red-300 px-4 py-3 rounded mb-4 text-sm">
            {error}
          </div>
        )}

        {!error && alerts.length === 0 && (
          <div className="bg-slate-900 border border-slate-800 rounded-xl p-6 text-slate-400 text-sm">
            Trenutno nema upozorenja za prikaz.
          </div>
        )}

        {!error && alerts.length > 0 && (
          <div className="grid grid-cols-1 gap-4">
            {alerts.map(alert => (
              <div key={alert.id} className="bg-slate-900 border border-slate-800 rounded-xl p-5">
                <div className="flex flex-wrap items-center justify-between gap-2 mb-2">
                  <span className={`border rounded-full px-3 py-1 text-xs ${getTypeClasses(alert.type)}`}>
                    {getTypeLabel(alert.type)}
                  </span>
                  <span className="text-slate-400 text-xs">{formatDateTime(alert.createdAt)}</span>
                </div>
                <h2 className="text-white font-semibold text-sm">{alert.title}</h2>
                <p className="text-slate-300 text-sm whitespace-pre-line mt-2">{alert.message}</p>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}

export default AlertsPage
