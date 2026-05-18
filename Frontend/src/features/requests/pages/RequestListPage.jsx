import { useCallback, useEffect, useMemo, useState } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { Button } from '../../../components/ui/Button.jsx'
import { selectAuthUser, selectRoleNames } from '../../auth/authSlice.js'
import { RequestActionModal } from '../components/RequestActionModal.jsx'
import { RequestFilters } from '../components/RequestFilters.jsx'
import { RequestTable } from '../components/RequestTable.jsx'
import {
  approveRequest,
  assignRequest,
  clearRequestError,
  fetchRequestComments,
  fetchRequestDetail,
  fetchRequests,
  rejectRequest,
  selectRequestActiveTab,
  selectRequestCategoryFilter,
  selectRequestError,
  selectRequestList,
  selectRequestLoading,
  selectRequestMutationLoading,
  selectRequestNumberFilter,
  selectRequestPage,
  selectRequestPageSize,
  selectRequestPriorityFilter,
  selectRequestStatusFilter,
  selectRequestTotal,
  selectRequestTotalPages,
  setActiveTab,
  setCategoryFilter,
  setPage,
  setPageSize,
  setPriorityFilter,
  setRequestNumberFilter,
  setStatusFilter,
  updateRequestStatus,
} from '../requestSlice.js'
import { REQUEST_TABS, REQUEST_STATUS, hasRole } from '../requestUtils.js'

