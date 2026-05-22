import axios from "axios";
import { mapAuthResponse } from "../features/auth/authMappers.js";
import { API_BASE_URL } from "../config/env.js";
import { authTokenBridge } from "./authTokenBridge.js";

const REFRESH_PATH = "/auth/refresh";
const ME_PATH = "/auth/me";
const LOGOUT_PATH = "/auth/logout";

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true,
  headers: {
    "Content-Type": "application/json",
  },
});

let isRefreshing = false;
let failedQueue = [];

function processQueue(error, token = null) {
  failedQueue.forEach((p) => {
    if (error) {
      p.reject(error);
    } else {
      p.resolve(token);
    }
  });
  failedQueue = [];
}

function isRefreshRequest(config) {
  const url = config?.url ?? "";
  return url.replace(/^\//, "") === REFRESH_PATH.replace(/^\//, "");
}

function isLogoutRequest(config) {
  const url = config?.url ?? "";
  return url.replace(/^\//, "") === LOGOUT_PATH.replace(/^\//, "");
}

function isAuthLoginRequest(config) {
  const u = (config?.url ?? "").toLowerCase();
  return u.includes("/login");
}

function redirectToLogin() {
  if (typeof window === "undefined") return;
  if (window.location.pathname !== "/login") {
    window.location.assign("/login");
  }
}

async function fetchCurrentUserAfterRefresh() {
  const { data } = await apiClient.get(ME_PATH, { _skipAuthRefresh: true });
  return mapAuthResponse(data).user;
}

apiClient.interceptors.request.use((config) => {
  const token = authTokenBridge.getAccessToken();
  if (token && !isRefreshRequest(config) && !isAuthLoginRequest(config)) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (res) => res,
  async (error) => {
    const originalRequest = error.config;
    const status = error.response?.status;

    if (
      status !== 401 ||
      originalRequest._retry ||
      originalRequest._skipAuthRefresh ||
      isRefreshRequest(originalRequest) ||
      isLogoutRequest(originalRequest) ||
      isAuthLoginRequest(originalRequest)
    ) {
      return Promise.reject(error);
    }

    originalRequest._retry = true;

    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        failedQueue.push({ resolve, reject });
      })
        .then(() => {
          return apiClient(originalRequest);
        })
        .catch((err) => Promise.reject(err));
    }

    isRefreshing = true;

    try {
      await apiClient.post(REFRESH_PATH, undefined, { _skipAuthRefresh: true });
      const user = await fetchCurrentUserAfterRefresh();

      authTokenBridge.setTokens({ user });

      processQueue(null);
      return apiClient(originalRequest);
    } catch (refreshErr) {
      processQueue(refreshErr, null);
      authTokenBridge.clearAuth();
      redirectToLogin();
      return Promise.reject(refreshErr);
    } finally {
      isRefreshing = false;
    }
  },
);
