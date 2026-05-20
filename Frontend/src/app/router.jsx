import { Navigate, Route, Routes } from "react-router-dom";
import { ProtectedRoute } from "../components/auth/ProtectedRoute.jsx";
import { GuestRoute } from "../components/auth/GuestRoute.jsx";
import { MainLayout } from "../components/layout/MainLayout.jsx";
import { LoginPage } from "../features/auth/pages/LoginPage.jsx";
import { UnauthorizedPage } from "../features/auth/pages/UnauthorizedPage.jsx";
import { CategoryPage } from "../features/categories/pages/CategoryPage.jsx";
import { UserPage } from "../features/users/pages/UserPage.jsx";
import { RequestListPage } from "../features/requests/pages/RequestListPage.jsx";
import { RequestCreatePage } from "../features/requests/pages/RequestCreatePage.jsx";
import { RequestDetailsPage } from "../features/requests/pages/RequestDetailsPage.jsx";
import { HomePage } from "./pages/HomePage.jsx";
import { AdminSamplePage } from "./pages/AdminSamplePage.jsx";

const REQUEST_ROLES = ["Employee", "Manager", "Admin", "Support"];
const REQUEST_CREATE_ROLES = ["Employee", "Manager"];

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

          <Route element={<ProtectedRoute roles={REQUEST_CREATE_ROLES} />}>
            <Route path="requests/new" element={<RequestCreatePage />} />
          </Route>

          <Route element={<ProtectedRoute roles={REQUEST_ROLES} />}>
            <Route path="requests" element={<RequestListPage />} />
            <Route path="requests/:id" element={<RequestDetailsPage />} />
          </Route>

          <Route element={<ProtectedRoute roles={["Admin"]} />}>
            <Route path="admin-sample" element={<AdminSamplePage />} />
            <Route path="categories" element={<CategoryPage />} />
            <Route path="users" element={<UserPage />} />
          </Route>
        </Route>
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
