import { createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import {
  activateUserApi,
  createUserApi,
  deactivateUserApi,
  deleteUserApi,
  fetchUsersApi,
  parseApiError,
  updateUserApi,
} from "./userApi.js";

const initialState = {
  items: [],
  loading: "idle",
  error: null,
  page: 1,
  pageSize: 10,
  total: 0,
  totalPages: 1,
  isActive: "",
  role: "",
  email: "",
  mutationLoading: false,
};

export const fetchUsers = createAsyncThunk(
  "users/fetchList",
  async ({ page, pageSize, isActive, role, email }, { rejectWithValue }) => {
    try {
      return await fetchUsersApi({ page, pageSize, isActive, role, email });
    } catch (err) {
      return rejectWithValue(parseApiError(err));
    }
  },
);

export const createUser = createAsyncThunk(
  "users/create",
  async (payload, { rejectWithValue }) => {
    try {
      return await createUserApi(payload);
    } catch (err) {
      return rejectWithValue(parseApiError(err));
    }
  },
);

export const updateUser = createAsyncThunk(
  "users/update",
  async ({ id, ...body }, { rejectWithValue }) => {
    try {
      return await updateUserApi(id, body);
    } catch (err) {
      return rejectWithValue(parseApiError(err));
    }
  },
);

export const deleteUser = createAsyncThunk(
  "users/delete",
  async (id, { rejectWithValue }) => {
    try {
      return await deleteUserApi(id);
    } catch (err) {
      return rejectWithValue(parseApiError(err));
    }
  },
);

export const activateUser = createAsyncThunk(
  "users/activate",
  async (id, { rejectWithValue }) => {
    try {
      return await activateUserApi(id);
    } catch (err) {
      return rejectWithValue(parseApiError(err));
    }
  },
);

export const deactivateUser = createAsyncThunk(
  "users/deactivate",
  async (id, { rejectWithValue }) => {
    try {
      return await deactivateUserApi(id);
    } catch (err) {
      return rejectWithValue(parseApiError(err));
    }
  },
);

const userSlice = createSlice({
  name: "users",
  initialState,
  reducers: {
    setPage(state, action) {
      state.page = action.payload;
    },
    setPageSize(state, action) {
      state.pageSize = action.payload;
      state.page = 1;
    },
    setIsActiveFilter(state, action) {
      state.isActive = action.payload;
      state.page = 1;
    },
    setRoleFilter(state, action) {
      state.role = action.payload;
      state.page = 1;
    },
    setEmailFilter(state, action) {
      state.email = action.payload;
      state.page = 1;
    },
    clearUserError(state) {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchUsers.pending, (state) => {
        state.loading = "pending";
        state.error = null;
      })
      .addCase(fetchUsers.fulfilled, (state, action) => {
        state.loading = "succeeded";
        state.items = action.payload.items;
        state.total = action.payload.total;
        state.page = action.payload.page;
        state.pageSize = action.payload.pageSize;
        state.totalPages = action.payload.totalPages;
      })
      .addCase(fetchUsers.rejected, (state, action) => {
        state.loading = "failed";
        state.error = action.payload ?? "Failed to load users";
      })
      .addCase(createUser.pending, (state) => {
        state.mutationLoading = true;
        state.error = null;
      })
      .addCase(createUser.fulfilled, (state) => {
        state.mutationLoading = false;
      })
      .addCase(createUser.rejected, (state, action) => {
        state.mutationLoading = false;
        state.error = action.payload ?? "Create failed";
      })
      .addCase(updateUser.pending, (state) => {
        state.mutationLoading = true;
        state.error = null;
      })
      .addCase(updateUser.fulfilled, (state) => {
        state.mutationLoading = false;
      })
      .addCase(updateUser.rejected, (state, action) => {
        state.mutationLoading = false;
        state.error = action.payload ?? "Update failed";
      })
      .addCase(deleteUser.fulfilled, (state) => {
        state.mutationLoading = false;
      })
      .addCase(activateUser.fulfilled, (state) => {
        state.mutationLoading = false;
      })
      .addCase(deactivateUser.fulfilled, (state) => {
        state.mutationLoading = false;
      });
  },
});

export const {
  setPage,
  setPageSize,
  setIsActiveFilter,
  setRoleFilter,
  setEmailFilter,
  clearUserError,
} = userSlice.actions;
export default userSlice.reducer;

export const selectUserList = (state) => state.users.items;
export const selectUserLoading = (state) => state.users.loading;
export const selectUserError = (state) => state.users.error;
export const selectUserPage = (state) => state.users.page;
export const selectUserPageSize = (state) => state.users.pageSize;
export const selectUserTotal = (state) => state.users.total;
export const selectUserTotalPages = (state) => state.users.totalPages;
export const selectUserIsActive = (state) => state.users.isActive;
export const selectUserRole = (state) => state.users.role;
export const selectUserEmail = (state) => state.users.email;
export const selectUserMutationLoading = (state) => state.users.mutationLoading;
