import { useEffect, useMemo, useState } from 'react'
import axios from 'axios'
import ReturnToDashboardButton from '../components/ReturnToDashboardButton'

interface NotificationStatusItem {
  treatmentId: string
  parcelId: string
  parcelName: string
  plannedStartAt: string
  durationHours: number
  pesticideType: string
  treatmentStatus: string
  notifiedBeekeepersCount: number
  notificationCreatedAt: string
  notificationUpdatedAt?: string | null
  cancelledAt?: string | null
}

interface NotificationStatusOverview {
  totalTreatments: number
  scheduledTreatments: number
  cancelledTreatments: number
  completedTreatments: number
  totalNotifiedBeekeepers: number
  items: NotificationStatusItem[]
}

function NotificationStatusPage() {
  const [overview, setOverview] = useState<NotificationStatusOverview | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const token = localStorage.getItem('token')
  const headers = useMemo(() => ({ Authorization: `Bearer ${token}` }), [token])

  const fetchNotificationStatus = async () => {
    setLoading(true)
    setError('')

    try {
      const response = await axios.get<NotificationStatusOverview>(
        'http://localhost:5108/api/PesticideTreatments/notification-status',
        { headers }
      )

      setOverview(response.data)
    } catch (err: unknown) {
      const response = axios.isAxiosError(err) ? err.response : undefined
      console.error('Greska pri ucitavanju statusa obavestenja:', err)

      if (response) {
        setError(`Greska pri ucitavanju statusa obavestenja. Status: ${response.status}`)
      } else {
        setError('Greska pri ucitavanju statusa obavestenja. Backend nije dostupan.')
      }
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchNotificationStatus()
    // Initial load only; the refresh button invokes the same function.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const getTreatmentStatusLabel = (status: string) => {
    if (status === 'Cancelled') return 'Otkazano'
    if (status === 'Scheduled') return 'Zakazano'
    if (status === 'Completed') return 'Izvršeno'

    return status
  }

  const getNotificationLabel = (item: NotificationStatusItem) => {
    if (item.notifiedBeekeepersCount === 0) {
      return 'Nema obavestenih pcelara'
    }

    return `Obavesteno: ${item.notifiedBeekeepersCount}`
  }

  const getNotificationClasses = (item: NotificationStatusItem) => {
    if (item.notifiedBeekeepersCount === 0) {
      return 'bg-yellow-900/50 text-yellow-200 border-yellow-700'
    }

    return 'bg-green-900/50 text-green-300 border-green-700'
  }

  const getTreatmentStatusClasses = (status: string) => {
    if (status === 'Cancelled') {
      return 'bg-red-900/50 text-red-300 border-red-700'
    }

    return 'bg-blue-900/50 text-blue-300 border-blue-700'
  }

  const formatDateTime = (value?: string | null) => {
    if (!value) return '-'

    return new Date(value).toLocaleString('sr-RS', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    })
  }

  if (loading) {
    return (
      <div className="min-h-screen app-shell flex items-center justify-center text-white">
        Ucitavanje statusa obavestenja...
      </div>
    )
  }

  return (
    <div className="min-h-screen app-shell p-8">
      <div className="max-w-[1400px] mx-auto">
        <div className="mb-6 flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
          <div>
            <h1 className="text-3xl font-bold text-white mt-2">
              Status obavestenja
            </h1>

            <p className="text-slate-400 text-sm mt-1">
              Pregled broja pcelara koji su obavesteni nakon zakazivanja, izmene ili otkazivanja prskanja.
            </p>
          </div>
          <ReturnToDashboardButton />
        </div>

        {error && (
          <div className="bg-red-900/50 border border-red-700 text-red-300 px-4 py-3 rounded mb-4 text-sm">
            {error}
          </div>
        )}

        {overview && (
          <>
          <div className="grid grid-cols-1 md:grid-cols-5 gap-4 mb-6">
              <div className="bg-slate-900 border border-slate-800 rounded-xl p-5">
                <p className="text-slate-400 text-sm">Ukupno najava</p>
                <p className="text-3xl font-bold text-white mt-2">{overview.totalTreatments}</p>
              </div>

              <div className="bg-slate-900 border border-slate-800 rounded-xl p-5">
                <p className="text-slate-400 text-sm">Zakazano</p>
                <p className="text-3xl font-bold text-blue-300 mt-2">{overview.scheduledTreatments}</p>
              </div>

              <div className="bg-slate-900 border border-slate-800 rounded-xl p-5">
                <p className="text-slate-400 text-sm">Otkazano</p>
                <p className="text-3xl font-bold text-red-300 mt-2">{overview.cancelledTreatments}</p>
              </div>
              <div className="bg-slate-900 border border-slate-800 rounded-xl p-5">
                <p className="text-slate-400 text-sm">Izvršeno</p>
                <p className="text-3xl font-bold text-green-300 mt-2">{overview.completedTreatments}</p>
              </div>

              <div className="bg-slate-900 border border-slate-800 rounded-xl p-5">
                <p className="text-slate-400 text-sm">Ukupno obavestenih</p>
                <p className="text-3xl font-bold text-green-300 mt-2">{overview.totalNotifiedBeekeepers}</p>
              </div>
            </div>

            <div className="bg-slate-900 border border-slate-800 rounded-xl overflow-hidden">
              <div className="px-6 py-5 border-b border-slate-800">
                <h2 className="text-white font-bold text-xl">Pregled po najavi</h2>
                <p className="text-slate-400 text-sm mt-1">
                  Broj obavestenih se racuna za pcelare sa pcelinjakom u krugu od 5 km od parcele.
                </p>
              </div>

              {overview.items.length === 0 ? (
                <div className="p-6 text-slate-400 text-sm">
                  Jos uvek nemate najave tretiranja za prikaz statusa obavestenja.
                </div>
              ) : (
                <div className="p-3 overflow-x-auto">
                  <table className="w-full min-w-[980px] table-fixed text-sm">
                    <thead>
                      <tr className="border-b border-slate-800">
                        <th className="w-[18%] text-left text-slate-400 px-4 py-4 font-medium">
                          Parcela
                        </th>
                        <th className="w-[15%] text-left text-slate-400 px-4 py-4 font-medium">
                          Termin
                        </th>
                        <th className="w-[10%] text-left text-slate-400 px-4 py-4 font-medium">
                          Trajanje
                        </th>
                        <th className="w-[15%] text-left text-slate-400 px-4 py-4 font-medium">
                          Preparat
                        </th>
                        <th className="w-[15%] text-left text-slate-400 px-4 py-4 font-medium">
                          Obavestenje
                        </th>
                        <th className="w-[12%] text-left text-slate-400 px-4 py-4 font-medium">
                          Status najave
                        </th>
                        <th className="w-[15%] text-left text-slate-400 px-4 py-4 font-medium">
                          Poslednja promena
                        </th>
                      </tr>
                    </thead>

                    <tbody>
                      {overview.items.map((item) => (
                        <tr
                          key={item.treatmentId}
                          className="border-b border-slate-800 hover:bg-slate-800/50 align-top"
                        >
                          <td className="px-4 py-4 text-white font-medium whitespace-normal break-words">
                            {item.parcelName}
                          </td>

                          <td className="px-4 py-4 text-slate-300 whitespace-normal break-words">
                            {formatDateTime(item.plannedStartAt)}
                          </td>

                          <td className="px-4 py-4 text-slate-300 whitespace-normal break-words">
                            {item.durationHours} h
                          </td>

                          <td className="px-4 py-4 text-slate-300 whitespace-normal break-words">
                            {item.pesticideType || '-'}
                          </td>

                          <td className="px-4 py-4">
                            <span className={`inline-block border rounded-full px-3 py-1 text-xs ${getNotificationClasses(item)}`}>
                              {getNotificationLabel(item)}
                            </span>
                          </td>

                          <td className="px-4 py-4">
                            <span className={`inline-block border rounded-full px-3 py-1 text-xs ${getTreatmentStatusClasses(item.treatmentStatus)}`}>
                              {getTreatmentStatusLabel(item.treatmentStatus)}
                            </span>
                          </td>

                          <td className="px-4 py-4 text-slate-300 whitespace-normal break-words">
                            {formatDateTime(item.cancelledAt ?? item.notificationUpdatedAt ?? item.notificationCreatedAt)}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          </>
        )}
      </div>
    </div>
  )
}

export default NotificationStatusPage
