import { useEffect, useMemo, useState } from 'react'
import axios from 'axios'
import ReturnToDashboardButton from '../components/ReturnToDashboardButton'
import { MapContainer, Marker, Popup, TileLayer } from 'react-leaflet'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'

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
  cropName: string
  floweringStart: string | null
  floweringEnd: string | null
  cropNotes: string
}

interface ParcelForm {
  name: string
  area: string
  location: string
  latitude: string
  longitude: string
  description: string
}

const emptyForm: ParcelForm = {
  name: '',
  area: '',
  location: '',
  latitude: '',
  longitude: '',
  description: '',
}

const emptyCropForm = {
  parcelId: '',
  cropName: '',
  floweringStart: '',
  floweringEnd: '',
  cropNotes: '',
}

const markerIcon = new L.Icon({
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
})

const parcelIcon = (cropName: string) => {
  if (!cropName) return markerIcon
  const normalized = cropName.toLocaleLowerCase('sr-RS')
  const symbol = normalized.includes('suncokret')
    ? '🌻'
    : normalized.includes('lavand')
      ? '🪻'
      : normalized.includes('repic')
        ? '🌼'
        : '🌱'
  return L.divIcon({
    html: `<span style="font-size:30px;filter:drop-shadow(0 2px 2px #111)">${symbol}</span>`,
    className: '',
    iconSize: [34, 40],
    iconAnchor: [17, 36],
  })
}

