import axios from 'axios'
import { mapAuthResponse } from '../features/auth/authMappers.js'
import { API_BASE_URL } from '../config/env.js'
import { authTokenBridge } from './authTokenBridge.js'

const REFRESH_PATH = '/refresh'

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

let isRefreshing = false
let failedQueue = []

function processQueue(error, token = null) {
  failedQueue.forEach((p) => {
    if (error) {
      p.reject(error)
    } else {
      p.resolve(token)
    }
  })
  failedQueue = []
}

function isRefreshRequest(config) {
  const url = config?.url ?? ''
  return url.replace(/^\//, '') === REFRESH_PATH.replace(/^\//, '')
}

function isAuthLoginRequest(config) {
  const u = (config?.url ?? '').toLowerCase()
  return u.includes('/login')
}

apiClient.interceptors.request.use((config) => {
  const token = authTokenBridge.getAccessToken()
  if (token && !isRefreshRequest(config) && !isAuthLoginRequest(config)) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

apiClient.interceptors.response.use(
  (res) => res,
  async (error) => {
    const originalRequest = error.config
    const status = error.response?.status

    if (
      status !== 401 ||
      originalRequest._retry ||
      isRefreshRequest(originalRequest) ||
      isAuthLoginRequest(originalRequest)
    ) {
      return Promise.reject(error)
    }

    const refreshToken = authTokenBridge.getRefreshToken()
    if (!refreshToken) {
      authTokenBridge.clearAuth()
      return Promise.reject(error)
    }

    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        failedQueue.push({ resolve, reject })
      })
        .then((token) => {
          originalRequest.headers.Authorization = `Bearer ${token}`
          return apiClient(originalRequest)
        })
        .catch((err) => Promise.reject(err))
    }

    originalRequest._retry = true
    isRefreshing = true

    try {
      const { data } = await axios.post(
        `${API_BASE_URL}${REFRESH_PATH}`,
        { refreshToken },
        { headers: { 'Content-Type': 'application/json' } },
      )

      const mapped = mapAuthResponse(data)

      const accessToken = mapped.accessToken
      const nextRefresh = mapped.refreshToken ?? refreshToken

      if (!accessToken) {
        throw new Error('Refresh response missing access token')
      }

      const payload = {
        accessToken,
        refreshToken: nextRefresh,
      }
      if (mapped.user) {
        payload.user = mapped.user
      }

      authTokenBridge.setTokens(payload)

      processQueue(null, accessToken)
      originalRequest.headers.Authorization = `Bearer ${accessToken}`
      return apiClient(originalRequest)
    } catch (refreshErr) {
      processQueue(refreshErr, null)
      authTokenBridge.clearAuth()
      return Promise.reject(refreshErr)
    } finally {
      isRefreshing = false
    }
  },
)
