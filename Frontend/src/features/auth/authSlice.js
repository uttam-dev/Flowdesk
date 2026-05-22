import {
  createAsyncThunk,
  createSelector,
  createSlice,
} from "@reduxjs/toolkit";
import { getCurrentUserRequest, loginRequest } from "./authApi.js";

const initialState = {
  user: null,
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

export const restoreSession = createAsyncThunk(
  "auth/restoreSession",
  async (_, { rejectWithValue }) => {
    try {
      return await getCurrentUserRequest();
    } catch {
      return rejectWithValue(null);
    }
  },
);

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    setCredentials(state, action) {
      const { user } = action.payload;
      if (user !== undefined) state.user = user;
    },
    clearUser(state) {
      state.user = null;
    },
    logout(state) {
      state.user = null;
      state.error = null;
      state.status = "idle";
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
        state.user = action.payload.user;
      })
      .addCase(login.rejected, (state, action) => {
        state.status = "failed";
        state.error = action.payload ?? "Login failed";
      })
      .addCase(restoreSession.fulfilled, (state, action) => {
        state.status = "succeeded";
        state.user = action.payload;
        state.error = null;
      })
      .addCase(restoreSession.rejected, (state) => {
        state.status = "idle";
        state.user = null;
        state.error = null;
      });
  },
});

export const { setCredentials, clearUser, logout, clearError } = authSlice.actions;
export default authSlice.reducer;

export const selectAuth = (state) => state.auth;
export const selectIsAuthenticated = (state) => Boolean(state.auth.user);
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
