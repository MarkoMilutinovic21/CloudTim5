import { useEffect, useMemo, useState } from 'react'
import axios from 'axios'
import ReturnToDashboardButton from '../components/ReturnToDashboardButton'

interface Parcel {
  id: string
  name: string
  area: number
  location: string
}

interface Crop {
  id: string
  name: string
  sowingDate: string
  parcelId: string
}

function CropsPage() {
  const [parcels, setParcels] = useState<Parcel[]>([])
  const [crops, setCrops] = useState<Crop[]>([])

  const [selectedParcelId, setSelectedParcelId] = useState('')

  const [name, setName] = useState('')
  const [sowingDate, setSowingDate] = useState('')

  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const token = localStorage.getItem('token')

  const headers = useMemo(
    () => ({
      Authorization: `Bearer ${token}`,
    }),
    [token]
  )

  const fetchParcels = async () => {
    try {
      const response = await axios.get(
        'http://localhost:5108/api/Parcels',
        { headers }
      )

      setParcels(response.data)
    } catch (err) {
      console.error(err)
      setError('Greška pri učitavanju parcela.')
    } finally {
      setLoading(false)
    }
  }

  const fetchCrops = async (parcelId: string) => {
    try {
      const response = await axios.get(
        `http://localhost:5108/api/farmer/crops/${parcelId}`,
        { headers }
      )

      setCrops(response.data)
    } catch (err) {
      console.error(err)
    }
  }

  const createCrop = async () => {
    try {
      await axios.post(
        'http://localhost:5108/api/farmer/crops',
        {
          name,
          sowingDate: new Date(sowingDate).toISOString(),
          parcelId: selectedParcelId,
        },
        { headers }
      )

      alert('Kultura uspešno sačuvana.')

      setName('')
      setSowingDate('')

      if (selectedParcelId) {
        fetchCrops(selectedParcelId)
      }
    } catch (err) {
      console.error(err)
      alert('Greška pri čuvanju kulture.')
    }
  }

  useEffect(() => {
    fetchParcels()
    // Initial load only.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  if (loading) {
    return (
      <div className="min-h-screen app-shell flex items-center justify-center text-white">
        Učitavanje...
      </div>
    )
  }

  return (
    <div className="min-h-screen app-shell p-8">
      <div className="max-w-7xl mx-auto">
        <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
          <h1 className="text-3xl font-bold text-white mt-2">
            Evidencija posejanih kultura
          </h1>
          <ReturnToDashboardButton />
        </div>

        {error && (
          <div className="mt-4 bg-red-500/20 border border-red-500 text-red-300 p-3 rounded">
            {error}
          </div>
        )}

        <div className="mt-8 bg-slate-900 rounded-xl p-6 border border-slate-800">
          <h2 className="text-xl font-bold text-white mb-4">
            Nova kultura
          </h2>

          <div>
            <label className="block text-white mb-2">
              Parcela
            </label>

            <select
              value={selectedParcelId}
              onChange={(e) => {
                const id = e.target.value

                setSelectedParcelId(id)

                if (id) {
                  fetchCrops(id)
                }
              }}
              className="w-full bg-slate-950 border border-slate-700 rounded-lg p-3 text-white"
            >
              <option value="">-- Izaberi parcelu --</option>

              {parcels.map((parcel) => (
                <option key={parcel.id} value={parcel.id}>
                  {parcel.name}
                </option>
              ))}
            </select>
          </div>

          <div className="mt-4">
            <label className="block text-white mb-2">
              Naziv kulture
            </label>

            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              className="w-full bg-slate-950 border border-slate-700 rounded-lg p-3 text-white"
            />
          </div>

          <div className="mt-4">
            <label className="block text-white mb-2">
              Datum setve
            </label>

            <input
              type="date"
              value={sowingDate}
              onChange={(e) => setSowingDate(e.target.value)}
              className="w-full bg-slate-950 border border-slate-700 rounded-lg p-3 text-white"
            />
          </div>

          <button
            onClick={createCrop}
            className="mt-6 bg-green-600 hover:bg-green-700 px-6 py-3 rounded-lg text-white font-semibold"
          >
            Sačuvaj kulturu
          </button>
        </div>

        <div className="mt-8 bg-slate-900 rounded-xl p-6 border border-slate-800">
          <h2 className="text-xl font-bold text-white mb-4">
            Evidencija kultura
          </h2>

          <table className="w-full border border-slate-700 text-white">
            <thead>
              <tr className="bg-slate-800">
                <th className="p-2 border">Kultura</th>
                <th className="p-2 border">Datum setve</th>
              </tr>
            </thead>

            <tbody>
              {crops.map((crop) => (
                <tr key={crop.id}>
                  <td className="p-2 border">
                    {crop.name}
                  </td>

                  <td className="p-2 border">
                    {new Date(crop.sowingDate).toLocaleDateString()}
                  </td>
                </tr>
              ))}

              {crops.length === 0 && (
                <tr>
                  <td
                    colSpan={2}
                    className="p-4 text-center text-slate-400"
                  >
                    Nema evidentiranih kultura za izabranu parcelu.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}

export default CropsPage
