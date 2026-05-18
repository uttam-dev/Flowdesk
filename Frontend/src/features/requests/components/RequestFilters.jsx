import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { Button } from '../../../components/ui/Button.jsx'
import { fetchActiveCategoriesApi } from '../requestApi.js'
import { REQUEST_PRIORITY, REQUEST_STATUS, canCreateRequest, hasRole } from '../requestUtils.js'

const selectClass =
  'min-h-[44px] rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm transition focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/20'

const inputClass =
  'min-h-[44px] w-full rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm transition focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/20'

export function RequestFilters({
  roles,
  pageSize,
  status,
  priority,
  categoryId,
  requestNumber,
  hideRejected = false,
  activeTab,
  onPageSizeChange,
  onStatusChange,
  onPriorityChange,
  onCategoryChange,
  onRequestNumberChange,
}) {
  const isSupport = hasRole(roles, 'Support')
  const isManager = hasRole(roles, 'Manager')
  const showCreateBtn = canCreateRequest(roles) && (!isManager || activeTab === 'my')
  const [categories, setCategories] = useState([])
  const [searchInput, setSearchInput] = useState(requestNumber)

  useEffect(() => {
    let cancelled = false
    fetchActiveCategoriesApi()
      .then((list) => {
        if (!cancelled) setCategories(list)
      })
      .catch(() => {
        if (!cancelled) setCategories([])
      })
    return () => {
      cancelled = true
    }
  }, [])

  useEffect(() => {
    setSearchInput(requestNumber)
  }, [requestNumber])

  function applySearch() {
    onRequestNumberChange(searchInput.trim())
  }

  function clearSearch() {
    setSearchInput('')
    onRequestNumberChange('')
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-col gap-4 xl:flex-row xl:items-end xl:justify-between">
        <div className="grid w-full gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <label className="grid gap-1 text-sm font-medium text-gray-700">
            Page size
            <select
              className={selectClass}
              value={pageSize}
              onChange={(ev) => onPageSizeChange(Number(ev.target.value))}
            >
              {[5, 10, 25, 50].map((size) => (
                <option key={size} value={size}>
                  {size}
                </option>
              ))}
            </select>
          </label>

          <label className="grid gap-1 text-sm font-medium text-gray-700">
            Status
            <select
              className={selectClass}
              value={status}
              onChange={(ev) => onStatusChange(ev.target.value)}
            >
              <option value="">All</option>
              {isSupport ? (
                <>
                  <option value={REQUEST_STATUS.Assigned}>Assigned</option>
                  <option value={REQUEST_STATUS.InProgress}>In progress</option>
                  <option value={REQUEST_STATUS.Closed}>Closed</option>
                </>
              ) : (
                <>
                  <option value={REQUEST_STATUS.Open}>Open</option>
                  <option value={REQUEST_STATUS.PendingApproval}>Pending approval</option>
                  <option value={REQUEST_STATUS.Approved}>Approved</option>
                  {!hideRejected ? (
                    <option value={REQUEST_STATUS.Rejected}>Rejected</option>
                  ) : null}
                  <option value={REQUEST_STATUS.Assigned}>Assigned</option>
                  <option value={REQUEST_STATUS.InProgress}>In progress</option>
                  <option value={REQUEST_STATUS.Resolved}>Resolved</option>
                  <option value={REQUEST_STATUS.Closed}>Closed</option>
                </>
              )}
            </select>
          </label>

          <label className="grid gap-1 text-sm font-medium text-gray-700">
            Priority
            <select
              className={selectClass}
              value={priority}
              onChange={(ev) => onPriorityChange(ev.target.value)}
            >
              <option value="">All</option>
              <option value={REQUEST_PRIORITY.High}>High</option>
              <option value={REQUEST_PRIORITY.Medium}>Medium</option>
              <option value={REQUEST_PRIORITY.Low}>Low</option>
            </select>
          </label>

          <label className="grid gap-1 text-sm font-medium text-gray-700">
            Category
            <select
              className={selectClass}
              value={categoryId}
              onChange={(ev) => onCategoryChange(ev.target.value)}
            >
              <option value="">All</option>
              {categories.map((c) => (
                <option key={c.categoryId} value={c.categoryId}>
                  {c.categoryName}
                </option>
              ))}
            </select>
          </label>
        </div>

        {showCreateBtn ? (
          <Link
            to="/requests/new"
            className="inline-flex min-h-[44px] shrink-0 items-center justify-center rounded-lg bg-emerald-600 px-4 py-2 text-sm font-semibold text-white shadow-sm transition hover:bg-emerald-700"
          >
            Create request
          </Link>
        ) : null}
      </div>

      <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
        <label className="grid min-w-0 flex-1 gap-1 text-sm font-medium text-gray-700">
          Search
          <input
            type="text"
            className={inputClass}
            value={searchInput}
            onChange={(ev) => setSearchInput(ev.target.value)}
            onKeyDown={(ev) => {
              if (ev.key === 'Enter') applySearch()
            }}
            placeholder="Request number or title"
          />
        </label>
        <div className="flex shrink-0 gap-2">
          <Button
            type="button"
            variant="primary"
            className="min-h-[44px]"
            onClick={applySearch}
          >
            Search
          </Button>
          <Button
            type="button"
            variant="secondary"
            className="min-h-[44px]"
            onClick={clearSearch}
          >
            Clear
          </Button>
        </div>
      </div>
    </div>
  )
}