export function RequestListPage() {
  const dispatch = useDispatch()
  const navigate = useNavigate()
  const roles = useSelector(selectRoleNames)
  const authUser = useSelector(selectAuthUser)
  const currentUserId = authUser?.id ?? null
  const isManager = hasRole(roles, 'Manager')
  const isSupport = hasRole(roles, 'Support')

  const items = useSelector(selectRequestList)
  const loading = useSelector(selectRequestLoading)
  const error = useSelector(selectRequestError)
  const page = useSelector(selectRequestPage)
  const pageSize = useSelector(selectRequestPageSize)
  const total = useSelector(selectRequestTotal)
  const totalPages = useSelector(selectRequestTotalPages)
  const status = useSelector(selectRequestStatusFilter)
  const priority = useSelector(selectRequestPriorityFilter)
  const categoryId = useSelector(selectRequestCategoryFilter)
  const requestNumber = useSelector(selectRequestNumberFilter)
  const activeTab = useSelector(selectRequestActiveTab)
  const mutating = useSelector(selectRequestMutationLoading)
  
  const filteredItems = useMemo(() => {
    let result = items
    if (activeTab === 'resolved') {
      result = result.filter(
        (it) =>
          Number(it.status) === REQUEST_STATUS.Resolved ||
          Number(it.status) === REQUEST_STATUS.Closed
      )
    }
    if (isSupport) {
      result = result.filter(
        (it) =>
          Number(it.status) === REQUEST_STATUS.Assigned ||
          Number(it.status) === REQUEST_STATUS.InProgress ||
          Number(it.status) === REQUEST_STATUS.Resolved ||
          Number(it.status) === REQUEST_STATUS.Closed
      )
    }
    return result
  }, [items, isSupport, activeTab])
  
  const [actionModal, setActionModal] = useState({ open: false, type: null, row: null })
  const [actionError, setActionError] = useState(null)

  const visibleTabs = useMemo(() => {
    if (isManager) {
      return [
        { key: 'my', label: 'My Requests', status: '' },
        { key: 'team', label: 'Team Requests', status: '' },
      ]
    }
    if (isSupport) {
      return REQUEST_TABS.filter((tab) =>
        ['all', 'assigned', 'inprogress', 'resolved'].includes(tab.key)
      )
    }
    return REQUEST_TABS
  }, [isManager, isSupport])

  const listQuery = useCallback(
    () => ({
      page,
      pageSize,
      status,
      priority,
      categoryId,
      requestNumber,
      activeTab,
    }),
    [page, pageSize, status, priority, categoryId, requestNumber, activeTab],
  )

  useEffect(() => {
    dispatch(fetchRequests(listQuery()))
  }, [dispatch, listQuery])

  useEffect(() => {
    if (isManager && (activeTab === 'all' || activeTab === 'rejected')) {
      dispatch(setActiveTab({ key: 'my', status: '' }))
    }
  }, [isManager, activeTab, dispatch])

  useEffect(() => {
    if (isSupport && !['all', 'assigned', 'inprogress', 'resolved'].includes(activeTab)) {
      dispatch(setActiveTab({ key: 'all', status: '' }))
    }
  }, [isSupport, activeTab, dispatch])

  useEffect(() => {
    if (error) {
      toast.error(error)
      dispatch(clearRequestError())
    }
  }, [error, dispatch])

  const displayTotal = activeTab === 'resolved' ? filteredItems.length : total
  const displayTotalPages = activeTab === 'resolved' ? Math.ceil(filteredItems.length / pageSize) : totalPages

  const safeTotalPages = Math.max(1, displayTotalPages || 1)
  const firstItem = displayTotal === 0 ? 0 : (page - 1) * pageSize + 1
  const lastItem = Math.min(page * pageSize, displayTotal)

  async function refreshAfterAction(requestId) {
    await dispatch(fetchRequests(listQuery())).unwrap()
    if (requestId) {
      await dispatch(fetchRequestDetail(requestId)).unwrap().catch(() => {})
      await dispatch(fetchRequestComments(requestId)).unwrap().catch(() => {})
    }
  }

  function handleTableAction(key, row) {
    if (key === 'view') {
      navigate(`/requests/${row.requestId}`)
      return
    }
    setActionError(null)
    setActionModal({ open: true, type: key, row })
  }

  async function handleActionConfirm(payload) {
    if (!actionModal.row) return
    const id = actionModal.row.requestId
    setActionError(null)
    try {
      if (actionModal.type === 'approve') {
        await dispatch(approveRequest({ id, ...payload })).unwrap()
        toast.success('Request approved')
      } else if (actionModal.type === 'reject') {
        await dispatch(rejectRequest({ id, ...payload })).unwrap()
        toast.success('Request rejected')
      } else if (actionModal.type === 'assign') {
        await dispatch(assignRequest({ id, ...payload })).unwrap()
        toast.success('Request assigned')
      } else if (actionModal.type === 'start' || actionModal.type === 'resolve') {
        await dispatch(updateRequestStatus({ id, ...payload })).unwrap()
        toast.success(
          actionModal.type === 'start' ? 'Request started' : 'Request resolved',
        )
      }
      setActionModal({ open: false, type: null, row: null })
      await refreshAfterAction(id)
    } catch (e) {
      setActionError(String(e))
    }
  }

  return (
    <div className="space-y-6 overflow-x-hidden">
      <div className="flex flex-wrap gap-2 border-b border-gray-200 pb-1">
        {visibleTabs.map((tab) => (
          <button
            key={tab.key}
            type="button"
            onClick={() =>
              dispatch(setActiveTab({ key: tab.key, status: tab.status }))
            }
            className={[
              'rounded-lg px-3 py-2 text-sm font-medium transition',
              activeTab === tab.key
                ? 'bg-indigo-50 text-indigo-800 ring-1 ring-indigo-100'
                : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900',
            ].join(' ')}
          >
            {tab.label}
          </button>
        ))}
      </div>

      <RequestFilters
        roles={roles}
        pageSize={pageSize}
        status={status}
        priority={priority}
        categoryId={categoryId}
        requestNumber={requestNumber}
        hideRejected={isManager}
        activeTab={activeTab}
        onPageSizeChange={(v) => dispatch(setPageSize(v))}
        onStatusChange={(v) => dispatch(setStatusFilter(v))}
        onPriorityChange={(v) => dispatch(setPriorityFilter(v))}
        onCategoryChange={(v) => dispatch(setCategoryFilter(v))}
        onRequestNumberChange={(v) => dispatch(setRequestNumberFilter(v))}
      />

      <RequestTable
        rows={filteredItems}
        loading={loading}
        roles={roles}
        currentUserId={currentUserId}
        activeTab={activeTab}
        onAction={handleTableAction}
      />

      <div className="flex flex-col items-center justify-between gap-3 border-t border-gray-200 pt-4 sm:flex-row">
        <p className="text-sm text-gray-600">
          {displayTotal > 0
            ? `Showing ${firstItem}-${lastItem} of ${displayTotal}`
            : 'No requests'}
          {' · '}
          Page {page} of {safeTotalPages}
        </p>
        <div className="flex gap-2">
          <Button
            type="button"
            variant="secondary"
            disabled={page <= 1}
            onClick={() => dispatch(setPage(page - 1))}
          >
            Previous
          </Button>
          <Button
            type="button"
            variant="secondary"
            disabled={page >= safeTotalPages}
            onClick={() => dispatch(setPage(page + 1))}
          >
            Next
          </Button>
        </div>
      </div>

      <RequestActionModal
        open={actionModal.open}
        actionType={actionModal.type}
        saving={mutating}
        serverError={actionError}
        onClose={() => {
          if (!mutating) setActionModal({ open: false, type: null, row: null })
        }}
        onConfirm={handleActionConfirm}
      />
    </div>
  )
}
