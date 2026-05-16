import { useDispatch } from 'react-redux'
import { Link, useNavigate } from 'react-router-dom'
import { Button } from '../../../components/ui/Button.jsx'
import { logout } from '../authSlice.js'

export function UnauthorizedPage() {
  const dispatch = useDispatch()
  const navigate = useNavigate()

  function switchAccount() {
    dispatch(logout())
    navigate('/login', { replace: true })
  }

  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-gray-50 px-4 py-12 font-sans">
      <div className="w-full max-w-md rounded-xl border border-gray-200 bg-white p-8 text-center shadow-sm transition-all duration-200 hover:shadow-md sm:p-10">
        <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-full bg-amber-50 ring-1 ring-amber-100">
          <svg
            className="h-7 w-7 text-amber-600"
            fill="none"
            viewBox="0 0 24 24"
            strokeWidth={1.5}
            stroke="currentColor"
            aria-hidden
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z"
            />
          </svg>
        </div>
        <p className="mt-6 text-xs font-semibold uppercase tracking-wider text-amber-600">
          Access denied
        </p>
        <h1 className="mt-2 text-3xl font-semibold tracking-tight text-gray-900">
          You don&apos;t have permission
        </h1>
        <p className="mt-3 text-sm leading-relaxed text-gray-600">
          Your account is signed in, but your role does not allow this page.
          Ask an administrator if you need access.
        </p>
        <div className="mt-8 flex flex-col gap-3 sm:flex-row sm:justify-center">
          <Link to="/" className="w-full sm:w-auto">
            <Button type="button" variant="primary" className="w-full sm:w-auto">
              Go to home
            </Button>
          </Link>
          <Button
            type="button"
            variant="secondary"
            className="w-full sm:w-auto"
            onClick={switchAccount}
          >
            Switch account
          </Button>
        </div>
      </div>
    </div>
  )
}
