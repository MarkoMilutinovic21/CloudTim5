import { useEffect, useMemo, useState } from 'react'
import axios from 'axios'
import ReturnToDashboardButton from '../components/ReturnToDashboardButton'

interface JournalEntry {
  id: string
  hiveId: string
  entryDate: string
  title: string
  content: string
  bottomBoardColor: string
  honeyFrames: number
  honeyKg: number
  broodFrames: number
  queenPresent: boolean
  createdAt: string
}

interface HiveOption {
  hiveId: string
  label: string
  source: string
}

const emptyEntry = {
  entryDate: new Date().toISOString().slice(0, 16),
  title: '',
  content: '',
  bottomBoardColor: '',
  honeyFrames: '0',
  honeyKg: '0',
  broodFrames: '0',
  queenPresent: true,
}

function HiveJournalPage() {
  const [hiveId, setHiveId] = useState(localStorage.getItem('journalHiveId') ?? '')
  const [entries, setEntries] = useState<JournalEntry[]>([])
  const [hiveOptions, setHiveOptions] = useState<HiveOption[]>([])
  const [loadingOptions, setLoadingOptions] = useState(false)
  const [form, setForm] = useState(emptyEntry)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [formErrors, setFormErrors] = useState<{ title?: string; content?: string }>({})
  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [page, setPage] = useState(1)
  const pageSize = 5

  const token = localStorage.getItem('token')
  const headers = useMemo(() => ({ Authorization: `Bearer ${token}` }), [token])

  const fetchEntries = async (selectedHiveId = hiveId) => {
    const trimmedHiveId = selectedHiveId.trim()
    if (!trimmedHiveId) {
      setError('HiveId je obavezan.')
      return
    }

    setLoading(true)
    setError('')
    localStorage.setItem('journalHiveId', trimmedHiveId)

    try {
      const response = await axios.get<JournalEntry[]>(
        `http://localhost:5108/api/HiveJournal/${trimmedHiveId}`,
        { headers },
      )
      setEntries(response.data)
      setHiveId(trimmedHiveId)
      setPage(1)
    } catch (err: unknown) {
      const response = axios.isAxiosError(err) ? err.response : undefined
      console.error('Journal load failed:', err)
      if (response?.status === 401 || response?.status === 403) {
        setError('Nemate pristup dnevniku. Prijavite se kao pcelar.')
      } else if (response?.status === 404) {
        setEntries([])
        setError('Kosnica nije pronadjena.')
      } else {
        setError('Greska pri ucitavanju dnevnika.')
      }
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    const loadHiveOptions = async () => {
      setLoadingOptions(true)
      try {
        const response = await axios.get<HiveOption[]>('http://localhost:5108/api/HiveJournal/options', {
          headers,
        })
        setHiveOptions(response.data)

        const savedHiveId = localStorage.getItem('journalHiveId') ?? localStorage.getItem('telemetryHiveId') ?? ''
        const initialHiveId = savedHiveId || response.data[0]?.hiveId || ''
        if (initialHiveId) {
          setHiveId(initialHiveId)
          await fetchEntries(initialHiveId)
        }
      } catch (err) {
        console.error('Journal hive options load failed:', err)
        if (hiveId.trim()) await fetchEntries(hiveId)
      } finally {
        setLoadingOptions(false)
      }
    }

    loadHiveOptions()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const handleEdit = (entry: JournalEntry) => {
    setEditingId(entry.id)
    setForm({
      entryDate: new Date(entry.entryDate).toISOString().slice(0, 16),
      title: entry.title,
      content: entry.content,
      bottomBoardColor: entry.bottomBoardColor,
      honeyFrames: entry.honeyFrames.toString(),
      honeyKg: entry.honeyKg.toString(),
      broodFrames: entry.broodFrames.toString(),
      queenPresent: entry.queenPresent,
    })
    setFormErrors({})
    setError('')
    setSuccess('')
  }

  const handleCancelEdit = () => {
    setEditingId(null)
    setForm(emptyEntry)
    setFormErrors({})
    setError('')
    setSuccess('')
  }

  const saveEntry = async () => {
    const trimmedHiveId = hiveId.trim()

    const errors: { title?: string; content?: string } = {}
    if (!form.title.trim()) errors.title = 'Molim vas popunite naslov.'
    if (!form.content.trim()) errors.content = 'Molim vas popunite napomenu.'

    if (Object.keys(errors).length > 0) {
      setFormErrors(errors)
      return
    }

    setFormErrors({})

    if (!trimmedHiveId) {
      setError('HiveId je obavezan.')
      return
    }

    setSaving(true)
    setError('')
    setSuccess('')

    const payload = {
      hiveId: trimmedHiveId,
      entryDate: new Date(form.entryDate).toISOString(),
      title: form.title,
      content: form.content,
      bottomBoardColor: form.bottomBoardColor,
      honeyFrames: Number(form.honeyFrames) || 0,
      honeyKg: Number(form.honeyKg) || 0,
      broodFrames: Number(form.broodFrames) || 0,
      queenPresent: form.queenPresent,
    }

    try {
      if (editingId) {
        await axios.put(
          `http://localhost:5108/api/HiveJournal/${trimmedHiveId}/${editingId}`,
          payload,
          { headers },
        )
        setSuccess('Zapis je izmenjen.')
        setEditingId(null)
      } else {
        await axios.post('http://localhost:5108/api/HiveJournal', payload, { headers })
        setSuccess('Zapis je dodat u dnevnik.')
      }

      setForm(emptyEntry)
      await fetchEntries(trimmedHiveId)
    } catch (err) {
      console.error('Journal save failed:', err)
      setError('Greska pri cuvanju zapisa.')
    } finally {
      setSaving(false)
    }
  }

  const totalPages = Math.max(1, Math.ceil(entries.length / pageSize))
  const visibleEntries = entries.slice((page - 1) * pageSize, page * pageSize)

  const deleteEntry = async (entryId: string) => {
    if (!confirm('Da li ste sigurni da zelite da obrisete zapis?')) return

    try {
      await axios.delete(`http://localhost:5108/api/HiveJournal/${hiveId.trim()}/${entryId}`, {
        headers,
      })
      setSuccess('Zapis je obrisan.')
      await fetchEntries(hiveId.trim())
    } catch (err) {
      console.error('Journal delete failed:', err)
      setError('Greska pri brisanju zapisa.')
    }
  }

  return (
    <div className="min-h-screen app-shell p-8">
      <div className="max-w-6xl mx-auto">
        <div className="mb-6 flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
          <div>
            <h1 className="text-2xl font-bold text-white mt-2">Pcelarski dnevnik</h1>
            <p className="text-slate-400 text-sm">Zapisi fizickih pregleda za izabranu kosnicu.</p>
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

        <div className="bg-slate-900 border border-slate-800 rounded-xl p-5 mb-6">
          <div>
            <label className="block text-slate-400 text-sm mb-2">HiveId</label>
            <div className="grid grid-cols-1 md:grid-cols-[1fr_auto] gap-4 items-center">
              <div>
                {hiveOptions.length > 0 ? (
                  <select
                    value={hiveId}
                    onChange={(event) => {
                      setHiveId(event.target.value)
                      fetchEntries(event.target.value)
                    }}
                    className="w-full h-14 bg-slate-800 text-white border border-slate-700 rounded px-4 focus:outline-none focus:border-yellow-500"
                  >
                    {hiveOptions.map((option) => (
                      <option key={option.hiveId} value={option.hiveId}>
                        {option.label} - {option.source}
                      </option>
                    ))}
                  </select>
                ) : (
                  <input
                    value={hiveId}
                    onChange={(event) => setHiveId(event.target.value)}
                    className="w-full h-14 bg-slate-800 text-white border border-slate-700 rounded px-4 focus:outline-none focus:border-yellow-500"
                    placeholder={loadingOptions ? 'Ucitavanje kosnica...' : 'Unesite ID kosnice'}
                  />
                )}
              </div>
              <button
                onClick={() => fetchEntries()}
                disabled={loading || loadingOptions}
                className="h-14 bg-yellow-500 hover:bg-yellow-400 text-slate-950 font-bold px-6 rounded transition-colors disabled:opacity-50"
              >
                {loading ? 'Ucitavanje...' : 'Ucitaj dnevnik'}
              </button>
            </div>
            {hiveOptions.length > 0 && (
              <p className="text-slate-500 text-xs mt-2">Ako je simulator pokrenut, njegov HiveId se prikazuje kao Simulator.</p>
            )}
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-[1fr_1.4fr] gap-6">
          <section className="bg-slate-900 border border-slate-800 rounded-xl p-5">
            <h2 className="text-white font-bold text-lg mb-4">
              {editingId ? 'Izmena zapisa' : 'Novi zapis'}
            </h2>
            <div className="space-y-4">
              <div>
                <label className="block text-slate-400 text-sm mb-2">Datum i vreme pregleda</label>
                <input
                  type="datetime-local"
                  value={form.entryDate}
                  onChange={(event) => setForm({ ...form, entryDate: event.target.value })}
                  className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                />
              </div>
              <div>
                <label className="block text-slate-400 text-sm mb-2">Naslov</label>
                <input
                  value={form.title}
                  onChange={(event) => {
                    setForm({ ...form, title: event.target.value })
                    if (formErrors.title) setFormErrors({ ...formErrors, title: undefined })
                  }}
                  className={`w-full bg-slate-800 text-white border rounded px-4 py-3 focus:outline-none focus:border-yellow-500 ${
                    formErrors.title ? 'border-red-500' : 'border-slate-700'
                  }`}
                  placeholder="Unesite napomenu o pregledu"
                />
                {formErrors.title && (
                  <p className="text-red-400 text-xs mt-1">{formErrors.title}</p>
                )}
              </div>
              <div>
                <label className="block text-slate-400 text-sm mb-2">Boja podnjace</label>
                <input
                  value={form.bottomBoardColor}
                  onChange={(event) => setForm({ ...form, bottomBoardColor: event.target.value })}
                  className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                  placeholder="Unesite boju podnjace"
                />
              </div>
              <div className="grid grid-cols-3 gap-3">
                <div>
                  <label className="block text-slate-400 text-sm mb-2">Ramovi med</label>
                  <input
                    type="number"
                    min="0"
                    value={form.honeyFrames}
                    onChange={(event) => setForm({ ...form, honeyFrames: event.target.value })}
                    className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                  />
                </div>
                <div>
                  <label className="block text-slate-400 text-sm mb-2">Med kg</label>
                  <input
                    type="number"
                    min="0"
                    step="0.1"
                    value={form.honeyKg}
                    onChange={(event) => setForm({ ...form, honeyKg: event.target.value })}
                    className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                  />
                </div>
                <div>
                  <label className="block text-slate-400 text-sm mb-2">Ramovi leglo</label>
                  <input
                    type="number"
                    min="0"
                    value={form.broodFrames}
                    onChange={(event) => setForm({ ...form, broodFrames: event.target.value })}
                    className="w-full bg-slate-800 text-white border border-slate-700 rounded px-4 py-3 focus:outline-none focus:border-yellow-500"
                  />
                </div>
              </div>
              <label className="flex items-center gap-3 text-slate-300 text-sm">
                <input
                  type="checkbox"
                  checked={form.queenPresent}
                  onChange={(event) => setForm({ ...form, queenPresent: event.target.checked })}
                  className="h-4 w-4"
                />
                Matica je prisutna
              </label>
              <div>
                <label className="block text-slate-400 text-sm mb-2">Napomena</label>
                <textarea
                  value={form.content}
                  onChange={(event) => {
                    setForm({ ...form, content: event.target.value })
                    if (formErrors.content) setFormErrors({ ...formErrors, content: undefined })
                  }}
                  className={`w-full bg-slate-800 text-white border rounded px-4 py-3 min-h-40 focus:outline-none focus:border-yellow-500 ${
                    formErrors.content ? 'border-red-500' : 'border-slate-700'
                  }`}
                  placeholder="Unesite tekst za pretragu dnevnika"
                />
                {formErrors.content && (
                  <p className="text-red-400 text-xs mt-1">{formErrors.content}</p>
                )}
              </div>
              <div className="flex gap-3">
                <button
                  onClick={saveEntry}
                  disabled={saving}
                  className="flex-1 bg-yellow-500 hover:bg-yellow-400 text-slate-950 font-bold py-3 rounded transition-colors disabled:opacity-50"
                >
                  {saving ? 'Cuvanje...' : editingId ? 'Sacuvaj izmene' : 'Dodaj zapis'}
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
          </section>

          <section className="bg-slate-900 border border-slate-800 rounded-xl overflow-hidden">
            <div className="px-5 py-4 border-b border-slate-800">
              <h2 className="text-white font-bold">Zapisi</h2>
              <p className="text-slate-500 text-sm">{entries.length} zapisa</p>
            </div>

            {entries.length === 0 ? (
              <div className="p-5 text-slate-400">Nema zapisa za izabranu kosnicu.</div>
            ) : (
              <div className="divide-y divide-green-700">
                {visibleEntries.map((entry) => (
                  <article key={entry.id} className="p-5">
                    <div className="flex items-start justify-between gap-4">
                      <div>
                        <h3 className="text-white font-bold">{entry.title}</h3>
                        <p className="text-slate-500 text-sm">{new Date(entry.entryDate).toLocaleString('sr-RS')}</p>
                      </div>
                      <div className="flex gap-2">
                        <button
                          onClick={() => handleEdit(entry)}
                          className="bg-blue-600 hover:bg-blue-500 text-white text-xs px-3 py-1 rounded transition-colors"
                        >
                          Izmeni
                        </button>
                        <button
                          onClick={() => deleteEntry(entry.id)}
                          className="bg-red-600 hover:bg-red-500 text-white text-xs px-3 py-1 rounded transition-colors"
                        >
                          Obrisi
                        </button>
                      </div>
                    </div>
                    <div className="grid grid-cols-2 md:grid-cols-5 gap-3 mt-4 text-sm">
                      <Info label="Podnjaca" value={entry.bottomBoardColor || '-'} />
                      <Info label="Ramovi med" value={entry.honeyFrames.toString()} />
                      <Info label="Med kg" value={entry.honeyKg.toString()} />
                      <Info label="Ramovi leglo" value={entry.broodFrames.toString()} />
                      <Info label="Matica" value={entry.queenPresent ? 'Da' : 'Ne'} />
                    </div>
                    <p className="text-slate-300 text-sm mt-3 whitespace-pre-wrap">{entry.content}</p>
                  </article>
                ))}
              </div>
            )}

            {entries.length > pageSize && (
              <div className="flex items-center justify-between px-5 py-4 border-t border-slate-800">
                <button
                  onClick={() => setPage((value) => Math.max(1, value - 1))}
                  disabled={page === 1}
                  className="bg-slate-700 hover:bg-slate-600 disabled:opacity-40 text-white px-4 py-2 rounded"
                >
                  Prethodna
                </button>
                <span className="text-slate-400 text-sm">
                  Strana {page} / {totalPages}
                </span>
                <button
                  onClick={() => setPage((value) => Math.min(totalPages, value + 1))}
                  disabled={page === totalPages}
                  className="bg-slate-700 hover:bg-slate-600 disabled:opacity-40 text-white px-4 py-2 rounded"
                >
                  Sledeca
                </button>
              </div>
            )}
          </section>
        </div>
      </div>
    </div>
  )
}

function Info({ label, value }: { label: string; value: string }) {
  return (
    <div className="bg-slate-950 rounded p-3">
      <div className="text-slate-500 text-xs">{label}</div>
      <div className="text-slate-200 font-semibold mt-1">{value}</div>
    </div>
  )
}

export default HiveJournalPage
