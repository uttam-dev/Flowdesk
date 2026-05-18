import {
  fmtDate,
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

export function RequestDetails({ request }) {
  if (!request) return null

  return (
    <div className="rounded-xl border border-gray-200 bg-white p-4 shadow-sm">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <p className="text-xs font-medium uppercase tracking-wide text-gray-500">
            #{request.requestNumber || request.requestId}
          </p>
          <h2 className="mt-1 text-xl font-semibold text-gray-900">{request.title}</h2>
          <p className="mt-2 text-sm text-gray-600">{request.description || '—'}</p>
        </div>
        <div className="flex flex-wrap gap-2">
          <Badge className={priorityBadgeClass(request.priority)}>
            {priorityLabel(request.priority)}
          </Badge>
          <Badge className={statusBadgeClass(request.status)}>
            {statusLabel(request.status)}
          </Badge>
        </div>
      </div>

      <dl className="mt-6 grid gap-4 border-t border-gray-100 pt-4 text-sm sm:grid-cols-2 lg:grid-cols-3">
        <div>
          <dt className="font-medium text-gray-500">Category</dt>
          <dd className="mt-0.5 text-gray-900">{request.categoryName || '—'}</dd>
        </div>
        <div>
          <dt className="font-medium text-gray-500">Assigned to</dt>
          <dd className="mt-0.5 text-gray-900">{request.assignedToName || '—'}</dd>
        </div>
        <div>
          <dt className="font-medium text-gray-500">Approved by</dt>
          <dd className="mt-0.5 text-gray-900">{request.approvalName || '—'}</dd>
        </div>
        <div>
          <dt className="font-medium text-gray-500">Created by</dt>
          <dd className="mt-0.5 text-gray-900">{request.fullName || '—'}</dd>
        </div>
        <div>
          <dt className="font-medium text-gray-500">Created</dt>
          <dd className="mt-0.5 text-gray-900">{fmtDate(request.createdOn)}</dd>
        </div>
        <div>
          <dt className="font-medium text-gray-500">Updated</dt>
          <dd className="mt-0.5 text-gray-900">{fmtDate(request.updatedOn)}</dd>
        </div>
      </dl>
    </div>
  )
}
