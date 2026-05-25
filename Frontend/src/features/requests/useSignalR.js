/**
 * useSignalR.js
 *
 * React hook that manages the SignalR connection lifecycle for the request list page.
 *
 * Subscribes to three server events:
 *  ┌──────────────────────────┬────────────────────────────────────────────────┐
 *  │ Event                    │ Sent to (server-side groups)                   │
 *  ├──────────────────────────┼────────────────────────────────────────────────┤
 *  │ RequestUpdated           │ role-Admin, role-Manager, user-{managerId},    │
 *  │ (request created)        │ user-{employeeId}                              │
 *  ├──────────────────────────┼────────────────────────────────────────────────┤
 *  │ RequestStatusUpdated     │ role-Admin, user-{employeeId},                 │
 *  │ (approve/reject/status)  │ user-{assignedToId}, user-{managerId}          │
 *  ├──────────────────────────┼────────────────────────────────────────────────┤
 *  │ RequestAssigned          │ role-Support, user-{assignedToId}, role-Admin  │
 *  └──────────────────────────┴────────────────────────────────────────────────┘
 *
 * The server already targets the correct groups, so ALL authenticated users
 * subscribe on the frontend — server-side groups control who actually receives
 * each message. No role-filtering needed on the client.
 *
 * On any event: dispatch fetchRequests() (safe refetch, picks up current
 * filters/page/tab from Redux state). No optimistic patching.
 */

import { useCallback, useEffect, useRef } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { selectAuthUser } from '../auth/authSlice.js'
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
    },
    [dispatch],
  )

  const handleStatusUpdated = useCallback(
    (payload) => {
      console.info('[SignalR] RequestStatusUpdated:', payload)
      dispatch(fetchRequests())
    },
    [dispatch],
  )

  const handleAssigned = useCallback(
    (payload) => {
      console.info('[SignalR] RequestAssigned:', payload)
      dispatch(fetchRequests())
    },
    [dispatch],
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
