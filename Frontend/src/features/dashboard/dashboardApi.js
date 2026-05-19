import { apiClient } from "../../services/apiClient.js";

function mapDashboardData(raw) {
  const source = raw?.data ?? raw?.Data ?? raw ?? {};

  return {
    total:
      source.total ??
      source.Total ??
      source.totalRequests ??
      source.TotalRequests ??
      0,
    open: source.open ?? source.Open ?? 0,
    pendingApproval: source.pendingApproval ?? source.PendingApproval ?? 0,
    assigned: source.assigned ?? source.Assigned ?? 0,
    inProgress: source.inProgress ?? source.InProgress ?? 0,
    resolved: source.resolved ?? source.Resolved ?? 0,
    closed: source.closed ?? source.Closed ?? 0,
    role:
      source.role ?? source.Role ?? source.roleName ?? source.RoleName ?? "",
  };
}

export function parseDashboardError(err) {
  const data = err?.response?.data;
  if (typeof data === "string") return data;
  if (data?.message) return String(data.message);
  if (data?.Message) return String(data.Message);
  if (data?.title) return String(data.title);
  if (err?.message) return String(err.message);
  return "Failed to load dashboard";
}

export async function fetchDashboardApi() {
  const { data } = await apiClient.get("/Dashboard");
  return mapDashboardData(data);
}
