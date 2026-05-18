export const REQUEST_STATUS = {
  Open: 1,
  PendingApproval: 2,
  Approved: 3,
  Rejected: 4,
  Assigned: 5,
  InProgress: 6,
  Resolved: 7,
  Closed: 8,
}

export const REQUEST_PRIORITY = {
  High: 1,
  Medium: 2,
  Low: 3,
}

export const REMARK_ACTION_TYPE = {
  Approve: 1,
  Reject: 2,
  Assign: 3,
  Status: 4,
}

export const REQUEST_TABS = [
  { key: 'all', label: 'All', status: '' },
  { key: 'pending', label: 'Pending approval', status: REQUEST_STATUS.PendingApproval },
  { key: 'approved', label: 'Approved', status: REQUEST_STATUS.Approved },
  { key: 'assigned', label: 'Assigned', status: REQUEST_STATUS.Assigned },
  { key: 'inprogress', label: 'In progress', status: REQUEST_STATUS.InProgress },
  { key: 'resolved', label: 'Resolved', status: REQUEST_STATUS.Resolved },
  { key: 'rejected', label: 'Rejected', status: REQUEST_STATUS.Rejected },
]

export function normalizeRole(role) {
  return String(role ?? '').toLowerCase()
}

export function hasRole(roles, target) {
  const t = normalizeRole(target)
  return (roles ?? []).some((r) => normalizeRole(r) === t)
}

export function canCreateRequest(roles) {
  return hasRole(roles, 'Employee') || hasRole(roles, 'Manager')
}

export function canApproveReject(roles) {
  return hasRole(roles, 'Manager')
}

export function canAssign(roles) {
  return hasRole(roles, 'Admin')
}

export function canSupportStatus(roles) {
  return hasRole(roles, 'Support')
}

export function statusLabel(status) {
  const n = Number(status)
  switch (n) {
    case REQUEST_STATUS.Open:
      return 'Open'
    case REQUEST_STATUS.PendingApproval:
      return 'Pending approval'
    case REQUEST_STATUS.Approved:
      return 'Approved'
    case REQUEST_STATUS.Rejected:
      return 'Rejected'
    case REQUEST_STATUS.Assigned:
      return 'Assigned'
    case REQUEST_STATUS.InProgress:
      return 'In progress'
    case REQUEST_STATUS.Resolved:
      return 'Resolved'
    case REQUEST_STATUS.Closed:
      return 'Closed'
    default:
      return status != null && status !== '' ? String(status) : '—'
  }
}

export function priorityLabel(priority) {
  const n = Number(priority)
  switch (n) {
    case REQUEST_PRIORITY.High:
      return 'High'
    case REQUEST_PRIORITY.Medium:
      return 'Medium'
    case REQUEST_PRIORITY.Low:
      return 'Low'
    default:
      return priority != null && priority !== '' ? String(priority) : '—'
  }
}

export function statusBadgeClass(status) {
  const n = Number(status)
  switch (n) {
    case REQUEST_STATUS.Open:
      return 'bg-gray-100 text-gray-700 ring-gray-200'
    case REQUEST_STATUS.PendingApproval:
      return 'bg-yellow-50 text-yellow-800 ring-yellow-100'
    case REQUEST_STATUS.Approved:
      return 'bg-blue-50 text-blue-800 ring-blue-100'
    case REQUEST_STATUS.Rejected:
      return 'bg-red-50 text-red-800 ring-red-100'
    case REQUEST_STATUS.Assigned:
      return 'bg-indigo-50 text-indigo-800 ring-indigo-100'
    case REQUEST_STATUS.InProgress:
      return 'bg-purple-50 text-purple-800 ring-purple-100'
    case REQUEST_STATUS.Resolved:
      return 'bg-emerald-50 text-emerald-800 ring-emerald-100'
    case REQUEST_STATUS.Closed:
      return 'bg-gray-200 text-gray-800 ring-gray-300'
    default:
      return 'bg-gray-100 text-gray-600 ring-gray-200'
  }
}

export function priorityBadgeClass(priority) {
  const n = Number(priority)
  switch (n) {
    case REQUEST_PRIORITY.High:
      return 'bg-orange-50 text-orange-800 ring-orange-100'
    case REQUEST_PRIORITY.Medium:
      return 'bg-yellow-50 text-yellow-800 ring-yellow-100'
    case REQUEST_PRIORITY.Low:
      return 'bg-emerald-50 text-emerald-800 ring-emerald-100'
    default:
      return 'bg-gray-100 text-gray-600 ring-gray-200'
  }
}

export function fmtDate(v) {
  if (!v) return '—'
  try {
    const d = new Date(v)
    if (Number.isNaN(d.getTime())) return String(v)
    return d.toLocaleString()
  } catch {
    return String(v)
  }
}

/** @param {import('./requestUtils.js').REQUEST_STATUS[keyof typeof REQUEST_STATUS]} status */
export function getRowActions(row, roles) {
  const actions = []
  const status = Number(row?.status)

  actions.push({ key: 'view', label: 'View', variant: 'secondary' })

  if (canApproveReject(roles) && status === REQUEST_STATUS.PendingApproval) {
    actions.push({ key: 'approve', label: 'Approve', variant: 'primary' })
    actions.push({ key: 'reject', label: 'Reject', variant: 'danger' })
  }

  if (canAssign(roles) && status === REQUEST_STATUS.Approved) {
    actions.push({ key: 'assign', label: 'Assign', variant: 'primary' })
  }

  if (canSupportStatus(roles)) {
    if (status === REQUEST_STATUS.Assigned) {
      actions.push({ key: 'start', label: 'Start', variant: 'primary' })
    }
    if (status === REQUEST_STATUS.InProgress) {
      actions.push({ key: 'resolve', label: 'Resolve', variant: 'primary' })
    }
  }

  return actions
}
