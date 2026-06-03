import { useEffect, useState } from 'react'
import { Modal } from '../../../components/ui/Modal.jsx'
import { Button } from '../../../components/ui/Button.jsx'
import {
  fetchRemarksApi,
  fetchSupportUsersApi,
} from '../requestApi.js'
import { REMARK_ACTION_TYPE, REQUEST_STATUS } from '../requestUtils.js'

const selectClass =
  'w-full rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm transition focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/20'

const textareaClass =
  'w-full rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm transition focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/20'

const CONFIG = {
  approve: {
    title: 'Approve request',
    remarkActionType: REMARK_ACTION_TYPE.Approve,
    remarkRequired: false,
    commentRequired: false,
    confirmLabel: 'Approve',
    tone: 'primary',
  },
  reject: {
    title: 'Reject request',
    remarkActionType: REMARK_ACTION_TYPE.Reject,
    remarkRequired: true,
    commentRequired: true,
    confirmLabel: 'Reject',
    tone: 'danger',
  },
  assign: {
    title: 'Assign request',
    remarkActionType: REMARK_ACTION_TYPE.Assign,
    remarkRequired: false,
    commentRequired: false,
    confirmLabel: 'Assign',
    tone: 'primary',
    showSupport: true,
  },
  bulkAssign: {
    title: 'Assign selected requests',
    remarkActionType: REMARK_ACTION_TYPE.Assign,
    remarkRequired: false,
    commentRequired: false,
    confirmLabel: 'Assign',
    tone: 'primary',
    showSupport: true,
  },
  start: {
    title: 'Start request',
    remarkActionType: REMARK_ACTION_TYPE.Status,
    remarkRequired: false,
    commentRequired: false,
    confirmLabel: 'Start',
    tone: 'primary',
    targetStatus: REQUEST_STATUS.InProgress,
  },
  resolve: {
    title: 'Resolve request',
    remarkActionType: REMARK_ACTION_TYPE.Status,
    remarkRequired: false,
    commentRequired: false,
    confirmLabel: 'Resolve',
    tone: 'primary',
    targetStatus: REQUEST_STATUS.Resolved,
  },
}

export function RequestActionModal({
  open,
  actionType,
  saving,
  serverError,
  onClose,
  onConfirm,
}) {
  const config = CONFIG[actionType]
  const [remarks, setRemarks] = useState([])
  const [supportUsers, setSupportUsers] = useState([])
  const [remarksId, setRemarksId] = useState(null)
  const [assignToId, setAssignToId] = useState(null)
  const [commentText, setCommentText] = useState('')
  const [error, setError] = useState(null)
  


  useEffect(() => {
    if (!open || !config) return
    setRemarksId(null)
    setAssignToId(null)
    setCommentText('')
    setError(null)

    let cancelled = false

    fetchRemarksApi(config.remarkActionType)
      .then((list) => {
        if (!cancelled) setRemarks(list)
      })
      .catch(() => {
        if (!cancelled) setRemarks([])
      })

    if (config.showSupport) {
      fetchSupportUsersApi()
        .then((list) => {
          if (!cancelled) setSupportUsers(list)
        })
        .catch(() => {
          if (!cancelled) setSupportUsers([])
        })
    }

    return () => {
      cancelled = true
    }
  }, [open, actionType, config])

  if (!open || !config) return null

  function validate() {
    if (config.showSupport && (assignToId === null || assignToId === '')) return 'Support user is required'
    if (config.remarkRequired && (remarksId === null || remarksId === '')) return 'Remark is required'
    const text = commentText.trim()
    if (config.commentRequired && !text) return 'Comment is required'
    if (text.length > 200) return 'Comment must be at most 200 characters'
    return null
  }

  function handleConfirm() {
    const err = validate()
    if (err) {
      setError(err)
      return
    }
    setError(null)

    const payload = {
      commentText: commentText.trim() || undefined,
    }

    const toIdValue = (val) => {
      if (val === null || val === '') return undefined
      const num = Number(val)
      return Number.isNaN(num) ? val : num
    }

    if (actionType === 'approve') {
      const rid = toIdValue(remarksId)
      if (rid !== undefined) payload.remarksId = rid
    } else if (actionType === 'reject') {
      payload.remarksId = toIdValue(remarksId)
      payload.commentText = commentText.trim()
    } else if (actionType === 'assign' || actionType === 'bulkAssign') {
      payload.assignToId = toIdValue(assignToId)
      const rid = toIdValue(remarksId)
      if (rid !== undefined) payload.remarksId = rid
    } else if (actionType === 'start' || actionType === 'resolve') {
      payload.status = config.targetStatus
      const rid = toIdValue(remarksId)
      if (rid !== undefined) payload.remarksId = rid
    }

    onConfirm(payload)
  }

  const combinedError = error || serverError

  return (
    <Modal
      open
      onClose={saving ? () => {} : onClose}
      title={config.title}
      closeOnOverlayClick={!saving}
      closeOnEscape={!saving}
      footer={
        <>
          <Button type="button" variant="secondary" disabled={saving} onClick={onClose}>
            Cancel
          </Button>
          <Button
            type="button"
            variant={config.tone === 'danger' ? 'danger' : 'primary'}
            loading={saving}
            disabled={saving}
            onClick={handleConfirm}
          >
            {config.confirmLabel}
          </Button>
        </>
      }
    >
      <div className="space-y-4">
        {combinedError ? (
          <p
            className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700"
            role="alert"
          >
            {combinedError}
          </p>
        ) : null}

        {config.showSupport ? (
          <label className="grid gap-1 text-sm font-medium text-gray-700">
            Support user
            <select
              className={selectClass}
              value={assignToId || ''}
              onChange={(ev) => setAssignToId(ev.target.value || null)}
              disabled={saving}
            >
              <option value="">Select support user</option>
              {supportUsers.map((u, idx) => (
                <option key={u.userId ?? idx} value={u.userId != null ? String(u.userId) : ''}>
                  {u.fullName}
                </option>
              ))}
            </select>
          </label>
        ) : null}

        <label className="grid gap-1 text-sm font-medium text-gray-700">
          Remark{config.remarkRequired ? ' *' : ''}
          <select
            className={selectClass}
            value={remarksId || ''}
            onChange={(ev) => setRemarksId(ev.target.value || null)}
            disabled={saving}
          >
            <option value="">
              {config.remarkRequired ? 'Select remark' : 'None'}
            </option>
            {remarks.map((r, idx) => (
              <option key={r.masterRemarkId ?? idx} value={r.masterRemarkId != null ? String(r.masterRemarkId) : ''}>
                {r.remarkText}
              </option>
            ))}
          </select>
        </label>

        <label className="grid gap-1 text-sm font-medium text-gray-700">
          Comment{config.commentRequired ? ' *' : ''}
          <textarea
            className={textareaClass}
            rows={3}
            value={commentText}
            onChange={(ev) => setCommentText(ev.target.value)}
            disabled={saving}
            maxLength={200}
            placeholder={config.commentRequired ? 'Required' : 'Optional'}
          />
          <span className="text-xs text-gray-500">{commentText.length}/200</span>
        </label>
      </div>
    </Modal>
  )
}
