import { Link } from 'react-router-dom'

function ReturnToDashboardButton() {
  return (
    <Link
      to="/"
      className="inline-flex items-center gap-2 rounded-lg border border-slate-700 bg-slate-900 px-4 py-2 text-sm font-semibold text-slate-200 transition-colors hover:border-yellow-500 hover:text-yellow-300"
    >
      &larr; Nazad na dashboard
    </Link>
  )
}

export default ReturnToDashboardButton
