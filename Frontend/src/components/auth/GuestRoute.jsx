import { Navigate, Outlet } from 'react-router-dom'
import { useSelector } from 'react-redux'
import { selectIsAuthenticated } from '../../features/auth/authSlice.js'

/** Redirects authenticated users away from guest-only pages (e.g. login). */
export function GuestRoute() {
  const isAuthenticated = useSelector(selectIsAuthenticated)
  if (isAuthenticated) {
    return <Navigate to="/" replace />
  }
  return <Outlet />
}
