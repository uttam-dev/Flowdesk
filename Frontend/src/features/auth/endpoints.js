/** Default login path; override with VITE_AUTH_LOGIN_PATH if your API differs. */
export const AUTH_ENDPOINTS = {
  login: import.meta.env.VITE_AUTH_LOGIN_PATH ?? '/auth/login',
  me: '/auth/me',
  logout: '/auth/logout',
}
