import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useSelector } from 'react-redux'
import {
  selectHasAnyPermission,
  selectHasAnyRole,
  selectIsAuthenticated,
} from '../../features/auth/authSlice.js'

/**
 * RBAC-aware route guard. Wrap routes that require authentication.
 * - `roles`: if provided, user must have at least one role.
 * - `permissions`: if provided, user must have at least one permission.
 */
export function ProtectedRoute({ roles, permissions }) {
  const location = useLocation()
  const isAuthenticated = useSelector(selectIsAuthenticated)
  const hasRole = useSelector((s) => selectHasAnyRole(s, roles))
  const hasPerm = useSelector((s) => selectHasAnyPermission(s, permissions))

  if (!isAuthenticated) {
    return (
      <Navigate to="/login" replace state={{ from: location.pathname }} />
    )
  }

  if (roles?.length && !hasRole) {
    return <Navigate to="/unauthorized" replace />
  }

  if (permissions?.length && !hasPerm) {
    return <Navigate to="/unauthorized" replace />
  }

  return <Outlet />
}
