import { useNavigate } from 'react-router-dom'

function DashboardPage() {
  const name = localStorage.getItem('name')
  const role = localStorage.getItem('role')
  const navigate = useNavigate()

  const handleLogout = () => {
    localStorage.clear()
    window.location.href = '/login'
  }

  return (
    <div className="min-h-screen bg-slate-950">
      <nav className="bg-slate-900 border-b border-slate-800 px-8 py-4 flex justify-between items-center">
        <h1 className="text-xl font-black bg-gradient-to-r from-yellow-400 to-orange-400 bg-clip-text text-transparent">
          Smart Apiary
        </h1>
        <div className="flex items-center gap-4">
          <span className="text-slate-400 text-sm">{name}</span>
          <button
            onClick={handleLogout}
            className="bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded text-sm transition-colors"
          >
            Odjavi se
          </button>
        </div>
      </nav>

      <div className="p-8 max-w-5xl mx-auto">
        <h2 className="text-2xl font-bold text-white mb-8">
          Dobrodošli, {name}!
        </h2>

        {role === 'Admin' && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div
              onClick={() => navigate('/admin/users')}
              className="bg-slate-900 border border-slate-800 hover:border-yellow-500 rounded-xl p-6 cursor-pointer transition-colors"
            >
              <h3 className="text-white font-bold text-lg mb-1">Korisnici</h3>
              <p className="text-slate-400 text-sm">
                Upravljanje korisničkim nalozima
              </p>
            </div>
          </div>
        )}

        {role === 'Farmer' && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div
              onClick={() => navigate('/farmer/parcels')}
              className="bg-slate-900 border border-slate-800 hover:border-green-500 rounded-xl p-6 cursor-pointer transition-colors"
            >
              <h3 className="text-white font-bold text-lg mb-1">Parcele</h3>
              <p className="text-slate-400 text-sm">
                Registracija, izmena, brisanje i prikaz parcela na mapi
              </p>
            </div>

            <div
              onClick={() => navigate('/farmer/pesticide-treatments')}
              className="bg-slate-900 border border-slate-800 hover:border-yellow-500 rounded-xl p-6 cursor-pointer transition-colors"
            >
              <h3 className="text-white font-bold text-lg mb-1">
                Najave tretiranja pesticidima
              </h3>
              <p className="text-slate-400 text-sm">
                Zakazivanje, pomeranje i otkazivanje tretiranja parcela
              </p>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}

export default DashboardPage