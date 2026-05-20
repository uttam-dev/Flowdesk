import { useSelector } from "react-redux";
import {
  Activity,
  Archive,
  CheckCircle2,
  ClipboardList,
  Clock,
  Circle,
  UserCheck,
} from "lucide-react";
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  Cell,
  LabelList,
  ResponsiveContainer,
  PieChart,
  Pie,
} from "recharts";

const CARD_ORDER = [
  "total",
  "open",
  "pendingApproval",
  "assigned",
  "inProgress",
  "resolved",
  "closed",
];

const ROLE_CARDS = {
  Employee: ["total", "open", "pendingApproval", "resolved", "closed"],
  Manager: [
    "total",
    "open",
    "pendingApproval",
    "inProgress",
    "resolved",
    "closed",
  ],
  Support: ["total", "assigned", "inProgress", "resolved", "closed"],
  Admin: CARD_ORDER,
};

const CARD_META = {
  total: {
    label: "Total",
    Icon: ClipboardList,
    accent: "bg-gray-100 text-gray-700 ring-gray-200",
    bar: "bg-gray-200",
  },
  open: {
    label: "Open",
    Icon: Circle,
    accent: "bg-gray-100 text-gray-700 ring-gray-200",
    bar: "bg-gray-300",
  },
  pendingApproval: {
    label: "Pending approval",
    Icon: Clock,
    accent: "bg-yellow-50 text-yellow-800 ring-yellow-100",
    bar: "bg-yellow-100",
  },
  assigned: {
    label: "Assigned",
    Icon: UserCheck,
    accent: "bg-blue-50 text-blue-800 ring-blue-100",
    bar: "bg-blue-100",
  },
  inProgress: {
    label: "In progress",
    Icon: Activity,
    accent: "bg-purple-50 text-purple-800 ring-purple-100",
    bar: "bg-purple-100",
  },
  resolved: {
    label: "Resolved",
    Icon: CheckCircle2,
    accent: "bg-emerald-50 text-emerald-800 ring-emerald-100",
    bar: "bg-emerald-100",
  },
  closed: {
    label: "Closed",
    Icon: Archive,
    accent: "bg-gray-200 text-gray-800 ring-gray-300",
    bar: "bg-gray-300",
  },
};

const STATUS_COLORS = {
  Open: "#378ADD",
  PendingApproval: "#EF9F27",
  Assigned: "#7F77DD",
  InProgress: "#D85A30",
  Resolved: "#1D9E75",
  Closed: "#639922",
};

function DashboardSkeleton() {
  return (
    <div className="space-y-6">
      <section className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
        <div className="h-6 w-48 rounded bg-gray-100" />
        <div className="mt-3 h-4 w-64 max-w-full rounded bg-gray-100" />
      </section>

      <div className="grid grid-cols-2 gap-4 md:grid-cols-3 xl:grid-cols-4">
        {[0, 1, 2, 3, 4, 5].map((item) => (
          <div
            key={item}
            className="rounded-xl border border-gray-200 bg-white p-4 shadow-sm"
          >
            <div className="flex items-center justify-between gap-3">
              <div className="h-10 w-10 rounded-lg bg-gray-100" />
              <div className="h-2 w-12 rounded bg-gray-100" />
            </div>
            <div className="mt-5 h-4 w-24 rounded bg-gray-100" />
            <div className="mt-3 h-7 w-16 rounded bg-gray-100" />
          </div>
        ))}
      </div>
    </div>
  );
}

function StatCard({ itemKey, count }) {
  const meta = CARD_META[itemKey];
  const Icon = meta.Icon;

  return (
    <article className="overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm transition-shadow duration-200 hover:shadow-md">
      <div className={`h-1 ${meta.bar}`} />
      <div className="p-4">
        <div className="flex items-start justify-between gap-3">
          <span
            className={`inline-flex h-10 w-10 shrink-0 items-center justify-center rounded-lg ring-1 ring-inset ${meta.accent}`}
          >
            <Icon className="h-5 w-5" aria-hidden />
          </span>
          <span className="rounded-full bg-gray-100 px-2 py-0.5 text-xs font-medium text-gray-600 ring-1 ring-inset ring-gray-200">
            Requests
          </span>
        </div>
        <p className="mt-5 text-sm font-medium text-gray-500">{meta.label}</p>
        <p className="mt-1 text-3xl font-semibold tracking-tight text-gray-900">
          {Number(count ?? 0).toLocaleString()}
        </p>
      </div>
    </article>
  );
}

