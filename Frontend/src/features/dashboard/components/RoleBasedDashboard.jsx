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

export function RoleBasedDashboard({ dashboardData }) {
  const { name, email } = useSelector((state) => state.auth.user) ?? {};

  if (!dashboardData) {
    return <DashboardSkeleton />;
  }

  const role = dashboardData.role;
  const visibleCards = ROLE_CARDS[role] ?? ROLE_CARDS.Admin;
  const displayName = name || email || "there";

  return (
    <div className="space-y-6">
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
    </div>
  );
}

export default RoleBasedDashboard;
