import { Fragment, useState } from "react";
import { Button } from "../../../components/ui/Button.jsx";
import {
  TableHead,
  TableBody,
  TableRow,
  Th,
  Td,
} from "../../../components/ui/Table.jsx";
import {
  REQUEST_STATUS,
  fmtDate,
  getRowActions,
  hasRole,
  priorityBadgeClass,
  priorityLabel,
  statusBadgeClass,
  statusLabel,
  truncate,
} from "../requestUtils.js";

function Badge({ className, children }) {
  return (
    <span
      className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ring-1 ring-inset ${className}`}
    >
      {children}
    </span>
  );
}

function idsMatch(a, b) {
  if (a == null || b == null) return false;
  return String(a) === String(b);
}

function getManagerId(row) {
  return row.managerId ?? row.ManagerId;
}

function getCreatedById(row) {
  return row.createdById ?? row.CreatedById;
}

const isEscalateEligible = (request) =>
  request?.isEscalated !== true &&
  Number(request?.status) !== REQUEST_STATUS.Resolved &&
  Number(request?.status) !== REQUEST_STATUS.Closed &&
  Number(request?.status) !== REQUEST_STATUS.Rejected &&
  Number(request?.status) !== REQUEST_STATUS.PendingApproval;

const getSLABadgeStyle = (slaStatus) => {
  switch (slaStatus) {
    case "Within SLA":
      return "bg-emerald-50 text-emerald-800 ring-emerald-100";
    case "Nearing Breach":
      return "bg-orange-50 text-orange-800 ring-orange-100";
    case "Breached":
      return "bg-red-50 text-red-800 ring-red-100";
    default:
      return "bg-gray-100 text-gray-600 ring-gray-200";
  }
};

function getTableRowActions(row, roles, activeTab) {
  const actions = getRowActions(row, roles);
  if (!hasRole(roles, "Manager")) return actions;

  return actions.filter((action) => {
    if (action.key === "approve" || action.key === "reject") {
      return (
        activeTab === "team" &&
        Number(row.status) === REQUEST_STATUS.PendingApproval
      );
    }
    return true;
  });
}

export function RequestTable({
  rows,
  loading,
  roles,
  currentUserId,
  activeTab,
  onAction,
  emptyStateText,
  selectedIds,
  onSelectionToggle,
}) {
  const [expandedId, setExpandedId] = useState(null);
  const isAdmin = hasRole(roles, "Admin");
  const showSelection = isAdmin;

  const allSelected = showSelection && rows.length > 0 && rows.every((r) => selectedIds?.has(r.requestId));
  const someSelected = showSelection && rows.some((r) => selectedIds?.has(r.requestId));

  function toggleSelectAll() {
    if (!onSelectionToggle) return;
    if (allSelected) {
      rows.forEach((r) => onSelectionToggle(r.requestId));
    } else {
      const toSelect = rows.filter((r) => !selectedIds?.has(r.requestId));
      toSelect.forEach((r) => onSelectionToggle(r.requestId));
    }
  }

  if (loading === "pending" && !rows.length) {
    return (
      <div className="rounded-xl border border-gray-200 bg-white p-10 text-center text-sm text-gray-600 shadow-sm">
        Loading requests…
      </div>
    );
  }

  function toggleExpand(row) {
    const id = row.requestId;
    setExpandedId((prev) => (prev === id ? null : id));
  }
  return (
    <div className="w-full min-w-0 overflow-x-auto rounded-xl border border-gray-200 bg-white shadow-sm">
      <table className="min-w-full divide-y divide-gray-200 text-left text-sm">
        <TableHead>
          <TableRow className="hover:bg-transparent">
            {showSelection ? (
              <Th className="w-10">
                <input
                  type="checkbox"
                  className="h-4 w-4 cursor-pointer rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
                  checked={allSelected}
                  ref={(el) => { if (el && someSelected && !allSelected) el.indeterminate = true; }}
                  onChange={toggleSelectAll}
                  onClick={(e) => e.stopPropagation()}
                />
              </Th>
            ) : null}
            <Th>Request</Th>
            <Th>Category</Th>
            <Th>Priority</Th>
            <Th>Status</Th>
            <Th>Due Date</Th>
            <Th>SLA Status</Th>
            <Th className="text-right">Actions</Th>
          </TableRow>
        </TableHead>
        <TableBody>
          {rows.length === 0 ? (
            <TableRow>
              <Td colSpan={showSelection ? 8 : 7} className="py-10 text-center text-gray-500">
                {emptyStateText || "No requests found."}
              </Td>
            </TableRow>
          ) : (
            rows.map((row) => {
              const expanded = expandedId === row.requestId;
              const actions = getTableRowActions(row, roles, activeTab);
              const checked = selectedIds?.has(row.requestId) ?? false;
              return (
                <Fragment key={row.requestId}>
                  <TableRow
                    className={`cursor-pointer`}
                    onClick={() => toggleExpand(row)}
                  >
                    {showSelection ? (
                      <Td className="w-10" onClick={(e) => e.stopPropagation()}>
                        <input
                          type="checkbox"
                          className="h-4 w-4 cursor-pointer rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
                          checked={checked}
                          onChange={() => onSelectionToggle?.(row.requestId)}
                        />
                      </Td>
                    ) : null}
                    <Td className="whitespace-normal">
                      <p
                        className="font-semibold text-gray-900"
                        title={row.title}
                      >
                        {truncate(row.title, 25) || "—"}
                      </p>
                      <p className="text-xs text-gray-500">
                        #{row.requestNumber || row.requestId}
                      </p>
                    </Td>
                    <Td className="max-w-40 truncate">
                      {row.categoryName || "—"}
                    </Td>
                    <Td>
                      <Badge className={priorityBadgeClass(row.priority)}>
                        {priorityLabel(row.priority)}
                      </Badge>
                    </Td>
                    <Td>
                      <div className="flex flex-col items-start">
                        <Badge className={statusBadgeClass(row.status)}>
                          {statusLabel(row.status)}
                        </Badge>
                        {row.isEscalated === true &&
                        Number(row.status) !== REQUEST_STATUS.Resolved &&
                        Number(row.status) !== REQUEST_STATUS.Closed ? (
                          <Badge className="mt-2 bg-orange-50 text-orange-800 ring-orange-100">
                            🚨 Escalated
                          </Badge>
                        ) : null}
                      </div>
                    </Td>
                    <Td>{row.dueDate ? fmtDate(row.dueDate) : "—"}</Td>
                    <Td>
                      <Badge className={getSLABadgeStyle(row.slaStatus)}>
                        {row.slaStatus || "No SLA"}
                      </Badge>
                    </Td>
                    <Td
                      className="text-right"
                      onClick={(e) => e.stopPropagation()}
                    >
                      <div className="flex flex-wrap justify-end gap-2">
                        {actions.map((action) => (
                          <Button
                            key={action.key}
                            type="button"
                            variant={action.variant}
                            className="min-h-0 px-2 py-1.5 text-xs"
                            disabled={action.disabled}
                            onClick={() => onAction(action.key, row)}
                          >
                            {action.label}
                          </Button>
                        ))}
                        {hasRole(roles, "Admin") ? (
                          <Button
                            type="button"
                            variant="secondary"
                            className="min-h-0 border-orange-300 bg-orange-50 px-2 py-1.5 text-xs text-orange-800 hover:bg-orange-100"
                            disabled={!isEscalateEligible(row)}
                            title={
                              row.isEscalated === true
                                ? "Already escalated"
                                : Number(row.status) ===
                                      REQUEST_STATUS.Resolved ||
                                    Number(row.status) === REQUEST_STATUS.Closed
                                  ? "Cannot escalate a completed request"
                                  : Number(row.status) ===
                                      REQUEST_STATUS.Rejected
                                    ? "Cannot escalate a rejected request"
                                    : Number(row.status) ===
                                        REQUEST_STATUS.PendingApproval
                                      ? "Cannot escalate while pending approval"
                                      : undefined
                            }
                            onClick={() => onAction("escalate", row)}
                          >
                            Escalate
                          </Button>
                        ) : null}
                      </div>
                    </Td>
                  </TableRow>
                  {expanded ? (
                    <TableRow className="bg-gray-50/80 hover:bg-gray-50/80">
                      <Td colSpan={showSelection ? 8 : 7} className="whitespace-normal py-4">
                        <dl className="grid gap-3 text-sm sm:grid-cols-2 lg:grid-cols-4">
                          <div className="col-span-full sm:col-span-2 lg:col-span-4">
                            <dt className="font-medium text-gray-500">
                              Description
                            </dt>
                            <dd className="mt-0.5 text-gray-900">
                              {row.description || "—"}
                            </dd>
                          </div>
                          <div>
                            <dt className="font-medium text-gray-500">
                              Created By
                            </dt>
                            <dd className="mt-0.5 text-gray-900">
                              {row.fullName || "—"}
                            </dd>
                          </div>
                          <div>
                            <dt className="font-medium text-gray-500">
                              Approval Name
                            </dt>
                            <dd className="mt-0.5 text-gray-900">
                              {row.approvalName || "—"}
                            </dd>
                          </div>
                          <div>
                            <dt className="font-medium text-gray-500">
                              Assigned User
                            </dt>
                            <dd className="mt-0.5 text-gray-900">
                              {row.assignedUser || "—"}
                            </dd>
                          </div>
                          <div>
                            <dt className="font-medium text-gray-500">
                              Created
                            </dt>
                            <dd className="mt-0.5 text-gray-900">
                              {fmtDate(row.createdOn)}
                            </dd>
                          </div>
                        </dl>
                      </Td>
                    </TableRow>
                  ) : null}
                </Fragment>
              );
            })
          )}
        </TableBody>
      </table>
    </div>
  );
}
