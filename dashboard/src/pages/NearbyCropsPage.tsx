import { useEffect, useMemo, useState } from 'react'
import axios from 'axios'
import ReturnToDashboardButton from '../components/ReturnToDashboardButton'
import { MapContainer, Marker, Popup, TileLayer } from 'react-leaflet'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'

interface NearbyCrop {
  parcelId: string
  parcelName: string
  area: number
  location: string
  latitude: number
  longitude: number
  cropName: string
  floweringStart: string | null
  floweringEnd: string | null
  cropNotes: string
  farmerName: string
  farmerPhone: string
  apiaryId: string
  apiaryName: string
  distanceKm: number
}

const cropIcon = new L.Icon({
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
})

function NearbyCropsPage() {
  const [crops, setCrops] = useState<NearbyCrop[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const token = localStorage.getItem('token')
  const headers = useMemo(() => ({ Authorization: `Bearer ${token}` }), [token])

  const mapCenter = useMemo<[number, number]>(() => {
    if (crops.length > 0) return [crops[0].latitude, crops[0].longitude]
    return [44.7866, 20.4489]
  }, [crops])

  const fetchCrops = async () => {
    setLoading(true)
    setError('')

    try {
      const response = await axios.get<NearbyCrop[]>('http://localhost:5108/api/Crops/nearby', {
        headers,
      })
      setCrops(response.data)
    } catch (err: any) {
      console.error('Nearby crops load failed:', err)
      if (err.response?.status === 401 || err.response?.status === 403) {
        setError('Nemate pristup pregledu kultura. Prijavite se kao pcelar.')
      } else {
        setError('Greska pri ucitavanju posejanih kultura.')
      }
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchCrops()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  return (
    <div className="min-h-screen app-shell p-8">
      <div className="max-w-7xl mx-auto">
        <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between mb-6">
          <div>
            <h1 className="text-2xl font-bold text-white mt-2">Posejane kulture</h1>
            <p className="text-slate-400 text-sm">Pregled svih kultura koje su poljoprivrednici evidentirali.</p>
          </div>

          <div className="flex flex-col items-start gap-3 sm:flex-row sm:items-center">
            <ReturnToDashboardButton />
            <button
              onClick={fetchCrops}
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

        <div className="grid grid-cols-1 lg:grid-cols-[2fr_1fr] gap-6">
          <section className="bg-slate-900 border border-slate-800 rounded-xl p-4">
            <div className="h-[560px] rounded-xl overflow-hidden">
              <MapContainer
                center={mapCenter}
                zoom={crops.length > 0 ? 11 : 7}
                scrollWheelZoom
                className="h-full w-full"
              >
                <TileLayer
                  attribution="&copy; OpenStreetMap contributors"
                  url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                />
                {crops.map((crop) => (
                  <Marker
                    key={`${crop.parcelId}-${crop.apiaryId}`}
                    position={[crop.latitude, crop.longitude]}
                    icon={cropIcon}
                  >
                    <Popup>
                      <strong>{crop.cropName}</strong>
                      <br />
                      {crop.parcelName}
                      <br />
                      {crop.location}
                      <br />
                      Udaljenost: {crop.distanceKm} km
                    </Popup>
                  </Marker>
                ))}
              </MapContainer>
            </div>
          </section>

          <section className="bg-slate-900 border border-slate-800 rounded-xl overflow-hidden">
            <div className="px-5 py-4 border-b border-slate-800">
              <h2 className="text-white font-bold">Evidentirane kulture</h2>
              <p className="text-slate-500 text-sm">{crops.length} rezultata</p>
            </div>

            <div className="max-h-[560px] overflow-y-auto">
              {crops.length === 0 ? (
                <div className="p-5 text-slate-400">Nema evidentiranih kultura.</div>
              ) : (
                crops.map((crop) => (
                  <div key={`${crop.parcelId}-${crop.apiaryId}`} className="p-5 border-b border-slate-800">
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <h3 className="text-white font-bold">{crop.cropName}</h3>
                        <p className="text-slate-400 text-sm">{crop.parcelName}</p>
                      </div>
                      {crop.distanceKm > 0 && <span className="text-green-300 text-sm">{crop.distanceKm} km</span>}
                    </div>
                    <p className="text-slate-300 text-sm mt-3">{crop.location}</p>
                    <p className="text-slate-400 text-sm mt-2">
                      Cvetanje: {formatDate(crop.floweringStart)} - {formatDate(crop.floweringEnd)}
                    </p>
                    <p className="text-slate-400 text-sm mt-2">
                      Vlasnik: {crop.farmerName || '-'} {crop.farmerPhone ? `(${crop.farmerPhone})` : ''}
                    </p>
                    {crop.cropNotes && <p className="text-slate-500 text-sm mt-2">{crop.cropNotes}</p>}
                  </div>
                ))
              )}
            </div>
          </section>
        </div>
      </div>
    </div>
  )
}

function formatDate(value: string | null) {
  if (!value) return '-'
  return new Date(value).toLocaleDateString('sr-RS')
}

export default NearbyCropsPage
