import { useEffect, useMemo, useState } from 'react'
import axios from 'axios'
import { MapContainer, Marker, Popup, TileLayer } from 'react-leaflet'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'
import ReturnToDashboardButton from '../components/ReturnToDashboardButton'

interface Apiary {
  id: string
  name: string
  description: string
  location: string
  latitude: number
  longitude: number
  imageUrl: string
  thumbnailUrl: string
  ownerId: string
  createdAt: string
}

interface ApiaryForm {
  name: string
  latitude: string
  longitude: string
  description: string
  image: File | null
}

const apiBase = 'http://localhost:5108/api'

const emptyForm: ApiaryForm = {
  name: '',
  latitude: '',
  longitude: '',
  description: '',
  image: null,
}

const markerIcon = new L.Icon({
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
})

function ApiariesPage() {
  const [apiaries, setApiaries] = useState<Apiary[]>([])
  const [form, setForm] = useState<ApiaryForm>(emptyForm)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [imagePreviewUrl, setImagePreviewUrl] = useState('')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const token = localStorage.getItem('token')
  const headers = useMemo(() => ({ Authorization: `Bearer ${token}` }), [token])

  const mapCenter = useMemo<[number, number]>(() => {
    if (apiaries.length > 0) return [apiaries[0].latitude, apiaries[0].longitude]
    return [44.7866, 20.4489]
  }, [apiaries])

  const fetchApiaries = async () => {
    setLoading(true)
    setError('')

    try {
      const response = await axios.get<Apiary[]>(`${apiBase}/Apiaries`, { headers })
      setApiaries(response.data)
    } catch (err: unknown) {
      const response = axios.isAxiosError(err) ? err.response : undefined
      console.error('Apiary load failed:', err)
      setError(response ? `Greska pri ucitavanju pcelinjaka. Status: ${response.status}` : 'Backend nije dostupan.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchApiaries()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  useEffect(() => {
    if (!form.image) {
      setImagePreviewUrl('')
      return
    }

    const previewUrl = URL.createObjectURL(form.image)
    setImagePreviewUrl(previewUrl)

    return () => URL.revokeObjectURL(previewUrl)
  }, [form.image])

  const validateForm = () => {
    if (!form.name.trim()) return 'Naziv pcelinjaka je obavezan.'

    const latitude = Number(form.latitude)
    const longitude = Number(form.longitude)

    if (Number.isNaN(latitude) || latitude < -90 || latitude > 90) {
      return 'Latitude mora biti izmedju -90 i 90.'
    }

    if (Number.isNaN(longitude) || longitude < -180 || longitude > 180) {
      return 'Longitude mora biti izmedju -180 i 180.'
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

    const latitude = Number(form.latitude)
    const longitude = Number(form.longitude)
    const data = new FormData()
    data.append('name', form.name)
    data.append('description', form.description)
    data.append('location', `${latitude}, ${longitude}`)
    data.append('latitude', latitude.toString())
    data.append('longitude', longitude.toString())
    if (form.image) data.append('image', form.image)

    try {
      if (editingId) {
        await axios.put(`${apiBase}/Apiaries/${editingId}`, data, { headers })
        setSuccess('Pcelinjak je uspesno izmenjen.')
      } else {
        await axios.post(`${apiBase}/Apiaries`, data, { headers })
        setSuccess('Pcelinjak je uspesno dodat.')
      }

      setForm(emptyForm)
      setImagePreviewUrl('')
      setEditingId(null)
      await fetchApiaries()
    } catch (err: unknown) {
      const response = axios.isAxiosError(err) ? err.response : undefined
      console.error('Apiary save failed:', err)
      setError(response ? `Greska pri cuvanju pcelinjaka. Status: ${response.status}` : 'Backend nije dostupan.')
    } finally {
      setSaving(false)
    }
  }

  const handleEdit = (apiary: Apiary) => {
    setEditingId(apiary.id)
    setForm({
      name: apiary.name,
      latitude: apiary.latitude.toString(),
      longitude: apiary.longitude.toString(),
      description: apiary.description ?? '',
      image: null,
    })
    setImagePreviewUrl(apiary.thumbnailUrl || apiary.imageUrl || '')
    setError('')
    setSuccess('')
  }

  const handleCancelEdit = () => {
    setEditingId(null)
    setForm(emptyForm)
    setImagePreviewUrl('')
    setError('')
    setSuccess('')
  }

  const handleDelete = async (id: string) => {
    if (!confirm('Da li ste sigurni da zelite da obrisete pcelinjak?')) return

    try {
      await axios.delete(`${apiBase}/Apiaries/${id}`, { headers })
      setSuccess('Pcelinjak je obrisan.')
      await fetchApiaries()
    } catch (err: unknown) {
      const response = axios.isAxiosError(err) ? err.response : undefined
      console.error('Apiary delete failed:', err)
      setError(response ? `Greska pri brisanju pcelinjaka. Status: ${response.status}` : 'Backend nije dostupan.')
    }
  }

  if (loading) {
    return <div className="min-h-screen app-shell flex items-center justify-center text-white">Ucitavanje pcelinjaka...</div>
  }

  return (
    <div className="min-h-screen app-shell p-8">
      <div className="max-w-7xl mx-auto">
        <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between mb-6">
          <div>
            <h1 className="text-2xl font-bold text-white mt-2">Moji pcelinjaci</h1>
            <p className="text-slate-400 text-sm">Unos lokacije pcelinjaka i prikaz na mapi.</p>
          </div>
          <ReturnToDashboardButton />
        </div>

        {error && <div className="bg-red-900/50 border border-red-700 text-red-300 px-4 py-3 rounded mb-4 text-sm">{error}</div>}
        {success && <div className="bg-green-900/50 border border-green-700 text-green-300 px-4 py-3 rounded mb-4 text-sm">{success}</div>}

        <div className="grid grid-cols-1 xl:grid-cols-[420px_1fr] gap-6">
          <section className="bg-slate-900 border border-slate-800 rounded-xl p-6">
            <h2 className="text-white font-bold text-lg mb-4">{editingId ? 'Izmena pcelinjaka' : 'Novi pcelinjak'}</h2>
            <div className="space-y-4">
              <div>
                <label className="block text-slate-400 text-sm mb-2">Naziv</label>
                <input
                  value={form.name}
                  onChange={(event) => setForm({ ...form, name: event.target.value })}
                  className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                  placeholder="Unesite naziv pcelinjaka"
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-slate-400 text-sm mb-2">Latitude</label>
                  <input
                    type="number"
                    step="0.000001"
                    value={form.latitude}
                    onChange={(event) => setForm({ ...form, latitude: event.target.value })}
                    className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                  />
                </div>
                <div>
                  <label className="block text-slate-400 text-sm mb-2">Longitude</label>
                  <input
                    type="number"
                    step="0.000001"
                    value={form.longitude}
                    onChange={(event) => setForm({ ...form, longitude: event.target.value })}
                    className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                  />
                </div>
              </div>

              <div>
                <label className="block text-slate-400 text-sm mb-2">Opis terena</label>
                <textarea
                  value={form.description}
                  onChange={(event) => setForm({ ...form, description: event.target.value })}
                  className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500 min-h-28"
                  placeholder="Unesite opis terena"
                />
              </div>

              <div>
                <label className="block text-slate-400 text-sm mb-2">Slika pcelinjaka</label>
                <input
                  type="file"
                  accept="image/*"
                  onChange={(event) => setForm({ ...form, image: event.target.files?.[0] ?? null })}
                  className="w-full bg-slate-800 text-slate-300 border border-slate-700 rounded px-4 py-3"
                />
                {imagePreviewUrl && (
                  <div className="mt-3 overflow-hidden rounded border border-slate-700 bg-slate-950">
                    <img
                      src={imagePreviewUrl}
                      alt="Preview izabrane slike pcelinjaka"
                      className="h-44 w-full object-cover"
                    />
                    <div className="flex items-center justify-between px-3 py-2 text-xs text-slate-400">
                      <span>{form.image ? form.image.name : 'Postojeca slika'}</span>
                      {form.image && (
                        <button
                          type="button"
                          onClick={() => setForm({ ...form, image: null })}
                          className="text-red-300 hover:text-red-200"
                        >
                          Ukloni izbor
                        </button>
                      )}
                    </div>
                  </div>
                )}
                {editingId && <p className="text-slate-500 text-xs mt-2">Ako ne izaberete novu sliku, postojeca ostaje sacuvana.</p>}
              </div>

              <div className="flex gap-3">
                <button
                  onClick={handleSubmit}
                  disabled={saving}
                  className="flex-1 bg-yellow-500 hover:bg-yellow-400 text-slate-950 font-bold py-3 rounded transition-colors disabled:opacity-50"
                >
                  {saving ? 'Cuvanje...' : editingId ? 'Sacuvaj izmene' : 'Dodaj pcelinjak'}
                </button>
                {editingId && (
                  <button onClick={handleCancelEdit} className="flex-1 bg-slate-700 hover:bg-slate-600 text-white font-bold py-3 rounded transition-colors">
                    Odustani
                  </button>
                )}
              </div>
            </div>
          </section>

          <section className="space-y-6">
            <div className="bg-slate-900 border border-slate-800 rounded-xl p-4">
              <h2 className="text-white font-bold text-lg mb-4">Mapa pcelinjaka</h2>
              <div className="h-96 rounded-xl overflow-hidden">
                <MapContainer center={mapCenter} zoom={apiaries.length > 0 ? 12 : 7} scrollWheelZoom className="h-full w-full">
                  <TileLayer attribution="&copy; OpenStreetMap contributors" url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
                  {apiaries.map((apiary) => (
                    <Marker key={apiary.id} position={[apiary.latitude, apiary.longitude]} icon={markerIcon}>
                      <Popup>
                        <strong>{apiary.name}</strong>
                        <br />
                        {apiary.description || apiary.location}
                        {apiary.thumbnailUrl && (
                          <>
                            <br />
                            <img src={apiary.thumbnailUrl} alt={apiary.name} className="mt-2 max-w-40 rounded" />
                          </>
                        )}
                      </Popup>
                    </Marker>
                  ))}
                </MapContainer>
              </div>
            </div>

            <div className="bg-slate-900 border border-slate-800 rounded-xl overflow-hidden">
              <div className="px-6 py-4 border-b border-slate-800">
                <h2 className="text-white font-bold text-lg">Lista pcelinjaka</h2>
              </div>
              {apiaries.length === 0 ? (
                <div className="p-6 text-slate-400">Jos uvek nemate registrovane pcelinjake.</div>
              ) : (
                <div className="divide-y divide-slate-800">
                  {apiaries.map((apiary) => (
                    <div key={apiary.id} className="p-5 flex flex-col md:flex-row gap-4 md:items-center md:justify-between">
                      <div className="flex gap-4">
                        {apiary.thumbnailUrl ? (
                          <img src={apiary.thumbnailUrl} alt={apiary.name} className="h-20 w-28 object-cover rounded border border-slate-700" />
                        ) : (
                          <div className="h-20 w-28 rounded bg-slate-800 border border-slate-700" />
                        )}
                        <div>
                          <h3 className="text-white font-bold">{apiary.name}</h3>
                          <p className="text-slate-400 text-sm">{apiary.description || '-'}</p>
                          <p className="text-slate-500 text-xs mt-1">{apiary.latitude}, {apiary.longitude}</p>
                        </div>
                      </div>
                      <div className="flex gap-2">
                        <button onClick={() => handleEdit(apiary)} className="bg-blue-600 hover:bg-blue-500 text-white text-xs px-3 py-2 rounded transition-colors">Izmeni</button>
                        <button onClick={() => handleDelete(apiary.id)} className="bg-red-600 hover:bg-red-500 text-white text-xs px-3 py-2 rounded transition-colors">Obrisi</button>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </section>
        </div>
      </div>
    </div>
  )
}

export default ApiariesPage
