import { useEffect, useState } from "react";
import { RoleBasedDashboard } from "../../features/dashboard/components/RoleBasedDashboard.jsx";
import {
  fetchDashboardApi,
  parseDashboardError,
} from "../../features/dashboard/dashboardApi.js";

export function HomePage() {
  const [dashboardData, setDashboardData] = useState(null);
  const [loading, setLoading] = useState("idle");
  const [error, setError] = useState(null);

  useEffect(() => {
    let cancelled = false;

    Promise.resolve()
      .then(() => {
        if (cancelled) return null;
        setLoading("pending");
        setError(null);
        return fetchDashboardApi();
      })
      .then((data) => {
        if (!cancelled && data) {
          setDashboardData(data);
          setLoading("succeeded");
        }
      })
      .catch((err) => {
        if (!cancelled) {
          setDashboardData(null);
          setError(parseDashboardError(err));
          setLoading("failed");
        }
      });

    return () => {
      cancelled = true;
    };
  }, []);

  return (
    <div className="space-y-6">
      {error ? (
        <p className="rounded-xl border border-red-100 bg-red-50 p-4 text-sm font-medium text-red-800 shadow-sm">
          {error}
        </p>
      ) : (
        <RoleBasedDashboard
          dashboardData={loading === "pending" ? null : dashboardData}
        />
      )}
    </div>
  );
}
