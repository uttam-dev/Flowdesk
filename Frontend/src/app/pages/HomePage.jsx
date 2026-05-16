import { useSelector } from 'react-redux'
import {
  selectAuthUser,
  selectRoleNames,
} from '../../features/auth/authSlice.js'

export function HomePage() {
  const user = useSelector(selectAuthUser)
  const roles = useSelector(selectRoleNames)

  return (
    <div className="space-y-6">
      <section className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm transition-shadow duration-200 hover:shadow-md md:p-6">
        <h2 className="text-xl font-semibold tracking-tight text-gray-900">
          Dashboard
        </h2>
        <p className="mt-2 text-sm text-gray-600">
          Signed in as{' '}
          <span className="font-medium text-gray-900">{user?.email ?? '—'}</span>
        </p>
        <p className="mt-4 text-sm text-gray-600">
          Use the sidebar to navigate. Categories are available to{' '}
          <span className="font-medium">Admin</span> only.
        </p>
        <div className="mt-6">
          <h3 className="text-sm font-semibold text-gray-900">Your roles</h3>
          {roles.length ? (
            <ul className="mt-2 flex flex-wrap gap-2">
              {roles.map((r) => (
                <li
                  key={r}
                  className="rounded-full bg-emerald-50 px-3 py-1.5 text-xs font-medium text-emerald-900 ring-1 ring-inset ring-emerald-100 transition-colors duration-200 hover:bg-emerald-100"
                >
                  {r}
                </li>
              ))}
            </ul>
          ) : (
            <p className="mt-2 text-sm text-gray-500">
              No roles in session — map them in auth mappers if needed.
            </p>
          )}
        </div>
      </section>
    </div>
  )
}
