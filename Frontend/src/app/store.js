import { configureStore } from '@reduxjs/toolkit'
import authReducer, { setCredentials, logout } from '../features/auth/authSlice.js'
import categoryReducer from '../features/categories/categorySlice.js'
import userReducer from '../features/users/userSlice.js'
import { attachAuthTokenBridge } from '../services/authTokenBridge.js'

export const store = configureStore({
  reducer: {
    auth: authReducer,
    categories: categoryReducer,
    users: userReducer,
  },
})

attachAuthTokenBridge(store, { setCredentials, logout })
