import { useEffect, useMemo, useState } from 'react'
import axios from 'axios'
import ReturnToDashboardButton from '../components/ReturnToDashboardButton'

interface Parcel {
  id: string
  name: string
  area: number
  location: string
  latitude: number
  longitude: number
  description: string
  ownerId: string
  createdAt: string
}

interface PesticideTreatment {
  id: string
  parcelId: string
  parcelName: string
  plannedStartAt: string
  durationHours: number
  pesticideType: string
  status: string
  notifiedBeekeepersCount: number
  createdAt: string
  updatedAt?: string | null
  cancelledAt?: string | null
}

interface TreatmentForm {
  parcelId: string
  plannedStartAt: string
  durationHours: string
  pesticideType: string
}

const emptyForm: TreatmentForm = {
  parcelId: '',
  plannedStartAt: '',
  durationHours: '',
  pesticideType: '',
}

function PesticideTreatmentsPage() {
  const [parcels, setParcels] = useState<Parcel[]>([])
  const [treatments, setTreatments] = useState<PesticideTreatment[]>([])
  const [form, setForm] = useState<TreatmentForm>(emptyForm)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const token = localStorage.getItem('token')
  const headers = useMemo(() => ({ Authorization: `Bearer ${token}` }), [token])

  const fetchData = async () => {
    setLoading(true)
    setError('')

    try {
      const [parcelsResponse, treatmentsResponse] = await Promise.all([
        axios.get<Parcel[]>('http://localhost:5108/api/Parcels', { headers }),
        axios.get<PesticideTreatment[]>('http://localhost:5108/api/PesticideTreatments', { headers }),
      ])

      setParcels(parcelsResponse.data)
      setTreatments(treatmentsResponse.data)
    } catch (err: any) {
      console.error('Greška pri učitavanju najava tretiranja:', err)

      if (err.response) {
        setError(`Greška pri učitavanju najava tretiranja. Status: ${err.response.status}`)
      } else {
        setError('Greška pri učitavanju najava tretiranja. Backend nije dostupan.')
      }
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchData()
  }, [])

  const handleChange = (
    event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    setForm({
      ...form,
      [event.target.name]: event.target.value,
    })
  }

  const validateForm = () => {
    if (!form.parcelId) return 'Morate izabrati parcelu.'
    if (!form.plannedStartAt) return 'Morate uneti datum i vreme početka prskanja.'

    const duration = Number(form.durationHours)

    if (Number.isNaN(duration) || duration <= 0) {
      return 'Trajanje mora biti veće od 0.'
    }

    if (duration > 24) {
      return 'Trajanje ne može biti duže od 24 sata.'
    }

    return ''
  }

  const handleSubmit = async () => {
    const validationError = validateForm()

    if (validationError) {
      setError(validationError)
      setSuccess('')
      return
    }

    setSaving(true)
    setError('')
    setSuccess('')

    const payload = {
      parcelId: form.parcelId,
      plannedStartAt: toUtcIsoString(form.plannedStartAt),
      durationHours: Number(form.durationHours),
      pesticideType: form.pesticideType,
    }

    try {
      const response = editingId
        ? await axios.put(
            `http://localhost:5108/api/PesticideTreatments/${editingId}`,
            payload,
            { headers }
          )
        : await axios.post(
            'http://localhost:5108/api/PesticideTreatments',
            payload,
            { headers }
          )

      setSuccess(response.data.message ?? 'Najava tretiranja je uspešno sačuvana.')
      setForm(emptyForm)
      setEditingId(null)

      await fetchData()
    } catch (err: any) {
      console.error('Greška pri čuvanju najave tretiranja:', err)

      if (err.response) {
        setError(`Greška pri čuvanju najave tretiranja. Status: ${err.response.status}`)
      } else {
        setError('Greška pri čuvanju najave tretiranja. Backend nije dostupan.')
      }
    } finally {
      setSaving(false)
    }
  }

  const handleEdit = (treatment: PesticideTreatment) => {
    setEditingId(treatment.id)

    setForm({
      parcelId: treatment.parcelId,
      plannedStartAt: toDateTimeLocalValue(treatment.plannedStartAt),
      durationHours: treatment.durationHours.toString(),
      pesticideType: treatment.pesticideType ?? '',
    })

    setError('')
    setSuccess('')
  }

  const handleCancelEdit = () => {
    setEditingId(null)
    setForm(emptyForm)
    setError('')
    setSuccess('')
  }

  const handleCancelTreatment = async (id: string) => {
    if (!confirm('Da li ste sigurni da želite da otkažete ovu najavu prskanja?')) return

    setError('')
    setSuccess('')

    try {
      const response = await axios.put(
        `http://localhost:5108/api/PesticideTreatments/${id}/cancel`,
        {},
        { headers }
      )

      setSuccess(response.data.message ?? 'Najava tretiranja je otkazana.')
      await fetchData()
    } catch (err: any) {
      console.error('Greška pri otkazivanju najave tretiranja:', err)

      if (err.response) {
        setError(`Greška pri otkazivanju najave tretiranja. Status: ${err.response.status}`)
      } else {
        setError('Greška pri otkazivanju najave tretiranja. Backend nije dostupan.')
      }
    }
  }

  const getStatusLabel = (status: string) => {
    if (status === 'Cancelled') return 'Otkazano'
    if (status === 'Scheduled') return 'Zakazano'

    return status
  }

  const getStatusClasses = (status: string) => {
    if (status === 'Cancelled') {
      return 'bg-red-900/50 text-red-300 border-red-700'
    }

    return 'bg-green-900/50 text-green-300 border-green-700'
  }

  const formatDateTime = (value: string) => {
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
        Učitavanje najava tretiranja...
      </div>
    )
  }

  return (
    <div className="min-h-screen app-shell p-8">
      <div className="max-w-[1500px] mx-auto">
        <div className="mb-6 flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
          <div>
            <h1 className="text-3xl font-bold text-white mt-2">
              Najave tretiranja pesticidima
            </h1>

            <p className="text-slate-400 text-sm mt-1">
              Kreiranje, pomeranje i otkazivanje najava prskanja za vaše parcele.
            </p>
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

        {parcels.length === 0 && (
          <div className="bg-yellow-900/40 border border-yellow-700 text-yellow-200 px-4 py-3 rounded mb-4 text-sm">
            Nemate nijednu registrovanu parcelu. Prvo dodajte parcelu, pa onda kreirajte najavu tretiranja.
          </div>
        )}

        <div className="grid grid-cols-1 xl:grid-cols-[360px_minmax(0,1fr)] gap-8">
          <div className="bg-slate-900 border border-slate-800 rounded-xl p-6">
            <h2 className="text-white font-bold text-xl mb-5">
              {editingId ? 'Izmena najave' : 'Nova najava'}
            </h2>

            <div className="space-y-4">
              <div>
                <label className="block text-slate-400 text-sm mb-2">Parcela</label>
                <select
                  name="parcelId"
                  value={form.parcelId}
                  onChange={handleChange}
                  className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                >
                  <option value="">Izaberite parcelu</option>

                  {parcels.map((parcel) => (
                    <option key={parcel.id} value={parcel.id}>
                      {parcel.name} — {parcel.location}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-slate-400 text-sm mb-2">
                  Datum i vreme početka
                </label>
                <input
                  name="plannedStartAt"
                  type="datetime-local"
                  value={form.plannedStartAt}
                  onChange={handleChange}
                  className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                />
              </div>

              <div>
                <label className="block text-slate-400 text-sm mb-2">
                  Očekivano trajanje u satima
                </label>
                <input
                  name="durationHours"
                  type="number"
                  min="0"
                  step="0.5"
                  value={form.durationHours}
                  onChange={handleChange}
                  className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                  placeholder="Unesite trajanje prskanja"
                />
              </div>

              <div>
                <label className="block text-slate-400 text-sm mb-2">
                  Tip preparata
                </label>
                <input
                  name="pesticideType"
                  value={form.pesticideType}
                  onChange={handleChange}
                  className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                  placeholder="Opcioni naziv preparata"
                />
              </div>

              <div className="flex gap-3 pt-1">
                <button
                  onClick={handleSubmit}
                  disabled={saving || parcels.length === 0}
                  className="flex-1 bg-yellow-500 hover:bg-yellow-400 text-slate-950 font-bold py-3 rounded transition-colors disabled:opacity-50"
                >
                  {saving ? 'Čuvanje...' : editingId ? 'Sačuvaj izmene' : 'Kreiraj najavu'}
                </button>

                {editingId && (
                  <button
                    onClick={handleCancelEdit}
                    className="flex-1 bg-slate-700 hover:bg-slate-600 text-white font-bold py-3 rounded transition-colors"
                  >
                    Odustani
                  </button>
                )}
              </div>
            </div>
          </div>

          <div className="bg-slate-900 border border-slate-800 rounded-xl overflow-hidden min-w-0">
            <div className="px-6 py-5 border-b border-slate-800">
              <h2 className="text-white font-bold text-xl">Moje najave</h2>
              <p className="text-slate-400 text-sm mt-1">
                Status obaveštenja prikazuje broj pčelara čiji su pčelinjaci u krugu od 5 km.
              </p>
            </div>

            {treatments.length === 0 ? (
              <div className="p-6 text-slate-400 text-sm">
                Još uvek nemate kreirane najave tretiranja.
              </div>
            ) : (
              <div className="p-3">
                <table className="w-full table-fixed text-sm">
                  <thead>
                    <tr className="border-b border-slate-800">
                      <th className="w-[20%] text-left text-slate-400 px-4 py-4 font-medium">
                        Parcela
                      </th>
                      <th className="w-[18%] text-left text-slate-400 px-4 py-4 font-medium">
                        Početak
                      </th>
                      <th className="w-[10%] text-left text-slate-400 px-4 py-4 font-medium">
                        Trajanje
                      </th>
                      <th className="w-[16%] text-left text-slate-400 px-4 py-4 font-medium">
                        Preparat
                      </th>
                      <th className="w-[12%] text-left text-slate-400 px-4 py-4 font-medium">
                        Obavešteno
                      </th>
                      <th className="w-[11%] text-left text-slate-400 px-4 py-4 font-medium">
                        Status
                      </th>
                      <th className="w-[13%] text-left text-slate-400 px-4 py-4 font-medium">
                        Akcije
                      </th>
                    </tr>
                  </thead>

                  <tbody>
                    {treatments.map((treatment) => (
                      <tr
                        key={treatment.id}
                        className="border-b border-slate-800 hover:bg-slate-800/50 align-top"
                      >
                        <td className="px-4 py-4 text-white font-medium whitespace-normal break-words">
                          {treatment.parcelName}
                        </td>

                        <td className="px-4 py-4 text-slate-300 whitespace-normal break-words">
                          {formatDateTime(treatment.plannedStartAt)}
                        </td>

                        <td className="px-4 py-4 text-slate-300 whitespace-normal break-words">
                          {treatment.durationHours} h
                        </td>

                        <td className="px-4 py-4 text-slate-300 whitespace-normal break-words">
                          {treatment.pesticideType || '-'}
                        </td>

                        <td className="px-4 py-4 text-slate-300 whitespace-normal break-words">
                          {treatment.notifiedBeekeepersCount}
                        </td>

                        <td className="px-4 py-4">
                          <span className={`inline-block border rounded-full px-3 py-1 text-xs ${getStatusClasses(treatment.status)}`}>
                            {getStatusLabel(treatment.status)}
                          </span>
                        </td>

                        <td className="px-4 py-4">
                          <div className="flex flex-col 2xl:flex-row gap-2">
                            <button
                              onClick={() => handleEdit(treatment)}
                              disabled={treatment.status === 'Cancelled'}
                              className="bg-blue-600 hover:bg-blue-500 text-white text-xs px-3 py-1.5 rounded transition-colors disabled:opacity-50"
                            >
                              Izmeni
                            </button>

                            <button
                              onClick={() => handleCancelTreatment(treatment.id)}
                              disabled={treatment.status === 'Cancelled'}
                              className="bg-red-600 hover:bg-red-500 text-white text-xs px-3 py-1.5 rounded transition-colors disabled:opacity-50"
                            >
                              Otkaži
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}

function toDateTimeLocalValue(value: string) {
  const date = new Date(value)
  const offset = date.getTimezoneOffset()
  const localDate = new Date(date.getTime() - offset * 60 * 1000)

  return localDate.toISOString().slice(0, 16)
}

function toUtcIsoString(localDateTimeValue: string) {
  return new Date(localDateTimeValue).toISOString()
}

export default PesticideTreatmentsPage
