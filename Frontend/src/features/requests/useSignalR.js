/**
 * useSignalR.js
 *
 * React hook that manages the SignalR connection lifecycle for the request list page.
 *
 * Subscribes to three server events:
 *  +--------------------------+----------------------------------------------------+
 *  | Event                    | Sent to (server-side groups)                       |
 *  +--------------------------+----------------------------------------------------+
 *  | RequestUpdated           | role-Admin, role-Manager, user-{managerId},         |
 *  | (request created)        | user-{employeeId}                                   |
 *  +--------------------------+----------------------------------------------------+
 *  | RequestStatusUpdated     | role-Admin, user-{employeeId},                      |
 *  | (approve/reject/status)  | user-{assignedToId}, user-{managerId}               |
 *  +--------------------------+----------------------------------------------------+
 *  | RequestAssigned          | role-Support, user-{assignedToId}, role-Admin       |
 *  +--------------------------+----------------------------------------------------+
 *
 * The server already targets the correct groups, so ALL authenticated users
 * subscribe on the frontend — server-side groups control who actually receives
 * each message. No role-filtering needed on the client.
 *
 * On any event: dispatch fetchRequests() (safe refetch, picks up current
 * filters/page/tab from Redux state). No optimistic patching.
 * Also shows role-based toast notifications.
 */

import { useCallback, useEffect, useRef } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { toast } from 'sonner'
import { selectAuthUser, selectRoleNames } from '../auth/authSlice.js'
import { fetchRequests } from './requestSlice.js'
import {
  startSignalR,
  onRequestEvent,
  offRequestEvent,
  onStatusUpdatedEvent,
  offStatusUpdatedEvent,
  onAssignedEvent,
  offAssignedEvent,
} from '../../services/signalrService.js'

/**
 * Connects to SignalR and subscribes to all three request events.
 * Automatically disconnects handlers on unmount.
 * @returns {void}
 */
