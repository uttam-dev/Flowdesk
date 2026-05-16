/**
 * Breaks circular imports between the axios apiClient and the Redux store.
 * Call `attachAuthTokenBridge(store, actions)` once after the store is created.
 */
let bridge = {
  getAccessToken: () => null,
  getRefreshToken: () => null,
  setTokens: () => {},
  clearAuth: () => {},
}

export const authTokenBridge = {
  getAccessToken: () => bridge.getAccessToken(),
  getRefreshToken: () => bridge.getRefreshToken(),
  setTokens: (payload) => bridge.setTokens(payload),
  clearAuth: () => bridge.clearAuth(),
}

export function attachAuthTokenBridge(store, actions) {
  const { setCredentials, logout } = actions
  bridge = {
    getAccessToken: () => store.getState().auth.accessToken,
    getRefreshToken: () => store.getState().auth.refreshToken,
    setTokens: (payload) => {
      store.dispatch(setCredentials(payload))
    },
    clearAuth: () => {
      store.dispatch(logout())
    },
  }
}