// ─── Section A ────────────────────────────────────────────────────────────────

function SectionA_Admin({ data }) {
  const count = (data.open ?? 0) + (data.assigned ?? 0);
  const isHigh = count > 3;
  return (
    <div
      className={`overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm border-l-4 ${isHigh ? "border-l-amber-400" : "border-l-emerald-400"}`}
    >
      <div className="p-4">
        <p className="text-sm font-medium text-gray-500">Needs attention</p>
        <p
          className={`mt-1 text-3xl font-semibold tracking-tight ${isHigh ? "text-amber-600" : "text-emerald-600"}`}
        >
          {count.toLocaleString()}
        </p>
        <p className="mt-1 text-xs text-gray-500">open and assigned requests</p>
      </div>
    </div>
  );
}

function SectionA_Manager({ data }) {
  const count = data.pendingApproval ?? 0;
  const allClear = count === 0;
  return (
    <div
      className={`overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm border-l-4 ${allClear ? "border-l-emerald-400" : "border-l-amber-400"}`}
    >
      <div className="p-4">
        <p className="text-sm font-medium text-gray-500">
          Pending your approval
        </p>
        {allClear ? (
          <p className="mt-1 text-3xl font-semibold tracking-tight text-emerald-600">
            All caught up
          </p>
        ) : (
          <p className="mt-1 text-3xl font-semibold tracking-tight text-amber-600">
            {count.toLocaleString()}
          </p>
        )}
        <p className="mt-1 text-xs text-gray-500">
          requests awaiting your review
        </p>
      </div>
    </div>
  );
}

function SectionA_Employee({ data }) {
  return (
    <div className="overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm border-l-4 border-l-blue-400">
      <div className="p-4">
        <p className="text-sm font-medium text-gray-500">My active requests</p>
        <div className="mt-3 flex gap-8">
          <div>
            <p className="text-3xl font-semibold tracking-tight text-gray-900">
              {(data.open ?? 0).toLocaleString()}
            </p>
            <p className="mt-0.5 text-xs text-gray-500">Open</p>
          </div>
          <div>
            <p className="text-3xl font-semibold tracking-tight text-gray-900">
              {(data.pendingApproval ?? 0).toLocaleString()}
            </p>
            <p className="mt-0.5 text-xs text-gray-500">Pending approval</p>
          </div>
        </div>
      </div>
    </div>
  );
}

function SectionA_Support({ data }) {
  return (
    <div className="overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm border-l-4 border-l-purple-400">
      <div className="p-4">
        <p className="text-sm font-medium text-gray-500">My workload</p>
        <div className="mt-3 flex gap-8">
          <div>
            <p className="text-3xl font-semibold tracking-tight text-gray-900">
              {(data.assigned ?? 0).toLocaleString()}
            </p>
            <p className="mt-0.5 text-xs text-gray-500">Assigned to me</p>
          </div>
          <div>
            <p className="text-3xl font-semibold tracking-tight text-gray-900">
              {(data.inProgress ?? 0).toLocaleString()}
            </p>
            <p className="mt-0.5 text-xs text-gray-500">In progress</p>
          </div>
        </div>
      </div>
    </div>
  );
}

// ─── Section B ────────────────────────────────────────────────────────────────

