import { Fragment, useState } from 'react'
import { Button } from '../../../components/ui/Button.jsx'
import { TableHead, TableBody, TableRow, Th, Td } from '../../../components/ui/Table.jsx'
import {
  REQUEST_STATUS,
  fmtDate,
  getRowActions,
  hasRole,
  priorityBadgeClass,
  priorityLabel,
  statusBadgeClass,
  statusLabel,
} from '../requestUtils.js'

function Badge({ className, children }) {
  return (
    <span
      className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ring-1 ring-inset ${className}`}
    >
      {children}
    </span>
  )
}

function idsMatch(a, b) {
  if (a == null || b == null) return false
  return String(a) === String(b)
}

function getManagerId(row) {
  return row.managerId ?? row.ManagerId
}

function getCreatedById(row) {
  return row.createdById ?? row.CreatedById
}

function getOwnershipLabel(row, currentUserId, roles) {
  if (!hasRole(roles, 'Manager') || currentUserId == null) return null
  if (idsMatch(getCreatedById(row), currentUserId)) return 'My Request'
  if (idsMatch(getManagerId(row), currentUserId)) return 'Team Request'
  return null
}

function getTableRowActions(row, roles, currentUserId) {
  const actions = getRowActions(row, roles)
  if (!hasRole(roles, 'Manager') || currentUserId == null) return actions

  const isOwn = idsMatch(getCreatedById(row), currentUserId)
  const isTeam =
    idsMatch(getManagerId(row), currentUserId) && !isOwn

  return actions.filter((action) => {
    if (action.key !== 'approve' && action.key !== 'reject') return true
    return isTeam && Number(row.status) === REQUEST_STATUS.PendingApproval
  })
}

export function RequestTable({ rows, loading, roles, currentUserId, onAction }) {
  const [expandedId, setExpandedId] = useState(null)

  if (loading === 'pending' && !rows.length) {
    return (
      <div className="rounded-xl border border-gray-200 bg-white p-10 text-center text-sm text-gray-600 shadow-sm">
        Loading requests…
      </div>
    )
  }

  function toggleExpand(row) {
    const id = row.requestId
    setExpandedId((prev) => (prev === id ? null : id))
  }

  return (
    <div className="w-full min-w-0 overflow-x-auto rounded-xl border border-gray-200 bg-white shadow-sm">
      <table className="min-w-full divide-y divide-gray-200 text-left text-sm">
        <TableHead>
          <TableRow className="hover:bg-transparent">
            <Th>Request</Th>
            <Th>Category</Th>
            <Th>Priority</Th>
            <Th>Status</Th>
            <Th className="text-right">Actions</Th>
          </TableRow>
        </TableHead>
        <TableBody>
          {rows.length === 0 ? (
            <TableRow>
              <Td colSpan={5} className="py-10 text-center text-gray-500">
                No requests found.
              </Td>
            </TableRow>
          ) : (
            rows.map((row) => {
              const expanded = expandedId === row.requestId
              const actions = getTableRowActions(row, roles, currentUserId)
              const ownershipLabel = getOwnershipLabel(row, currentUserId, roles)

              return (
                <Fragment key={row.requestId}>
                  <TableRow
                    className="cursor-pointer"
                    onClick={() => toggleExpand(row)}
                  >
                    <Td className="whitespace-normal">
                      <p className="font-semibold text-gray-900">{row.title || '—'}</p>
                      <p className="text-xs text-gray-500">
                        #{row.requestNumber || row.requestId}
                      </p>
                      {ownershipLabel ? (
                        <span
                          className={`mt-1 inline-flex rounded-full px-2 py-0.5 text-xs font-medium ring-1 ring-inset ${
                            ownershipLabel === 'My Request'
                              ? 'bg-sky-50 text-sky-800 ring-sky-100'
                              : 'bg-violet-50 text-violet-800 ring-violet-100'
                          }`}
                        >
                          {ownershipLabel}
                        </span>
                      ) : null}
                    </Td>
                    <Td className="max-w-[10rem] truncate">{row.categoryName || '—'}</Td>
                    <Td>
                      <Badge className={priorityBadgeClass(row.priority)}>
                        {priorityLabel(row.priority)}
                      </Badge>
                    </Td>
                    <Td>
                      <Badge className={statusBadgeClass(row.status)}>
                        {statusLabel(row.status)}
                      </Badge>
                    </Td>
                    <Td className="text-right" onClick={(e) => e.stopPropagation()}>
                      <div className="flex flex-wrap justify-end gap-2">
                        {actions.map((action) => (
                          <Button
                            key={action.key}
                            type="button"
                            variant={action.variant}
                            className="min-h-0 px-2 py-1.5 text-xs"
                            onClick={() => onAction(action.key, row)}
                          >
                            {action.label}
                          </Button>
                        ))}
                      </div>
                    </Td>
                  </TableRow>
                  {expanded ? (
                    <TableRow className="bg-gray-50/80 hover:bg-gray-50/80">
                      <Td colSpan={5} className="whitespace-normal py-4">
                        <dl className="grid gap-3 text-sm sm:grid-cols-2 lg:grid-cols-4">
                          <div>
                            <dt className="font-medium text-gray-500">Description</dt>
                            <dd className="mt-0.5 text-gray-900">
                              {row.description || '—'}
                            </dd>
                          </div>
                          <div>
                            <dt className="font-medium text-gray-500">Assigned user</dt>
                            <dd className="mt-0.5 text-gray-900">
                              {row.assignedToName || '—'}
                            </dd>
                          </div>
                          <div>
                            <dt className="font-medium text-gray-500">Approval</dt>
                            <dd className="mt-0.5 text-gray-900">
                              {row.approvalName || '—'}
                            </dd>
                          </div>
                          <div>
                            <dt className="font-medium text-gray-500">Created</dt>
                            <dd className="mt-0.5 text-gray-900">
                              {fmtDate(row.createdOn)}
                            </dd>
                          </div>
                        </dl>
                      </Td>
                    </TableRow>
                  ) : null}
                </Fragment>
              )
            })
          )}
        </TableBody>
      </table>
    </div>
  )
}
