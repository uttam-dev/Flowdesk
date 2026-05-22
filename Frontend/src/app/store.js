import { configureStore, createListenerMiddleware } from '@reduxjs/toolkit'
import authReducer, { clearUser, setCredentials, logout } from '../features/auth/authSlice.js'
import categoryReducer from '../features/categories/categorySlice.js'
import userReducer from '../features/users/userSlice.js'
import requestReducer from '../features/requests/requestSlice.js'
import { attachAuthTokenBridge } from '../services/authTokenBridge.js'
import { logoutRequest } from '../features/auth/authApi.js'

const authListenerMiddleware = createListenerMiddleware()

authListenerMiddleware.startListening({
  actionCreator: logout,
  effect: async () => {
    try {
      await logoutRequest()
    } catch {
      /* logged out locally */
    }
  },
})

export const store = configureStore({
  reducer: {
    auth: authReducer,
    categories: categoryReducer,
    users: userReducer,
    requests: requestReducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().prepend(authListenerMiddleware.middleware),
})

attachAuthTokenBridge(store, { setCredentials, clearUser })
