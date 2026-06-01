import { useEffect, useMemo, useState } from 'react'
import axios from 'axios'

interface Parcel {
  id: string
  name: string
  area: number
  location: string
}

interface SprayingRecord {
  id: string
  startTime: string
  durationHours: number
  chemicalName: string
  parcelId: string
}

interface RecordForm {
  parcelId: string
  startTime: string
  durationHours: string
  chemicalName: string
}

const emptyForm: RecordForm = {
  parcelId: '',
  startTime: '',
  durationHours: '',
  chemicalName: '',
}

function SprayingRecordsPage() {
  const [parcels, setParcels] = useState<Parcel[]>([])
  const [records, setRecords] = useState<SprayingRecord[]>([])
  const [form, setForm] = useState<RecordForm>(emptyForm)

  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)

  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const token = localStorage.getItem('token')

  const headers = useMemo(
    () => ({
      Authorization: `Bearer ${token}`,
    }),
    [token]
  )

  const [selectedParcelId, setSelectedParcelId] = useState('')
  const [startTime, setStartTime] = useState('')
const [durationHours, setDurationHours] = useState('')
const [chemicalName, setChemicalName] = useState('')


  const fetchParcels = async () => {
  try {
    const response = await axios.get(
      'http://localhost:5108/api/Parcels',
      { headers }
    )

    setParcels(response.data)
  } catch (err) {
    console.error('Greška pri učitavanju parcela:', err)
    setError('Greška pri učitavanju parcela.')
  } finally {
    setLoading(false)
  }
}

const fetchRecords = async (parcelId: string) => {
  try {
    const response = await axios.get(
      `http://localhost:5108/api/farmer/spraying-records/${parcelId}`,
      { headers }
    )

    setRecords(response.data)
  } catch (err) {
    console.error('Greška pri učitavanju zapisa:', err)
  }
}

useEffect(() => {
  fetchParcels()
}, [])

  if (loading) {
  return (
    <div className="min-h-screen bg-slate-950 flex items-center justify-center text-white">
      Učitavanje...
    </div>
  )
}


const createRecord = async () => {
  try {
    console.log({
  startTime,
  durationHours,
  chemicalName,
  parcelId: selectedParcelId
})
    await axios.post(
      'http://localhost:5108/api/farmer/spraying-records',
      {
        startTime: new Date(startTime).toISOString(),
        durationHours: Number(durationHours),
        chemicalName,
        parcelId: selectedParcelId
      },
      { headers }
    )

    alert('Uspešno sačuvano')

    if (selectedParcelId) {
      fetchRecords(selectedParcelId)
}

    setChemicalName('')
    setDurationHours('')
    setStartTime('')
  }
  catch (err) {
    console.error(err)
    alert('Greška')
  }
}
return (
  <div className="min-h-screen bg-slate-950 p-8">
    <div className="max-w-7xl mx-auto">
      <a href="/" className="text-slate-400 hover:text-yellow-400 text-sm">
        ← Nazad na dashboard
      </a>

      <h1 className="text-3xl font-bold text-white mt-2">
        Digitalni karton prskanja
      </h1>

      <div className="mt-6">
  <label className="block text-white mb-2">
    Izaberi parcelu
  </label>

  <select
    value={selectedParcelId}
    onChange={(e) => {
      const id = e.target.value
        setSelectedParcelId(id)

      if (id) {
        fetchRecords(id)
  }
}}
    className="w-full bg-slate-900 border border-slate-700 rounded-lg p-3 text-white"
  >
    <option value="">-- Izaberi parcelu --</option>

    {parcels.map((parcel: any) => (
      <option key={parcel.id} value={parcel.id}>
        {parcel.name}
      </option>
    ))}
  </select>
  <div className="mt-6 space-y-4">

  <div>
    <label className="block text-white mb-2">
      Datum i vreme prskanja
    </label>

    <input
      type="datetime-local"
      value={startTime}
      onChange={(e) => setStartTime(e.target.value)}
      className="w-full bg-slate-900 border border-slate-700 rounded-lg p-3 text-white"
    />
  </div>

  <div>
    <label className="block text-white mb-2">
      Trajanje (sati)
    </label>

    <input
      type="number"
      value={durationHours}
      onChange={(e) => setDurationHours(e.target.value)}
      className="w-full bg-slate-900 border border-slate-700 rounded-lg p-3 text-white"
    />
  </div>

  <div>
    <label className="block text-white mb-2">
      Preparat
    </label>

    <input
      type="text"
      value={chemicalName}
      onChange={(e) => setChemicalName(e.target.value)}
      className="w-full bg-slate-900 border border-slate-700 rounded-lg p-3 text-white"
    />
  </div>
<button
  onClick={createRecord}
  className="mt-6 bg-green-600 hover:bg-green-700 px-6 py-3 rounded-lg text-white font-semibold"
>
  Sačuvaj zapis
</button>
<div className="mt-8">
  <h2 className="text-xl font-bold text-white mb-4">
    Istorija prskanja
  </h2>

  <table className="w-full border border-slate-700 text-white">
    <thead>
      <tr className="bg-slate-800">
        <th className="p-2 border">Datum</th>
        <th className="p-2 border">Trajanje</th>
        <th className="p-2 border">Preparat</th>
      </tr>
    </thead>

    <tbody>
      {records.map((record, index) => (
        <tr key={index}>
          <td className="p-2 border">
            {new Date(record.startTime).toLocaleString()}
          </td>

          <td className="p-2 border">
            {record.durationHours} h
          </td>

          <td className="p-2 border">
            {record.chemicalName}
          </td>
        </tr>
      ))}
    </tbody>
  </table>
</div>
</div>
</div>
    </div>
  </div>
)
}

export default SprayingRecordsPage