import { useEffect, useState } from 'react'
import { apiClient } from '../../../services/apiClient.js'
import { parseApiError } from '../requestApi.js'

const STATUS_MAP = {
  1: { label: 'Open', color: 'gray' },
  2: { label: 'Pending approval', color: 'amber' },
  3: { label: 'Approved', color: 'green' },
  4: { label: 'Rejected', color: 'red' },
  5: { label: 'Assigned', color: 'blue' },
  6: { label: 'In progress', color: 'purple' },
  7: { label: 'Resolved', color: 'teal' },
  8: { label: 'Closed', color: 'gray' },
}

const BADGE_CLASSES = {
  gray: 'bg-gray-100 text-gray-700 ring-gray-200',
  amber: 'bg-yellow-50 text-yellow-800 ring-yellow-100',
  green: 'bg-emerald-50 text-emerald-800 ring-emerald-100',
  red: 'bg-red-50 text-red-800 ring-red-100',
  blue: 'bg-blue-50 text-blue-800 ring-blue-100',
  purple: 'bg-purple-50 text-purple-800 ring-purple-100',
  teal: 'bg-emerald-50 text-emerald-800 ring-emerald-100',
}

const DOT_CLASSES = {
  gray: 'bg-gray-400 ring-gray-100',
  amber: 'bg-yellow-400 ring-yellow-100',
  green: 'bg-emerald-500 ring-emerald-100',
  red: 'bg-red-500 ring-red-100',
  blue: 'bg-blue-500 ring-blue-100',
  purple: 'bg-purple-500 ring-purple-100',
  teal: 'bg-emerald-500 ring-emerald-100',
}

function Badge({ className, children }) {
  return (
    <span
      className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ring-1 ring-inset ${className}`}
    >
      {children}
    </span>
  )
}

function HistoryIcon() {
  return (
    <svg
      className="h-4 w-4 text-gray-500"
      fill="none"
      viewBox="0 0 24 24"
      strokeWidth={1.5}
      stroke="currentColor"
      aria-hidden
    >
      <path
        strokeLinecap="round"
        strokeLinejoin="round"
        d="M12 6v6l4 2m5-2a9 9 0 11-2.64-6.36M21 3v5h-5"
      />
    </svg>
  )
}

function parseArrayPayload(data) {
  const root = data?.data !== undefined ? data.data : data
  if (Array.isArray(root)) return root
  return []
}

function statusMeta(status) {
  return (
    STATUS_MAP[Number(status)] ?? {
      label: status != null && status !== '' ? String(status) : '-',
      color: 'gray',
    }
  )
}

function StatusPill({ status }) {
  const meta = statusMeta(status)
  return (
    <Badge className={BADGE_CLASSES[meta.color] ?? BADGE_CLASSES.gray}>
      {meta.label}
    </Badge>
  )
}

function formatAuditDate(value) {
  if (!value) return '-'
  try {
    const normalized = String(value).includes(' ')
      ? String(value).replace(' ', 'T')
      : value
    const date = new Date(normalized)
    if (Number.isNaN(date.getTime())) return String(value)
    return date.toLocaleString(undefined, {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    })
  } catch {
    return String(value)
  }
}

function initials(name) {
  const first = String(name || '').trim().charAt(0)
  return first ? first.toUpperCase() : '-'
}

function TimelineLabel({ entry }) {
  if (entry.oldStatus == null) {
    return <p className="text-sm font-medium text-gray-900">Request created</p>
  }

  return (
    <div className="flex flex-wrap items-center gap-2">
      <StatusPill status={entry.oldStatus} />
      <span className="text-sm text-gray-400">&rarr;</span>
      <StatusPill status={entry.newStatus} />
    </div>
  )
}

function AuditSkeleton() {
  return (
    <div className="space-y-4">
      {[0, 1, 2].map((item) => (
        <div key={item} className="flex gap-3">
          <div className="mt-1 h-3 w-3 rounded-full bg-gray-200" />
          <div className="flex-1 space-y-2">
            <div className="h-4 w-40 rounded bg-gray-100" />
            <div className="h-3 w-64 max-w-full rounded bg-gray-100" />
          </div>
        </div>
      ))}
    </div>
  )
}

export function AuditTrail({ requestId }) {
  const [history, setHistory] = useState([])
  const [loading, setLoading] = useState('idle')
  const [error, setError] = useState(null)

  useEffect(() => {
    if (!requestId) return

    let cancelled = false

    Promise.resolve()
      .then(() => {
        if (cancelled) return null
        setLoading('pending')
        setError(null)
        return apiClient.get(`/requests/${requestId}/history`)
      })
      .then((response) => {
        if (!response) return
        const { data } = response
        if (!cancelled) {
          setHistory(parseArrayPayload(data))
          setLoading('succeeded')
        }
      })
      .catch((err) => {
        if (!cancelled) {
          setHistory([])
          setError(parseApiError(err))
          setLoading('failed')
        }
      })

    return () => {
      cancelled = true
    }
  }, [requestId])

  return (
    <div className="rounded-xl border border-gray-200 bg-white p-4 shadow-sm">
      <div className="flex items-center justify-between gap-3">
        <h3 className="inline-flex items-center gap-2 text-sm font-semibold text-gray-900">
          <HistoryIcon />
          Audit trail
        </h3>
        <p className="text-xs font-medium text-gray-500">
          {history.length} {history.length === 1 ? 'event' : 'events'}
        </p>
      </div>

      <div className="mt-4">
        {loading === 'pending' ? (
          <AuditSkeleton />
        ) : error ? (
          <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-800 ring-1 ring-inset ring-red-100">
            {error}
          </p>
        ) : history.length === 0 ? (
          <p className="py-4 text-center text-sm text-gray-500">
            No history available.
          </p>
        ) : (
          <ol className="space-y-0">
            {history.map((entry, index) => {
              const meta = statusMeta(entry.newStatus)
              const isLast = index === history.length - 1

              return (
                <li
                  key={`${entry.changedOn}-${entry.changedBy}-${index}`}
                  className="relative flex gap-3 pb-5 last:pb-0"
                >
                  <div className="relative flex w-4 justify-center">
                    <span
                      className={`mt-1 h-3 w-3 rounded-full ring-4 ${DOT_CLASSES[meta.color] ?? DOT_CLASSES.gray}`}
                    />
                    {!isLast ? (
                      <span className="absolute top-5 bottom-0 w-px bg-gray-200" />
                    ) : null}
                  </div>

                  <div className="min-w-0 flex-1">
                    <TimelineLabel entry={entry} />
                    <div className="mt-2 flex min-w-0 items-center gap-2 text-xs text-gray-500">
                      <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-gray-100 text-xs font-semibold text-gray-600 ring-1 ring-gray-200">
                        {initials(entry.changedBy)}
                      </span>
                      <span className="truncate font-medium text-gray-700">
                        {entry.changedBy || '-'}
                      </span>
                      <span className="text-gray-300">&middot;</span>
                      <span className="shrink-0">
                        {formatAuditDate(entry.changedOn)}
                      </span>
                    </div>
                  </div>
                </li>
              )
            })}
          </ol>
        )}
      </div>
    </div>
  )
}
