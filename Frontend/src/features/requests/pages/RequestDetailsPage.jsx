import { useCallback, useEffect, useState } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { Link, useParams } from 'react-router-dom'
import { toast } from 'sonner'
import { Button } from '../../../components/ui/Button.jsx'
import { selectRoleNames } from '../../auth/authSlice.js'
import { AuditTrail } from '../components/AuditTrail.jsx'
import { RequestActionModal } from '../components/RequestActionModal.jsx'
import { RequestComments } from '../components/RequestComments.jsx'
import { RequestDetails } from '../components/RequestDetails.jsx'
import {
  approveRequest,
  assignRequest,
  fetchRequestComments,
  fetchRequestDetail,
  fetchRequests,
  rejectRequest,
  selectRequestComments,
  selectRequestCommentsLoading,
  selectRequestDetail,
  selectRequestDetailError,
  selectRequestDetailLoading,
  selectRequestMutationLoading,
  selectRequestPage,
  selectRequestPageSize,
  selectRequestStatusFilter,
  selectRequestPriorityFilter,
  selectRequestCategoryFilter,
  selectRequestNumberFilter,
  updateRequestStatus,
} from '../requestSlice.js'
import { getRowActions, hasRole } from '../requestUtils.js'

export function RequestDetailsPage() {
  const { id } = useParams()
  const dispatch = useDispatch()
  const roles = useSelector(selectRoleNames)
  const detail = useSelector(selectRequestDetail)
  const detailLoading = useSelector(selectRequestDetailLoading)
  const detailError = useSelector(selectRequestDetailError)
  const comments = useSelector(selectRequestComments)
  const commentsLoading = useSelector(selectRequestCommentsLoading)
  const mutating = useSelector(selectRequestMutationLoading)

  const page = useSelector(selectRequestPage)
  const pageSize = useSelector(selectRequestPageSize)
  const status = useSelector(selectRequestStatusFilter)
  const priority = useSelector(selectRequestPriorityFilter)
  const categoryId = useSelector(selectRequestCategoryFilter)
  const requestNumber = useSelector(selectRequestNumberFilter)

  const [actionModal, setActionModal] = useState({ open: false, type: null })
  const [actionError, setActionError] = useState(null)

  const loadDetail = useCallback(() => {
    if (!id) return
    dispatch(fetchRequestDetail(id))
    dispatch(fetchRequestComments(id))
  }, [dispatch, id])

  useEffect(() => {
    loadDetail()
  }, [loadDetail])

  useEffect(() => {
    if (detailError) {
      toast.error(detailError)
    }
  }, [detailError])

  const actions =
    detail && detail.requestId
      ? getRowActions(detail, roles).filter((a) => a.key !== 'view')
      : []
  const isAdmin = hasRole(roles, 'Admin')

  async function refreshAll() {
    if (!id) return
    await dispatch(fetchRequestDetail(id)).unwrap()
    await dispatch(fetchRequestComments(id)).unwrap()
    await dispatch(
      fetchRequests({
        page,
        pageSize,
        status,
        priority,
        categoryId,
        requestNumber,
      }),
    ).unwrap().catch(() => {})
  }

  function openAction(key) {
    setActionError(null)
    setActionModal({ open: true, type: key })
  }

  async function handleActionConfirm(payload) {
    if (!id) return
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
      setActionModal({ open: false, type: null })
      await refreshAll()
    } catch (e) {
      setActionError(String(e))
    }
  }

  if (detailLoading === 'pending' && !detail) {
    return (
      <div className="rounded-xl border border-gray-200 bg-white p-10 text-center text-sm text-gray-600 shadow-sm">
        Loading request…
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <Link
        to="/requests"
        className="inline-flex text-sm font-medium text-indigo-600 hover:text-indigo-800"
      >
        ← Back to requests
      </Link>

      <RequestDetails request={detail} />

      {actions.length > 0 ? (
        <div className="flex flex-wrap gap-2 rounded-xl border border-gray-200 bg-white p-4 shadow-sm">
          <p className="w-full text-sm font-medium text-gray-700">Actions</p>
          {actions.map((action) => (
            <Button
              key={action.key}
              type="button"
              variant={action.variant}
              onClick={() => openAction(action.key)}
            >
              {action.label}
            </Button>
          ))}
        </div>
      ) : null}

      {isAdmin ? <AuditTrail requestId={id} /> : null}

      <RequestComments comments={comments} loading={commentsLoading} />

      <RequestActionModal
        open={actionModal.open}
        actionType={actionModal.type}
        saving={mutating}
        serverError={actionError}
        onClose={() => {
          if (!mutating) setActionModal({ open: false, type: null })
        }}
        onConfirm={handleActionConfirm}
      />
    </div>
  )
}