function ParcelsPage() {
  const [parcels, setParcels] = useState<Parcel[]>([])
  const [form, setForm] = useState<ParcelForm>(emptyForm)
  const [cropForm, setCropForm] = useState(emptyCropForm)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const token = localStorage.getItem('token')
  const headers = { Authorization: `Bearer ${token}` }

  const mapCenter = useMemo<[number, number]>(() => {
    if (parcels.length > 0) {
      return [parcels[0].latitude, parcels[0].longitude]
    }

    return [44.7866, 20.4489]
  }, [parcels])

  const fetchParcels = async () => {
    setLoading(true)
    setError('')

    try {
      const response = await axios.get<Parcel[]>('http://localhost:5108/api/Parcels', {
        headers,
      })

      setParcels(response.data)
    } catch (err: unknown) {
      const response = axios.isAxiosError(err) ? err.response : undefined
      console.error('Greška pri učitavanju parcela:', err)

      if (response) {
        setError(`Greška pri učitavanju parcela. Status: ${response.status}`)
      } else {
        setError('Greška pri učitavanju parcela. Backend nije dostupan.')
      }
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchParcels()
    // Initial load only.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const handleChange = (event: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    setForm({
      ...form,
      [event.target.name]: event.target.value,
    })
  }

  const validateForm = () => {
    if (!form.name.trim()) return 'Naziv parcele je obavezan.'
    if (!form.location.trim()) return 'Lokacija je obavezna.'

    const area = Number(form.area)
    const latitude = Number(form.latitude)
    const longitude = Number(form.longitude)

    if (Number.isNaN(area) || area <= 0) return 'Površina mora biti veća od 0.'

    if (Number.isNaN(latitude) || latitude < -90 || latitude > 90) {
      return 'Latitude mora biti između -90 i 90.'
    }

    if (Number.isNaN(longitude) || longitude < -180 || longitude > 180) {
      return 'Longitude mora biti između -180 i 180.'
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
      name: form.name,
      area: Number(form.area),
      location: form.location,
      latitude: Number(form.latitude),
      longitude: Number(form.longitude),
      description: form.description,
    }

    try {
      if (editingId) {
        await axios.put(`http://localhost:5108/api/Parcels/${editingId}`, payload, { headers })
        setSuccess('Parcela je uspešno izmenjena.')
      } else {
        await axios.post('http://localhost:5108/api/Parcels', payload, { headers })
        setSuccess('Parcela je uspešno dodata.')
      }

      setForm(emptyForm)
      setEditingId(null)
      await fetchParcels()
    } catch (err: unknown) {
      const response = axios.isAxiosError(err) ? err.response : undefined
      console.error('Greška pri čuvanju parcele:', err)

      if (response) {
        setError(`Greška pri čuvanju parcele. Status: ${response.status}`)
      } else {
        setError('Greška pri čuvanju parcele. Backend nije dostupan.')
      }
    } finally {
      setSaving(false)
    }
  }

  const handleEdit = (parcel: Parcel) => {
    setEditingId(parcel.id)
    setForm({
      name: parcel.name,
      area: parcel.area.toString(),
      location: parcel.location,
      latitude: parcel.latitude.toString(),
      longitude: parcel.longitude.toString(),
      description: parcel.description,
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

  const handleDelete = async (id: string) => {
    if (!confirm('Da li ste sigurni da želite da obrišete parcelu?')) return

    try {
      await axios.delete(`http://localhost:5108/api/Parcels/${id}`, { headers })
      setSuccess('Parcela je uspešno obrisana.')
      await fetchParcels()
    } catch (err: unknown) {
      const response = axios.isAxiosError(err) ? err.response : undefined
      console.error('Greška pri brisanju parcele:', err)

      if (response) {
        setError(`Greška pri brisanju parcele. Status: ${response.status}`)
      } else {
        setError('Greška pri brisanju parcele. Backend nije dostupan.')
      }
    }
  }

  const selectCropParcel = (parcel: Parcel) => {
    setCropForm({
      parcelId: parcel.id,
      cropName: parcel.cropName ?? '',
      floweringStart: parcel.floweringStart ? parcel.floweringStart.slice(0, 10) : '',
      floweringEnd: parcel.floweringEnd ? parcel.floweringEnd.slice(0, 10) : '',
      cropNotes: parcel.cropNotes ?? '',
    })
    setError('')
    setSuccess('')
  }

  const handleCropSubmit = async () => {
    if (!cropForm.parcelId) {
      setError('Izaberite parcelu za evidenciju kulture.')
      setSuccess('')
      return
    }

    if (!cropForm.cropName.trim() || !cropForm.floweringStart || !cropForm.floweringEnd) {
      setError('Naziv kulture i period cvetanja su obavezni.')
      setSuccess('')
      return
    }

    try {
      await axios.put(
        `http://localhost:5108/api/Parcels/${cropForm.parcelId}/crop`,
        {
          cropName: cropForm.cropName,
          floweringStart: new Date(cropForm.floweringStart).toISOString(),
          floweringEnd: new Date(cropForm.floweringEnd).toISOString(),
          cropNotes: cropForm.cropNotes,
        },
        { headers },
      )

      setCropForm(emptyCropForm)
      setSuccess('Kultura je uspesno evidentirana.')
      await fetchParcels()
    } catch (err) {
      console.error('Greska pri cuvanju kulture:', err)
      setError('Greska pri cuvanju kulture.')
    }
  }

  const handleCropDelete = async (parcelId: string) => {
    if (!confirm('Da li zelite da obrisete evidentiranu kulturu?')) return

    try {
      await axios.delete(`http://localhost:5108/api/Parcels/${parcelId}/crop`, { headers })
      setCropForm(emptyCropForm)
      setSuccess('Kultura je obrisana.')
      await fetchParcels()
    } catch (err) {
      console.error('Greska pri brisanju kulture:', err)
      setError('Greska pri brisanju kulture.')
    }
  }

  if (loading) {
    return (
      <div className="min-h-screen app-shell flex items-center justify-center text-white">
        Učitavanje parcela...
      </div>
    )
  }

  return (
    <div className="min-h-screen app-shell p-8">
      <div className="max-w-7xl mx-auto">
        <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between mb-6">
          <div>
            <h1 className="text-2xl font-bold text-white mt-2">Moje parcele</h1>
            <p className="text-slate-400 text-sm">
              Registracija parcela sa koordinatama i prikazom na mapi.
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

        <div className="space-y-6">
          <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
            <div className="bg-slate-900 border border-slate-800 rounded-xl p-6">
            <h2 className="text-white font-bold text-lg mb-4">
              {editingId ? 'Izmena parcele' : 'Nova parcela'}
            </h2>

            <div className="space-y-4">
              <div>
                <label className="block text-slate-400 text-sm mb-2">Naziv</label>
                <input
                  name="name"
                  value={form.name}
                  onChange={handleChange}
                  className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                  placeholder="Upišite naziv parcele"
                />
              </div>

              <div>
                <label className="block text-slate-400 text-sm mb-2">Površina</label>
                <input
                  name="area"
                  type="number"
                  min="0"
                  step="0.01"
                  value={form.area}
                  onChange={handleChange}
                  className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                  placeholder="Upišite površinu parcele"
                />
              </div>

              <div>
                <label className="block text-slate-400 text-sm mb-2">Lokacija</label>
                <input
                  name="location"
                  value={form.location}
                  onChange={handleChange}
                  className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                  placeholder="Upišite lokaciju parcele"
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-slate-400 text-sm mb-2">Latitude</label>
                  <input
                    name="latitude"
                    type="number"
                    step="0.000001"
                    value={form.latitude}
                    onChange={handleChange}
                    className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                    placeholder="Upišite geografsku širinu"
                  />
                </div>

                <div>
                  <label className="block text-slate-400 text-sm mb-2">Longitude</label>
                  <input
                    name="longitude"
                    type="number"
                    step="0.000001"
                    value={form.longitude}
                    onChange={handleChange}
                    className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                    placeholder="Upišite geografsku dužinu"
                  />
                </div>
              </div>

              <div>
                <label className="block text-slate-400 text-sm mb-2">Opis</label>
                <textarea
                  name="description"
                  value={form.description}
                  onChange={handleChange}
                  className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500 min-h-24"
                  placeholder="Upišite opis parcele"
                />
              </div>

              <div className="flex gap-3">
                <button
                  onClick={handleSubmit}
                  disabled={saving}
                  className="flex-1 bg-yellow-500 hover:bg-yellow-400 text-slate-950 font-bold py-3 rounded transition-colors disabled:opacity-50"
                >
                  {saving ? 'Čuvanje...' : editingId ? 'Sačuvaj izmene' : 'Dodaj parcelu'}
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

            <div className="bg-slate-900 border border-slate-800 rounded-xl p-6">
            <h2 className="text-white font-bold text-lg mb-4">Posejana kultura</h2>

            <div className="space-y-4">
              <div>
                <label className="block text-slate-400 text-sm mb-2">Parcela</label>
                <select
                  value={cropForm.parcelId}
                  onChange={(event) => {
                    const parcel = parcels.find((item) => item.id === event.target.value)
                    if (parcel) selectCropParcel(parcel)
                    else setCropForm(emptyCropForm)
                  }}
                  className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                >
                  <option value="">Izaberite parcelu</option>
                  {parcels.map((parcel) => (
                    <option key={parcel.id} value={parcel.id}>
                      {parcel.name}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-slate-400 text-sm mb-2">Kultura</label>
                <input
                  value={cropForm.cropName}
                  onChange={(event) => setCropForm({ ...cropForm, cropName: event.target.value })}
                  className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                  placeholder="Unesite naziv kulture"
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-slate-400 text-sm mb-2">Cvetanje od</label>
                  <input
                    type="date"
                    value={cropForm.floweringStart}
                    onChange={(event) => setCropForm({ ...cropForm, floweringStart: event.target.value })}
                    className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                  />
                </div>
                <div>
                  <label className="block text-slate-400 text-sm mb-2">Cvetanje do</label>
                  <input
                    type="date"
                    value={cropForm.floweringEnd}
                    onChange={(event) => setCropForm({ ...cropForm, floweringEnd: event.target.value })}
                    className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                  />
                </div>
              </div>

              <div>
                <label className="block text-slate-400 text-sm mb-2">Napomena</label>
                <textarea
                  value={cropForm.cropNotes}
                  onChange={(event) => setCropForm({ ...cropForm, cropNotes: event.target.value })}
                  className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500 min-h-24"
                  placeholder="Dodatne informacije o kulturi"
                />
              </div>

              <div className="flex gap-3">
                <button
                  onClick={handleCropSubmit}
                  className="flex-1 bg-green-500 hover:bg-green-400 text-slate-950 font-bold py-3 rounded transition-colors"
                >
                  Sacuvaj kulturu
                </button>
                {cropForm.parcelId && (
                  <button
                    onClick={() => handleCropDelete(cropForm.parcelId)}
                    className="flex-1 bg-red-600 hover:bg-red-500 text-white font-bold py-3 rounded transition-colors"
                  >
                    Obrisi
                  </button>
                )}
              </div>
            </div>
          </div>

          </div>

          <div className="space-y-6">
            <div className="bg-slate-900 border border-slate-800 rounded-xl p-4">
              <h2 className="text-white font-bold text-lg mb-4">Mapa parcela</h2>

              <div className="h-96 rounded-xl overflow-hidden">
                <MapContainer
                  center={mapCenter}
                  zoom={parcels.length > 0 ? 12 : 7}
                  scrollWheelZoom
                  className="h-full w-full"
                >
                  <TileLayer
                    attribution="&copy; OpenStreetMap contributors"
                    url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                  />

                  {parcels.map((parcel) => (
                    <Marker
                      key={parcel.id}
                      position={[parcel.latitude, parcel.longitude]}
                      icon={parcelIcon(parcel.cropName)}
                    >
                      <Popup>
                        <strong>{parcel.name}</strong>
                        <br />
                        {parcel.location}
                        <br />
                        Površina: {parcel.area}
                      </Popup>
                    </Marker>
                  ))}
                </MapContainer>
              </div>
            </div>

            <div className="bg-slate-900 border border-slate-800 rounded-xl overflow-hidden">
              <div className="px-6 py-4 border-b border-slate-800">
                <h2 className="text-white font-bold text-lg">Lista parcela</h2>
              </div>

              {parcels.length === 0 ? (
                <div className="p-6 text-slate-400">
                  Još uvek nemate registrovane parcele.
                </div>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead>
                      <tr className="border-b border-slate-800">
                        <th className="text-left text-slate-400 px-6 py-4 font-medium">Naziv</th>
                        <th className="text-left text-slate-400 px-6 py-4 font-medium">Lokacija</th>
                        <th className="text-left text-slate-400 px-6 py-4 font-medium">Površina</th>
                        <th className="text-left text-slate-400 px-6 py-4 font-medium">Kultura</th>
                        <th className="text-left text-slate-400 px-6 py-4 font-medium">Koordinate</th>
                        <th className="text-left text-slate-400 px-6 py-4 font-medium">Akcije</th>
                      </tr>
                    </thead>

                    <tbody>
                      {parcels.map((parcel) => (
                        <tr
                          key={parcel.id}
                          className="border-b border-slate-800 hover:bg-slate-800/50"
                        >
                          <td className="px-6 py-4 text-white font-medium">{parcel.name}</td>
                          <td className="px-6 py-4 text-slate-300">{parcel.location}</td>
                          <td className="px-6 py-4 text-slate-300">{parcel.area}</td>
                          <td className="px-6 py-4 text-slate-300">
                            {parcel.cropName ? (
                              <div>
                                <div className="text-green-300">{parcel.cropName}</div>
                                <div className="text-xs text-slate-500">
                                  {formatDate(parcel.floweringStart)} - {formatDate(parcel.floweringEnd)}
                                </div>
                              </div>
                            ) : (
                              '-'
                            )}
                          </td>
                          <td className="px-6 py-4 text-slate-300">
                            {parcel.latitude}, {parcel.longitude}
                          </td>
                          <td className="px-6 py-4">
                            <div className="flex gap-2">
                              <button
                                onClick={() => handleEdit(parcel)}
                                className="bg-blue-600 hover:bg-blue-500 text-white text-xs px-3 py-1 rounded transition-colors"
                              >
                                Izmeni
                              </button>

                              <button
                                onClick={() => selectCropParcel(parcel)}
                                className="bg-green-600 hover:bg-green-500 text-white text-xs px-3 py-1 rounded transition-colors"
                              >
                                Kultura
                              </button>

                              <button
                                onClick={() => handleDelete(parcel.id)}
                                className="bg-red-600 hover:bg-red-500 text-white text-xs px-3 py-1 rounded transition-colors"
                              >
                                Obriši
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
    </div>
  )
}

function formatDate(value: string | null) {
  if (!value) return '-'
  return new Date(value).toLocaleDateString('sr-RS')
}

export default ParcelsPage