export function useSignalR() {
  const dispatch = useDispatch()
  const user = useSelector(selectAuthUser)
  const roles = useSelector(selectRoleNames)

  function hasRole(target) {
    return roles.some(r => String(r).toLowerCase() === target.toLowerCase())
  }

  // Stable handler refs — prevents subscribe/unsubscribe identity mismatches
  // across re-renders. Each ref holds the handler registered with the hub.
  const createdHandlerRef = useRef(null)
  const statusHandlerRef = useRef(null)
  const assignedHandlerRef = useRef(null)

  /**
   * Shared dispatch action: refetch the current list with active filters.
   * We use useCallback so the handler identity is stable (no extra on/off cycles).
   */
  const handleRequestCreated = useCallback(
    (payload) => {
      console.info('[SignalR] RequestUpdated (created):', payload)
      dispatch(fetchRequests())

      const reqId = payload.requestNumber ?? payload.RequestNumber ?? payload.requestId ?? payload.RequestId
      const suffix = reqId ? `Request #${reqId}` : 'A new request'

      if (hasRole('Admin')) {
        toast.info('New request submitted', {
          description: `${suffix} has been created`,
          duration: 4000,
        })
      } else if (hasRole('Manager')) {
        toast.info('New request from your team', {
          description: `${suffix} needs your attention`,
          duration: 4000,
        })
      } else if (hasRole('Employee')) {
        toast.success('Request created successfully', {
          description: reqId
            ? `Your request #${reqId} has been submitted`
            : 'Your request has been submitted',
          duration: 4000,
        })
      }
    },
    [dispatch, roles],
  )

  const handleStatusUpdated = useCallback(
    (payload) => {
      console.info('[SignalR] RequestStatusUpdated:', payload)
      dispatch(fetchRequests())

      const action = payload.action ?? payload.Action ?? payload.newStatus ?? payload.NewStatus ?? ''
      const reqId  = payload.requestNumber ?? payload.RequestNumber ?? payload.requestId ?? payload.RequestId
      const suffix = reqId ? `Request #${reqId}` : 'A request'

      const messages = {
        Approved:   { type: 'success', title: 'Request approved',
                      desc: `${suffix} has been approved` },
        Rejected:   { type: 'error',   title: 'Request rejected',
                      desc: `${suffix} has been rejected` },
        InProgress: { type: 'info',    title: 'Work started',
                      desc: `${suffix} is now in progress` },
        Resolved:   { type: 'success', title: 'Request resolved',
                      desc: `${suffix} has been resolved` },
        Escalated:  { type: 'warning', title: 'Request escalated',
                      desc: `${suffix} has been marked urgent` },
      }

      const msg = messages[action]
      if (!msg) return

      const shouldShow = (
        hasRole('Admin') ||
        hasRole('Manager') ||
        (hasRole('Employee') && ['Approved','Rejected','InProgress','Resolved'].includes(action)) ||
        (hasRole('Support') && ['InProgress','Resolved','Escalated'].includes(action))
      )

      if (!shouldShow) return

      if (msg.type === 'success') toast.success(msg.title, { description: msg.desc, duration: 4000 })
      else if (msg.type === 'error')   toast.error(msg.title,   { description: msg.desc, duration: 5000 })
      else if (msg.type === 'warning') toast.warning(msg.title, { description: msg.desc, duration: 5000 })
      else toast.info(msg.title, { description: msg.desc, duration: 4000 })
    },
    [dispatch, roles],
  )

  const handleAssigned = useCallback(
    (payload) => {
      console.info('[SignalR] RequestAssigned:', payload)
      dispatch(fetchRequests())

      const reqId   = payload.requestNumber ?? payload.RequestNumber ?? payload.requestId ?? payload.RequestId
      const suffix  = reqId ? `Request #${reqId}` : 'A request'
      const assignee = payload.assignedToName ?? payload.AssignedToName ?? 'support'

      if (hasRole('Support')) {
        toast.info('New request assigned to you', {
          description: `${suffix} has been assigned to your queue`,
          duration: 5000,
        })
      } else if (hasRole('Admin')) {
        toast.success('Request assigned', {
          description: `${suffix} assigned to ${assignee}`,
          duration: 4000,
        })
      } else if (hasRole('Employee')) {
        toast.info('Your request is being handled', {
          description: reqId
            ? `Request #${reqId} has been assigned to the support team`
            : 'Your request has been assigned to the support team',
          duration: 4000,
        })
      } else if (hasRole('Manager')) {
        toast.info('Team request assigned', {
          description: `${suffix} assigned to ${assignee}`,
          duration: 4000,
        })
      }
    },
    [dispatch, roles],
  )

  useEffect(() => {
    // Only connect for authenticated users
    if (!user) return

    let cancelled = false

    async function connect() {
      await startSignalR()

      if (cancelled) return

      // Register all three handlers and store refs for clean unsubscription
      createdHandlerRef.current = handleRequestCreated
      statusHandlerRef.current = handleStatusUpdated
      assignedHandlerRef.current = handleAssigned

      onRequestEvent(createdHandlerRef.current)
      onStatusUpdatedEvent(statusHandlerRef.current)
      onAssignedEvent(assignedHandlerRef.current)
    }

    connect()

    return () => {
      cancelled = true

      // Unsubscribe all three handlers on unmount
      if (createdHandlerRef.current) {
        offRequestEvent(createdHandlerRef.current)
        createdHandlerRef.current = null
      }
      if (statusHandlerRef.current) {
        offStatusUpdatedEvent(statusHandlerRef.current)
        statusHandlerRef.current = null
      }
      if (assignedHandlerRef.current) {
        offAssignedEvent(assignedHandlerRef.current)
        assignedHandlerRef.current = null
      }

      // We intentionally do NOT call stopSignalR() on page unmount —
      // the connection stays alive if the user navigates away and comes back.
      // Call stopSignalR() explicitly on logout if needed.
    }
  }, [user, handleRequestCreated, handleStatusUpdated, handleAssigned])
}
