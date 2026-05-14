import { useDispatch, useSelector } from 'react-redux'
import { Link } from 'react-router-dom'
import { AppHeader } from '../../components/layout/AppHeader.jsx'
import { Button } from '../../components/ui/Button.jsx'
import {
  logout,
  selectAuthUser,
  selectRoleNames,
} from '../../features/auth/authSlice.js'

export function HomePage() {
  const dispatch = useDispatch()
  const user = useSelector(selectAuthUser)
  const roles = useSelector(selectRoleNames)

  const subtitle = (
    <>
      Signed in as{' '}
      <span className="font-medium text-gray-900">{user?.email ?? '—'}</span>
    </>
  )

  return (
    <div className="min-h-screen bg-gray-50 font-sans">
      <AppHeader title="Dashboard" subtitle={subtitle}>
        <Link to="/admin-sample" className="w-full sm:w-auto">
          <Button type="button" variant="secondary" className="w-full sm:w-auto">
            Admin sample
          </Button>
        </Link>
        <Button
          type="button"
          variant="ghost"
          className="w-full sm:w-auto"
          onClick={() => dispatch(logout())}
        >
          Sign out
        </Button>
      </AppHeader>

      <main className="mx-auto max-w-7xl space-y-6 p-4 md:p-6">
        <section className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm transition-shadow duration-200 hover:shadow-md md:p-6">
          <h2 className="text-xl font-semibold tracking-tight text-gray-900">
            RBAC snapshot
          </h2>
          <p className="mt-2 text-sm text-gray-600">
            Roles from your login response (used by{' '}
            <code className="rounded-md border border-gray-200 bg-gray-50 px-1.5 py-0.5 text-xs font-medium text-gray-800">
              ProtectedRoute
            </code>
            ).
          </p>
          <div className="mt-6">
            {roles.length ? (
              <ul className="flex flex-wrap gap-2">
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
              <p className="text-sm text-gray-500">
                No roles were returned — add roles to the login user payload or
                map them in <span className="font-medium">authMappers.js</span>.
              </p>
            )}
          </div>
        </section>
      </main>
    </div>
  )
}
