import {
  createAsyncThunk,
  createSelector,
  createSlice,
} from "@reduxjs/toolkit";
import { loginRequest } from "./authApi.js";
import { readPersistedAuth, writePersistedAuth } from "./authStorage.js";

const persisted = readPersistedAuth();

const initialState = {
  accessToken: persisted?.accessToken ?? null,
  refreshToken: persisted?.refreshToken ?? null,
  user: persisted?.user ?? null,
  status: "idle",
  error: null,
};

function pickErrorMessage(err) {
  const data = err?.response?.data;
  if (typeof data === "string") return data;
  if (data?.message) return String(data.message);
  if (data?.title) return String(data.title);
  if (Array.isArray(data?.errors) && data.errors.length) {
    return data.errors
      .map((e) => (typeof e === "string" ? e : e?.message))
      .join(", ");
  }
  if (err?.message) return String(err.message);
  return "Something went wrong";
}

export const login = createAsyncThunk(
  "auth/login",
  async ({ email, password }, { rejectWithValue }) => {
    try {
      return await loginRequest({ email, password });
    } catch (err) {
      return rejectWithValue(pickErrorMessage(err));
    }
  },
);

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    setCredentials(state, action) {
      const { accessToken, refreshToken, user } = action.payload;
      state.accessToken = accessToken;
      if (refreshToken !== undefined) state.refreshToken = refreshToken;
      if (user !== undefined) state.user = user;
      writePersistedAuth(state);
    },
    logout(state) {
      state.accessToken = null;
      state.refreshToken = null;
      state.user = null;
      state.error = null;
      state.status = "idle";
      writePersistedAuth(state);
    },
    clearError(state) {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(login.pending, (state) => {
        state.status = "loading";
        state.error = null;
      })
      .addCase(login.fulfilled, (state, action) => {
        state.status = "succeeded";
        state.accessToken = action.payload.accessToken;
        state.refreshToken = action.payload.refreshToken;
        state.user = action.payload.user;
        writePersistedAuth(state);
      })
      .addCase(login.rejected, (state, action) => {
        state.status = "failed";
        state.error = action.payload ?? "Login failed";
      });
  },
});

export const { setCredentials, logout, clearError } = authSlice.actions;
export default authSlice.reducer;

export const selectAuth = (state) => state.auth;
export const selectIsAuthenticated = (state) => Boolean(state.auth.accessToken);
export const selectAuthStatus = (state) => state.auth.status;
export const selectAuthUser = (state) => state.auth.user;

/** @param {{ auth: object }} state */
const selectAuthUserRaw = (state) => state.auth.user;

export const selectRoleNames = createSelector(selectAuthUserRaw, (user) => {
  if (!user) return [];
  const roles = user.roles ?? user.role;
  if (Array.isArray(roles)) return roles.map(String);
  if (typeof roles === "string" && roles.trim()) return [roles];
  return [];
});

/** @param {{ auth: object }} state */
export const selectPermissionNames = createSelector(
  selectAuthUserRaw,
  (user) => {
    if (!user) return [];
    const perms = user.permissions ?? user.permission;
    if (Array.isArray(perms)) return perms.map(String);
    if (typeof perms === "string" && perms.trim()) return [perms];
    return [];
  },
);

/**
 * RBAC: user must have at least one of the given roles (case-insensitive). Empty `required` means any authenticated user.
 * @param {{ auth: object }} state
 * @param {string[]|undefined} requiredRoleNames
 */
export function selectHasAnyRole(state, requiredRoleNames) {
  if (!requiredRoleNames?.length) return true;
  const mine = new Set(selectRoleNames(state).map((r) => r.toLowerCase()));
  return requiredRoleNames.some((r) => mine.has(String(r).toLowerCase()));
}

/**
 * @param {{ auth: object }} state
 * @param {string[]|undefined} requiredPermissions
 */
export function selectHasAnyPermission(state, requiredPermissions) {
  if (!requiredPermissions?.length) return true;
  const mine = new Set(
    selectPermissionNames(state).map((p) => p.toLowerCase()),
  );
  return requiredPermissions.some((p) => mine.has(String(p).toLowerCase()));
}
