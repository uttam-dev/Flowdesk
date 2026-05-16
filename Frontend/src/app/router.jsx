import { Navigate, Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from '../components/auth/ProtectedRoute.jsx'
import { GuestRoute } from '../components/auth/GuestRoute.jsx'
import { MainLayout } from '../components/layout/MainLayout.jsx'
import { LoginPage } from '../features/auth/pages/LoginPage.jsx'
import { UnauthorizedPage } from '../features/auth/pages/UnauthorizedPage.jsx'
import { CategoryPage } from '../features/categories/pages/CategoryPage.jsx'
import { UserPage } from '../features/users/pages/UserPage.jsx'
import { HomePage } from './pages/HomePage.jsx'
import { AdminSamplePage } from './pages/AdminSamplePage.jsx'

export function AppRouter() {
  return (
    <Routes>
      <Route element={<GuestRoute />}>
        <Route path="/login" element={<LoginPage />} />
      </Route>

      <Route path="/unauthorized" element={<UnauthorizedPage />} />

      <Route element={<ProtectedRoute />}>
        <Route element={<MainLayout />}>
          <Route index element={<HomePage />} />
          <Route element={<ProtectedRoute roles={['Admin']} />}>
            <Route path="admin-sample" element={<AdminSamplePage />} />
            <Route path="categories" element={<CategoryPage />} />
            <Route path="users" element={<UserPage />} />
          </Route>
        </Route>
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
