/**
 * Persists auth for SPA reloads:
 * - sessionStorage: access token + user profile (tab-scoped; cleared when tab closes).
 * - localStorage: refresh token only (survives reload; still XSS-sensitive like any SPA).
 *
 * Legacy key `flowdesk.auth` is migrated once on read.
 */
const LEGACY_KEY = 'flowdesk.auth'
const SESSION_KEY = 'flowdesk.session'
const REFRESH_KEY = 'flowdesk.refresh'

export function readPersistedAuth() {
  const migrated = tryMigrateLegacyAuth()
  if (migrated) return migrated

  let accessToken = null
  let user = null
  try {
    const raw = sessionStorage.getItem(SESSION_KEY)
    if (raw) {
      const p = JSON.parse(raw)
      if (p && typeof p === 'object') {
        accessToken = p.accessToken ?? null
        user = p.user ?? null
      }
    }
  } catch {
    /* ignore */
  }

  let refreshToken = null
  try {
    const raw = localStorage.getItem(REFRESH_KEY)
    if (raw) {
      const p = JSON.parse(raw)
      if (p && typeof p === 'object' && p.refreshToken != null) {
        refreshToken = String(p.refreshToken)
      }
    }
  } catch {
    /* ignore */
  }

  return { accessToken, refreshToken, user }
}

export function writePersistedAuth(state) {
  try {
    if (!state.accessToken) {
      sessionStorage.removeItem(SESSION_KEY)
      localStorage.removeItem(REFRESH_KEY)
      return
    }
    sessionStorage.setItem(
      SESSION_KEY,
      JSON.stringify({
        accessToken: state.accessToken,
        user: state.user ?? null,
      }),
    )
    if (state.refreshToken) {
      localStorage.setItem(
        REFRESH_KEY,
        JSON.stringify({ refreshToken: state.refreshToken }),
      )
    } else {
      localStorage.removeItem(REFRESH_KEY)
    }
  } catch {
    /* ignore quota / private mode */
  }
}

function tryMigrateLegacyAuth() {
  try {
    const raw = localStorage.getItem(LEGACY_KEY)
    if (!raw) return null
    const p = JSON.parse(raw)
    if (!p || typeof p !== 'object' || !p.accessToken) return null
    localStorage.removeItem(LEGACY_KEY)
    const next = {
      accessToken: p.accessToken,
      refreshToken: p.refreshToken ?? null,
      user: p.user ?? null,
    }
    writePersistedAuth({
      accessToken: next.accessToken,
      refreshToken: next.refreshToken,
      user: next.user,
    })
    return next
  } catch {
    return null
  }
}
