function DashboardPage() {
  const name = localStorage.getItem('name')

  const handleLogout = () => {
    localStorage.clear()
    window.location.href = '/login'
  }

  return (
    <div className="min-h-screen bg-slate-950 text-white flex flex-col items-center justify-center">
      <h1 className="text-3xl font-bold mb-4">Dobrodošli, {name}!</h1>
      <button
        onClick={handleLogout}
        className="bg-red-600 hover:bg-red-700 text-white px-6 py-2 rounded"
      >
        Odjavi se
      </button>
    </div>
  )
}

export default DashboardPage