function SectionB({ data, chartData, completionRate }) {
  return (
    <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
      {/* Left — horizontal bar chart */}
      <div className="overflow-hidden rounded-xl border border-gray-200 bg-white p-4 shadow-sm">
        <p className="mb-3 text-sm font-medium text-gray-700">
          Request breakdown
        </p>
        {chartData.length === 0 ? (
          <p className="text-sm text-gray-400">No data</p>
        ) : (
          <ResponsiveContainer width="100%" height={220}>
            <BarChart
              data={chartData}
              layout="vertical"
              margin={{ top: 0, right: 40, left: 8, bottom: 0 }}
            >
              <XAxis type="number" axisLine={false} tick={false} />
              <YAxis
                type="category"
                dataKey="name"
                width={110}
                tick={{ fontSize: 12, fill: "#6b7280" }}
                axisLine={false}
                tickLine={false}
              />
              <Tooltip
                cursor={{ fill: "#f3f4f6" }}
                contentStyle={{
                  fontSize: 12,
                  borderRadius: 8,
                  border: "1px solid #e5e7eb",
                }}
              />
              <Bar dataKey="value" radius={[0, 4, 4, 0]}>
                {chartData.map((entry) => (
                  <Cell
                    key={entry.name}
                    fill={STATUS_COLORS[entry.name] ?? "#94a3b8"}
                  />
                ))}
                <LabelList
                  dataKey="value"
                  position="right"
                  style={{ fontSize: 12, fill: "#374151" }}
                />
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        )}
      </div>

      {/* Right — donut + legend + completion bar */}
      <div className="overflow-hidden rounded-xl border border-gray-200 bg-white p-4 shadow-sm">
        <p className="mb-3 text-sm font-medium text-gray-700">Distribution</p>
        {chartData.length === 0 ? (
          <p className="text-sm text-gray-400">No data</p>
        ) : (
          <>
            {/* Donut with center label */}
            <div
              className="relative mx-auto"
              style={{ width: "100%", height: 160 }}
            >
              <ResponsiveContainer width="100%" height={160}>
                <PieChart>
                  <Pie
                    data={chartData}
                    dataKey="value"
                    innerRadius="55%"
                    outerRadius="80%"
                    paddingAngle={2}
                    startAngle={90}
                    endAngle={-270}
                  >
                    {chartData.map((entry) => (
                      <Cell
                        key={entry.name}
                        fill={STATUS_COLORS[entry.name] ?? "#94a3b8"}
                      />
                    ))}
                  </Pie>
                  <Tooltip
                    contentStyle={{
                      fontSize: 12,
                      borderRadius: 8,
                      border: "1px solid #e5e7eb",
                    }}
                  />
                </PieChart>
              </ResponsiveContainer>
              {/* Center overlay label */}
              <div className="pointer-events-none absolute inset-0 flex flex-col items-center justify-center">
                <span className="text-2xl font-medium text-gray-900">
                  {(data.total ?? 0).toLocaleString()}
                </span>
                <span className="text-xs text-gray-500">total</span>
              </div>
            </div>

            {/* Custom legend */}
            <div className="mt-3 space-y-1.5">
              {chartData.map((entry) => (
                <div
                  key={entry.name}
                  className="flex items-center justify-between gap-2"
                >
                  <div className="flex items-center gap-2 min-w-0">
                    <span
                      className="h-2 w-2 shrink-0 rounded-full"
                      style={{
                        backgroundColor: STATUS_COLORS[entry.name] ?? "#94a3b8",
                      }}
                    />
                    <span className="truncate text-xs text-gray-600">
                      {entry.name}
                    </span>
                  </div>
                  <span className="text-xs font-medium text-gray-900">
                    {entry.value}
                  </span>
                </div>
              ))}
            </div>

            {/* Completion bar */}
            <div className="mt-4">
              <div className="flex items-center justify-between text-xs text-gray-600">
                <span>Completion rate</span>
                <span className="font-medium text-gray-900">
                  {completionRate}%
                </span>
              </div>
              <div className="mt-1.5 h-2 w-full overflow-hidden rounded-full bg-gray-100">
                <div
                  className="h-2 rounded-full transition-all duration-500"
                  style={{
                    width: `${completionRate}%`,
                    backgroundColor: "#639922",
                  }}
                />
              </div>
              <p className="mt-1 text-xs text-gray-500">
                {data.closed ?? 0} of {data.total ?? 0} closed
              </p>
            </div>
          </>
        )}
      </div>
    </div>
  );
}

