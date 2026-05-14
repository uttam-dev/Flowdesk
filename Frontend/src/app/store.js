import { configureStore } from '@reduxjs/toolkit'
import authReducer, { setCredentials, logout } from '../features/auth/authSlice.js'
import { attachAuthTokenBridge } from '../services/authTokenBridge.js'

export const store = configureStore({
  reducer: {
    auth: authReducer,
  },
})

attachAuthTokenBridge(store, { setCredentials, logout })
