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
  const mapped = mapAuthResponse(data)
  if (!mapped.accessToken) {
    const err = new Error('Login response did not include an access token')
    // @ts-ignore
    err.response = { data }
    throw err
  }
  return mapped
}
