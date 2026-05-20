import { Button } from "../../../components/ui/Button.jsx";
import {
  Table,
  TableHead,
  TableBody,
  TableRow,
  Th,
  Td,
} from "../../../components/ui/Table.jsx";

function fmtDate(v) {
  if (!v) return "—";
  try {
    const d = new Date(v);
    if (Number.isNaN(d.getTime())) return String(v);
    return d.toLocaleString(undefined, {
      day: "numeric",
      month: "2-digit",
      year: "numeric",
      hour: "numeric",
      minute: "2-digit",
      hour12: true,
    });
  } catch {
    return String(v);
  }
}

/** First letter of first word + first letter of last word, uppercased */
function getInitials(fullName) {
  if (!fullName) return "?";
  const parts = fullName.trim().split(/\s+/);
  if (parts.length === 1) return parts[0][0].toUpperCase();
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
}

const ROLE_PILL = {
  Employee: "bg-blue-50 text-blue-700 ring-blue-100",
  Manager: "bg-amber-50 text-amber-700 ring-amber-100",
  Support: "bg-teal-50 text-teal-700 ring-teal-100",
};

function RolePill({ role }) {
  const cls = ROLE_PILL[role] ?? "bg-gray-100 text-gray-600 ring-gray-200";
  return (
    <span
      className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ring-1 ring-inset ${cls}`}
    >
      {role || "—"}
    </span>
  );
}

function ManagerPill({ name }) {
  if (!name) return <span className="text-gray-400">—</span>;
  return (
    <span className="inline-flex items-center rounded-full bg-gray-100 px-2 py-0.5 text-xs font-medium text-gray-600 ring-1 ring-inset ring-gray-200">
      {name}
    </span>
  );
}

export function UserTable({
  rows,
  loading,
  currentUserId,
  onEdit,
  onActivate,
  onDeactivate,
  onDelete,
}) {
  if (loading === "pending" && !rows.length) {
    return (
      <div className="rounded-xl border border-gray-200 bg-white p-10 text-center text-sm text-gray-600 shadow-sm">
        Loading users…
      </div>
    );
  }

  return (
    <Table>
      <TableHead>
        <TableRow className="hover:bg-transparent">
          <Th>Full name</Th>
          <Th>Role</Th>
          <Th className="hidden md:table-cell">Manager</Th>
          <Th>Active</Th>
          <Th className="hidden lg:table-cell">Created on</Th>
          <Th className="hidden xl:table-cell">Updated on</Th>
          <Th className="text-right">Actions</Th>
        </TableRow>
      </TableHead>
      <TableBody>
        {rows.length === 0 ? (
          <TableRow>
            <Td colSpan={7} className="py-10 text-center text-gray-500">
              No users found.
            </Td>
          </TableRow>
        ) : (
          rows.map((row) => {
            const isSelf =
              currentUserId != null &&
              String(row.userId) === String(currentUserId);

            return (
              <TableRow key={row.userId}>
                {/* ── Full name + email + avatar ── */}
                <Td className="align-middle">
                  <div className="flex items-center gap-2.5">
                    {/* Avatar circle */}
                    <span
                      aria-hidden
                      className="inline-flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-indigo-100 text-xs font-semibold text-indigo-700 ring-1 ring-inset ring-indigo-200"
                    >
                      {getInitials(row.fullName)}
                    </span>
                    {/* Name + email stack */}
                    <div className="min-w-0">
                      <p className="truncate text-sm font-medium text-gray-900">
                        {row.fullName || "—"}
                      </p>
                      <p className="truncate text-xs text-gray-500">
                        {row.email || "—"}
                      </p>
                    </div>
                  </div>
                </Td>

                {/* ── Role pill ── */}
                <Td className="align-middle">
                  <RolePill role={row.roleName} />
                </Td>

                {/* ── Manager pill ── */}
                <Td className="hidden align-middle md:table-cell">
                  <ManagerPill name={row.managerName} />
                </Td>

                {/* ── Active badge (unchanged) ── */}
                <Td className="align-middle">
                  <span
                    className={
                      row.isActive
                        ? "rounded-full bg-emerald-50 px-2 py-0.5 text-xs font-medium text-emerald-800 ring-1 ring-emerald-100"
                        : "rounded-full bg-gray-100 px-2 py-0.5 text-xs font-medium text-gray-600 ring-1 ring-gray-200"
                    }
                  >
                    {row.isActive ? "Active" : "Inactive"}
                  </span>
                </Td>

                {/* ── Created on (unchanged) ── */}
                <Td className="hidden align-middle text-xs text-gray-600 lg:table-cell">
                  {fmtDate(row.createdOn)}
                </Td>

                {/* ── Updated on (unchanged) ── */}
                <Td className="hidden align-middle text-xs text-gray-600 xl:table-cell">
                  {fmtDate(row.updatedOn)}
                </Td>

                {/* ── Actions — single flex row ── */}
                <Td className="align-middle text-right">
                  <div className="flex flex-nowrap items-center justify-end gap-1.5">
                    <Button
                      type="button"
                      variant="secondary"
                      className="min-h-0 px-2 py-1.5 text-xs"
                      onClick={() => onEdit(row)}
                    >
                      Edit
                    </Button>
                    {row.isActive ? (
                      <Button
                        type="button"
                        variant="ghost"
                        className="min-h-0 px-2 py-1.5 text-xs text-amber-800 hover:bg-amber-50"
                        onClick={() => onDeactivate(row)}
                      >
                        Deactivate
                      </Button>
                    ) : (
                      <Button
                        type="button"
                        variant="ghost"
                        className="min-h-0 px-2 py-1.5 text-xs text-emerald-800 hover:bg-emerald-50"
                        onClick={() => onActivate(row)}
                      >
                        Activate
                      </Button>
                    )}
                    {!isSelf ? (
                      <Button
                        type="button"
                        variant="ghost"
                        className="min-h-0 px-2 py-1.5 text-xs text-red-700 hover:bg-red-50"
                        onClick={() => onDelete(row)}
                      >
                        Delete
                      </Button>
                    ) : null}
                  </div>
                </Td>
              </TableRow>
            );
          })
        )}
      </TableBody>
    </Table>
  );
}
