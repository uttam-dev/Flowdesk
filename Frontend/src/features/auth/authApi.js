import { apiClient } from '../../services/apiClient.js'
import { AUTH_ENDPOINTS } from './endpoints.js'
import { mapAuthResponse } from './authMappers.js'

/**
 * @param {{ email: string, password: string }} credentials
 */
export async function loginRequest(credentials) {
  const { data } = await apiClient.post(AUTH_ENDPOINTS.login, {
    email: credentials.email,
    password: credentials.password,
  })
  return mapAuthResponse(data)
}

export async function getCurrentUserRequest() {
  const { data } = await apiClient.get(AUTH_ENDPOINTS.me)
  return mapAuthResponse(data).user
}

export async function logoutRequest() {
  await apiClient.post(AUTH_ENDPOINTS.logout)
}
