/**
 * signalrService.js
 *
 * Singleton SignalR service for real-time request notifications.
 *
 * Design decisions:
 *  - Module-level singleton: one connection per browser tab, no matter how
 *    many components import this file.
 *  - Cookie-based auth (HTTP-only): the browser sends the auth cookie
 *    automatically on the WebSocket upgrade request. No manual token needed.
 *  - withAutomaticReconnect: backs off automatically on transient disconnects.
 *  - Callers register/remove named handlers via the typed helpers below.
 *
 * Events from server:
 *  - "RequestUpdated"       → request was created (legacy event name kept)
 *  - "RequestStatusUpdated" → approve / reject / in-progress / resolved / closed
 *  - "RequestAssigned"      → request was assigned to a support user
 */

import * as signalR from '@microsoft/signalr'
import { API_BASE_URL } from '../config/env.js'

// Derive the hub URL from the API base URL.
// API_BASE_URL  = "https://localhost:7161/api"  →  hubUrl = "https://localhost:7161/hubs/request"
const HUB_URL = API_BASE_URL.replace(/\/api\/?$/, '') + '/hubs/request'

/** @type {signalR.HubConnection | null} */
let _connection = null

/** @type {boolean} */
let _starting = false

/**
 * Retry policy: attempt immediately then at 2s, 10s, 30s intervals.
 * After the four configured retries, SignalR stops and `onclose` fires.
 */
const RETRY_DELAYS = [0, 2_000, 10_000, 30_000]

function buildConnection() {
  return new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, {
      // Browser sends HTTP-only auth cookie automatically.
      withCredentials: true,
      // Prefer WebSockets, fall back to LongPolling.
      transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
    })
    .withAutomaticReconnect(RETRY_DELAYS)
    .configureLogging(
      import.meta.env.DEV
        ? signalR.LogLevel.Information
        : signalR.LogLevel.Warning
    )
    .build()
}

/**
 * Start the SignalR connection (idempotent – safe to call multiple times).
 * @returns {Promise<void>}
 */
export async function startSignalR() {
  if (_connection && _connection.state === signalR.HubConnectionState.Connected) {
    return
  }

  if (_starting) return
  _starting = true

  try {
    if (!_connection) {
      _connection = buildConnection()

      _connection.onreconnecting((err) => {
        console.info('[SignalR] Reconnecting…', err?.message ?? '')
      })

      _connection.onreconnected((connectionId) => {
        console.info('[SignalR] Reconnected. ConnectionId:', connectionId)
      })

      _connection.onclose((err) => {
        console.warn('[SignalR] Connection closed.', err?.message ?? '')
      })
    }

    if (_connection.state === signalR.HubConnectionState.Disconnected) {
      await _connection.start()
      console.info('[SignalR] Connected. State:', _connection.state)
    }
  } catch (err) {
    console.error('[SignalR] Failed to connect:', err)
    // Reset so the next call can try again
    _connection = null
  } finally {
    _starting = false
  }
}

/**
 * Stop and destroy the SignalR connection (called on logout / unmount).
 * @returns {Promise<void>}
 */
export async function stopSignalR() {
  if (!_connection) return
  try {
    await _connection.stop()
    console.info('[SignalR] Stopped.')
  } catch (err) {
    console.warn('[SignalR] Error while stopping:', err)
  } finally {
    _connection = null
    _starting = false
  }
}

// ── Event helpers ─────────────────────────────────────────────────────────────
// Each event has a pair: on* (subscribe) and off* (unsubscribe).
// Always pass the same function reference to on* and off* — they use identity.
// ─────────────────────────────────────────────────────────────────────────────

/**
 * Subscribe to "RequestUpdated" — fired when a request is created.
 * Payload: { requestId, action, createdAt }
 * @param {(payload: object) => void} handler
 */
export function onRequestEvent(handler) {
  _connection?.on('RequestUpdated', handler)
}
export function offRequestEvent(handler) {
  _connection?.off('RequestUpdated', handler)
}

/**
 * Subscribe to "RequestStatusUpdated" — fired after approve/reject/in-progress/resolved.
 * Payload: { requestId, newStatus, updatedById, updatedAt }
 * @param {(payload: object) => void} handler
 */
export function onStatusUpdatedEvent(handler) {
  _connection?.on('RequestStatusUpdated', handler)
}
export function offStatusUpdatedEvent(handler) {
  _connection?.off('RequestStatusUpdated', handler)
}

/**
 * Subscribe to "RequestAssigned" — fired when a request is assigned to a support user.
 * Payload: { requestId, assignedToId, assignedToName, assignedAt }
 * @param {(payload: object) => void} handler
 */
export function onAssignedEvent(handler) {
  _connection?.on('RequestAssigned', handler)
}
export function offAssignedEvent(handler) {
  _connection?.off('RequestAssigned', handler)
}

/**
 * Expose the raw connection for advanced use (e.g., checking state).
 * @returns {signalR.HubConnection | null}
 */
export function getConnection() {
  return _connection
}
