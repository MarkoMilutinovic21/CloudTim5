import { useEffect, useMemo, useState } from 'react'
import axios from 'axios'
import ReturnToDashboardButton from '../components/ReturnToDashboardButton'

interface Apiary {
  id: string
  name: string
}

interface Hive {
  id: string
  name: string
  hiveType: string
  extensionColor: string
  queenAge: number
  description: string
  apiaryId: string
  createdAt: string
}

interface HiveForm {
  name: string
  hiveType: string
  extensionColor: string
  queenAge: string
  description: string
}

const apiBase = 'http://localhost:5108/api'

const hiveTypes = ['LR', 'DB', 'Poloska']

const emptyHiveForm: HiveForm = {
  name: '',
  hiveType: 'LR',
  extensionColor: '',
  queenAge: '',
  description: '',
}

function HivesPage() {
  const [apiaries, setApiaries] = useState<Apiary[]>([])
  const [selectedApiaryId, setSelectedApiaryId] = useState(localStorage.getItem('hivesApiaryId') ?? '')
  const [hives, setHives] = useState<Hive[]>([])
  const [form, setForm] = useState<HiveForm>(emptyHiveForm)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [deviceHiveId, setDeviceHiveId] = useState('')
  const [serialNumber, setSerialNumber] = useState('')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const token = localStorage.getItem('token')
  const headers = useMemo(() => ({ Authorization: `Bearer ${token}` }), [token])

  const fetchApiaries = async () => {
    const response = await axios.get<Apiary[]>(`${apiBase}/Apiaries`, { headers })
    setApiaries(response.data)

    if (!selectedApiaryId && response.data.length > 0) {
      const firstId = response.data[0].id
      setSelectedApiaryId(firstId)
      localStorage.setItem('hivesApiaryId', firstId)
    }
  }

  const fetchHives = async (apiaryId = selectedApiaryId) => {
    if (!apiaryId) {
      setHives([])
      return
    }

    const response = await axios.get<Hive[]>(`${apiBase}/Hives/${apiaryId}`, { headers })
    setHives(response.data)
    if (!deviceHiveId && response.data.length > 0) setDeviceHiveId(response.data[0].id)
  }

  const loadPage = async () => {
    setLoading(true)
    setError('')

    try {
      await fetchApiaries()
    } catch (err: any) {
      console.error('Hives page load failed:', err)
      setError(err.response ? `Greska pri ucitavanju. Status: ${err.response.status}` : 'Backend nije dostupan.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadPage()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  useEffect(() => {
    if (!selectedApiaryId) return

    fetchHives(selectedApiaryId).catch((err) => {
      console.error('Hives load failed:', err)
      setError('Greska pri ucitavanju kosnica.')
    })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedApiaryId])

  const validateForm = () => {
    if (!selectedApiaryId) return 'Izaberite pcelinjak.'
    if (!form.name.trim()) return 'Oznaka kosnice je obavezna.'
    if (!form.hiveType.trim()) return 'Tip kosnice je obavezan.'
    if (!form.extensionColor.trim()) return 'Boja nastavka je obavezna.'

    const queenAge = Number(form.queenAge)
    if (!Number.isInteger(queenAge) || queenAge < 0 || queenAge > 10) {
      return 'Starost matice mora biti ceo broj od 0 do 10.'
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
      hiveType: form.hiveType,
      extensionColor: form.extensionColor,
      queenAge: Number(form.queenAge),
      description: form.description,
      apiaryId: selectedApiaryId,
    }

    try {
      if (editingId) {
        await axios.put(`${apiBase}/Hives/${selectedApiaryId}/${editingId}`, payload, { headers })
        setSuccess('Kosnica je uspesno izmenjena.')
      } else {
        await axios.post(`${apiBase}/Hives`, payload, { headers })
        setSuccess('Kosnica je uspesno dodata.')
      }

      setForm(emptyHiveForm)
      setEditingId(null)
      await fetchHives()
    } catch (err: any) {
      console.error('Hive save failed:', err)
      setError(err.response ? `Greska pri cuvanju kosnice. Status: ${err.response.status}` : 'Backend nije dostupan.')
    } finally {
      setSaving(false)
    }
  }

  const handleEdit = (hive: Hive) => {
    setEditingId(hive.id)
    setForm({
      name: hive.name,
      hiveType: hive.hiveType,
      extensionColor: hive.extensionColor,
      queenAge: hive.queenAge.toString(),
      description: hive.description ?? '',
    })
    setError('')
    setSuccess('')
  }

  const handleDelete = async (hiveId: string) => {
    if (!confirm('Da li ste sigurni da zelite da obrisete kosnicu?')) return

    try {
      await axios.delete(`${apiBase}/Hives/${selectedApiaryId}/${hiveId}`, { headers })
      setSuccess('Kosnica je obrisana.')
      await fetchHives()
    } catch (err: any) {
      console.error('Hive delete failed:', err)
      setError(err.response ? `Greska pri brisanju kosnice. Status: ${err.response.status}` : 'Backend nije dostupan.')
    }
  }

  const handleRegisterDevice = async () => {
    if (!deviceHiveId) {
      setError('Izaberite kosnicu za registraciju uredjaja.')
      setSuccess('')
      return
    }

    if (!/^SA-\d{4}-\d{5}$/.test(serialNumber.trim())) {
      setError('Serijski broj mora biti u formatu SA-YYYY-XXXXX.')
      setSuccess('')
      return
    }

    try {
      await axios.post(`${apiBase}/Hives/${selectedApiaryId}/${deviceHiveId}/devices`, {
        serialNumber: serialNumber.trim(),
      }, { headers })

      setSerialNumber('')
      setSuccess('Uredjaj je uspesno registrovan.')
      setError('')
    } catch (err: any) {
      console.error('Device register failed:', err)
      setError(err.response?.data?.message || err.response?.data?.error || (err.response ? `Greska pri registraciji uredjaja. Status: ${err.response.status}` : 'Backend nije dostupan.'))
      setSuccess('')
    }
  }

  const handleApiaryChange = (apiaryId: string) => {
    setSelectedApiaryId(apiaryId)
    localStorage.setItem('hivesApiaryId', apiaryId)
    setEditingId(null)
    setForm(emptyHiveForm)
    setDeviceHiveId('')
  }

  if (loading) {
    return <div className="min-h-screen app-shell flex items-center justify-center text-white">Ucitavanje kosnica...</div>
  }

  return (
    <div className="min-h-screen app-shell p-8">
      <div className="max-w-7xl mx-auto">
        <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between mb-6">
          <div>
            <h1 className="text-2xl font-bold text-white mt-2">Kosnice</h1>
            <p className="text-slate-400 text-sm">Upravljanje kosnicama i registracija pametnih vaga.</p>
          </div>
          <ReturnToDashboardButton />
        </div>

        {error && <div className="bg-red-900/50 border border-red-700 text-red-300 px-4 py-3 rounded mb-4 text-sm">{error}</div>}
        {success && <div className="bg-green-900/50 border border-green-700 text-green-300 px-4 py-3 rounded mb-4 text-sm">{success}</div>}

        <div className="bg-slate-900 border border-slate-800 rounded-xl p-5 mb-6">
          <label className="block text-slate-400 text-sm mb-2">Pcelinjak</label>
          <select
            value={selectedApiaryId}
            onChange={(event) => handleApiaryChange(event.target.value)}
            className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
          >
            <option value="">Izaberite pcelinjak</option>
            {apiaries.map((apiary) => (
              <option key={apiary.id} value={apiary.id}>{apiary.name}</option>
            ))}
          </select>
        </div>

        <div className="grid grid-cols-1 xl:grid-cols-[420px_1fr] gap-6">
          <div className="space-y-6">
            <section className="bg-slate-900 border border-slate-800 rounded-xl p-6">
              <h2 className="text-white font-bold text-lg mb-4">{editingId ? 'Izmena kosnice' : 'Nova kosnica'}</h2>
              <div className="space-y-4">
                <Field label="Oznaka kosnice" value={form.name} onChange={(value) => setForm({ ...form, name: value })} placeholder="Unesite oznaku kosnice" />

                <div>
                  <label className="block text-slate-400 text-sm mb-2">Tip kosnice</label>
                  <select
                    value={form.hiveType}
                    onChange={(event) => setForm({ ...form, hiveType: event.target.value })}
                    className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                  >
                    {hiveTypes.map((type) => <option key={type} value={type}>{type}</option>)}
                  </select>
                </div>

                <Field label="Boja nastavka" value={form.extensionColor} onChange={(value) => setForm({ ...form, extensionColor: value })} placeholder="Unesite boju nastavka" />
                <Field label="Starost matice" type="number" value={form.queenAge} onChange={(value) => setForm({ ...form, queenAge: value })} placeholder="Unesite starost matice u godinama" />

                <div>
                  <label className="block text-slate-400 text-sm mb-2">Napomena</label>
                  <textarea
                    value={form.description}
                    onChange={(event) => setForm({ ...form, description: event.target.value })}
                    className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500 min-h-24"
                    placeholder="Unesite napomenu o kosnici"
                  />
                </div>

                <div className="flex gap-3">
                  <button onClick={handleSubmit} disabled={saving || !selectedApiaryId} className="flex-1 bg-yellow-500 hover:bg-yellow-400 text-slate-950 font-bold py-3 rounded transition-colors disabled:opacity-50">
                    {saving ? 'Cuvanje...' : editingId ? 'Sacuvaj izmene' : 'Dodaj kosnicu'}
                  </button>
                  {editingId && (
                    <button onClick={() => { setEditingId(null); setForm(emptyHiveForm) }} className="flex-1 bg-slate-700 hover:bg-slate-600 text-white font-bold py-3 rounded transition-colors">
                      Odustani
                    </button>
                  )}
                </div>
              </div>
            </section>

            <section className="bg-slate-900 border border-slate-800 rounded-xl p-6">
              <h2 className="text-white font-bold text-lg mb-4">Registracija uredjaja</h2>
              <div className="space-y-4">
                <div>
                  <label className="block text-slate-400 text-sm mb-2">Kosnica</label>
                  <select
                    value={deviceHiveId}
                    onChange={(event) => setDeviceHiveId(event.target.value)}
                    className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                  >
                    <option value="">Izaberite kosnicu</option>
                    {hives.map((hive) => <option key={hive.id} value={hive.id}>{hive.name}</option>)}
                  </select>
                </div>

                <Field label="Serijski broj" value={serialNumber} onChange={setSerialNumber} placeholder="Unesite serijski broj uredjaja" />

                <button onClick={handleRegisterDevice} className="w-full bg-green-500 hover:bg-green-400 text-slate-950 font-bold py-3 rounded transition-colors">
                  Registruj uredjaj
                </button>

              </div>
            </section>
          </div>

          <section className="bg-slate-900 border border-slate-800 rounded-xl overflow-hidden">
            <div className="px-6 py-4 border-b border-slate-800">
              <h2 className="text-white font-bold text-lg">Kosnice u pcelinjaku</h2>
            </div>

            {hives.length === 0 ? (
              <div className="p-6 text-slate-400">Nema unetih kosnica za izabrani pcelinjak.</div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="border-b border-slate-800">
                      <th className="text-left text-slate-400 px-6 py-4 font-medium">Oznaka</th>
                      <th className="text-left text-slate-400 px-6 py-4 font-medium">Tip</th>
                      <th className="text-left text-slate-400 px-6 py-4 font-medium">Boja</th>
                      <th className="text-left text-slate-400 px-6 py-4 font-medium">Matica</th>
                      <th className="text-left text-slate-400 px-6 py-4 font-medium">Napomena</th>
                      <th className="text-left text-slate-400 px-6 py-4 font-medium">Akcije</th>
                    </tr>
                  </thead>
                  <tbody>
                    {hives.map((hive) => (
                      <tr key={hive.id} className="border-b border-slate-800 hover:bg-slate-800/50">
                        <td className="px-6 py-4 text-white font-medium">{hive.name}</td>
                        <td className="px-6 py-4 text-slate-300">{hive.hiveType}</td>
                        <td className="px-6 py-4 text-slate-300">{hive.extensionColor}</td>
                        <td className="px-6 py-4 text-slate-300">{hive.queenAge} god.</td>
                        <td className="px-6 py-4 text-slate-300">{hive.description || '-'}</td>
                        <td className="px-6 py-4">
                          <div className="flex gap-2">
                            <button onClick={() => handleEdit(hive)} className="bg-blue-600 hover:bg-blue-500 text-white text-xs px-3 py-1 rounded transition-colors">Izmeni</button>
                            <button onClick={() => handleDelete(hive.id)} className="bg-red-600 hover:bg-red-500 text-white text-xs px-3 py-1 rounded transition-colors">Obrisi</button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </section>
        </div>
      </div>
    </div>
  )
}

function Field({
  label,
  value,
  onChange,
  placeholder,
  type = 'text',
}: {
  label: string
  value: string
  onChange: (value: string) => void
  placeholder?: string
  type?: string
}) {
  return (
    <div>
      <label className="block text-slate-400 text-sm mb-2">{label}</label>
      <input
        type={type}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
        placeholder={placeholder}
      />
    </div>
  )
}

export default HivesPage