// ─── Section C ────────────────────────────────────────────────────────────────

function SectionC({ chartData }) {
  return (
    <div className="overflow-hidden rounded-xl border border-gray-200 bg-white p-4 shadow-sm">
      <p className="mb-3 text-sm font-medium text-gray-700">Status summary</p>
      <div className="flex flex-wrap gap-2">
        {chartData.map((entry) => (
          <span
            key={entry.name}
            className="px-3 py-1 rounded-full text-sm font-medium"
            style={{
              backgroundColor: (STATUS_COLORS[entry.name] ?? "#94a3b8") + "26",
              color: STATUS_COLORS[entry.name] ?? "#94a3b8",
            }}
          >
            {entry.name} · {entry.value}
          </span>
        ))}
      </div>
    </div>
  );
}

// ─── Main export ──────────────────────────────────────────────────────────────

export function RoleBasedDashboard({ dashboardData }) {
  const { name, email } = useSelector((state) => state.auth.user) ?? {};

  if (!dashboardData) {
    return <DashboardSkeleton />;
  }

  const role = dashboardData.role;
  const visibleCards = ROLE_CARDS[role] ?? ROLE_CARDS.Admin;
  const displayName = name || email || "there";

  // Shared helpers for new sections
  const chartData = [
    { name: "Open", value: dashboardData.open ?? 0 },
    { name: "PendingApproval", value: dashboardData.pendingApproval ?? 0 },
    { name: "Assigned", value: dashboardData.assigned ?? 0 },
    { name: "InProgress", value: dashboardData.inProgress ?? 0 },
    { name: "Resolved", value: dashboardData.resolved ?? 0 },
    { name: "Closed", value: dashboardData.closed ?? 0 },
  ].filter((item) => item.value > 0);

  const completionRate =
    (dashboardData.total ?? 0) > 0
      ? Math.round(((dashboardData.closed ?? 0) / dashboardData.total) * 100)
      : 0;

  return (
    <div className="space-y-6">
      {/* ── Existing: welcome card ── */}
      <section className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
        <p className="text-xs font-semibold uppercase tracking-wide text-emerald-700">
          {role || "Dashboard"}
        </p>
        <div className="mt-1 flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
          <div className="min-w-0">
            <h2 className="text-xl font-semibold tracking-tight text-gray-900">
              Welcome, {displayName}
            </h2>
            <p className="mt-1 truncate text-sm text-gray-600">
              {email || "No email available"}
            </p>
          </div>
          <span className="inline-flex w-fit rounded-full bg-emerald-50 px-3 py-1.5 text-xs font-medium text-emerald-900 ring-1 ring-inset ring-emerald-100">
            {visibleCards.length} stats
          </span>
        </div>
      </section>

      {/* ── Existing: stat cards ── */}
      <section
        className="grid grid-cols-2 gap-4 md:grid-cols-3 xl:grid-cols-4"
        aria-label="Dashboard statistics"
      >
        {visibleCards.map((itemKey) => (
          <StatCard
            key={itemKey}
            itemKey={itemKey}
            count={dashboardData[itemKey]}
          />
        ))}
      </section>

      {/* ── NEW sections (guarded) ── */}
      {dashboardData ? (
        <>
          {/* Section A — role callout */}
          {role === "Admin" && <SectionA_Admin data={dashboardData} />}
          {role === "Manager" && <SectionA_Manager data={dashboardData} />}
          {role === "Employee" && <SectionA_Employee data={dashboardData} />}
          {role === "Support" && <SectionA_Support data={dashboardData} />}

          {/* Section B — charts */}
          <SectionB
            data={dashboardData}
            chartData={chartData}
            completionRate={completionRate}
          />

          {/* Section C — badge summary */}
          {chartData.length > 0 && <SectionC chartData={chartData} />}
        </>
      ) : null}
    </div>
  );
}

export default RoleBasedDashboard;
