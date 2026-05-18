import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'
import {
  approveRequestApi,
  assignRequestApi,
  createRequestApi,
  fetchRequestByIdApi,
  fetchRequestCommentsApi,
  fetchRequestsApi,
  parseApiError,
  rejectRequestApi,
  updateRequestStatusApi,
} from './requestApi.js'

const initialState = {
  items: [],
  detail: null,
  comments: [],
  loading: 'idle',
  detailLoading: 'idle',
  commentsLoading: 'idle',
  error: null,
  detailError: null,
  page: 1,
  pageSize: 10,
  total: 0,
  totalPages: 1,
  status: '',
  priority: '',
  categoryId: '',
  requestNumber: '',
  activeTab: 'all',
  mutationLoading: false,
}

function listParams(state) {
  return {
    page: state.page,
    pageSize: state.pageSize,
    status: state.status,
    priority: state.priority,
    categoryId: state.categoryId,
    requestNumber: state.requestNumber,
  }
}

export const fetchRequests = createAsyncThunk(
  'requests/fetchList',
  async (params, { getState, rejectWithValue }) => {
    const s = getState().requests
    const p = params ?? listParams(s)
    try {
      return await fetchRequestsApi(p)
    } catch (err) {
      return rejectWithValue(parseApiError(err))
    }
  },
)

export const fetchRequestDetail = createAsyncThunk(
  'requests/fetchDetail',
  async (id, { rejectWithValue }) => {
    try {
      return await fetchRequestByIdApi(id)
    } catch (err) {
      return rejectWithValue(parseApiError(err))
    }
  },
)

export const fetchRequestComments = createAsyncThunk(
  'requests/fetchComments',
  async (id, { rejectWithValue }) => {
    try {
      return await fetchRequestCommentsApi(id)
    } catch (err) {
      return rejectWithValue(parseApiError(err))
    }
  },
)

export const createRequest = createAsyncThunk(
  'requests/create',
  async (payload, { rejectWithValue }) => {
    try {
      return await createRequestApi(payload)
    } catch (err) {
      return rejectWithValue(parseApiError(err))
    }
  },
)

export const approveRequest = createAsyncThunk(
  'requests/approve',
  async ({ id, ...body }, { rejectWithValue }) => {
    try {
      return await approveRequestApi(id, body)
    } catch (err) {
      return rejectWithValue(parseApiError(err))
    }
  },
)

export const rejectRequest = createAsyncThunk(
  'requests/reject',
  async ({ id, ...body }, { rejectWithValue }) => {
    try {
      return await rejectRequestApi(id, body)
    } catch (err) {
      return rejectWithValue(parseApiError(err))
    }
  },
)

export const assignRequest = createAsyncThunk(
  'requests/assign',
  async ({ id, ...body }, { rejectWithValue }) => {
    try {
      return await assignRequestApi(id, body)
    } catch (err) {
      return rejectWithValue(parseApiError(err))
    }
  },
)

export const updateRequestStatus = createAsyncThunk(
  'requests/updateStatus',
  async ({ id, ...body }, { rejectWithValue }) => {
    try {
      return await updateRequestStatusApi(id, body)
    } catch (err) {
      return rejectWithValue(parseApiError(err))
    }
  },
)

const requestSlice = createSlice({
  name: 'requests',
  initialState,
  reducers: {
    setPage(state, action) {
      state.page = action.payload
    },
    setPageSize(state, action) {
      state.pageSize = action.payload
      state.page = 1
    },
    setStatusFilter(state, action) {
      state.status = action.payload
      state.page = 1
    },
    setPriorityFilter(state, action) {
      state.priority = action.payload
      state.page = 1
    },
    setCategoryFilter(state, action) {
      state.categoryId = action.payload
      state.page = 1
    },
    setRequestNumberFilter(state, action) {
      state.requestNumber = action.payload
      state.page = 1
    },
    setActiveTab(state, action) {
      state.activeTab = action.payload.key
      state.status = action.payload.status
      state.page = 1
    },
    clearRequestError(state) {
      state.error = null
    },
    clearRequestDetail(state) {
      state.detail = null
      state.comments = []
      state.detailError = null
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchRequests.pending, (state) => {
        state.loading = 'pending'
        state.error = null
      })
      .addCase(fetchRequests.fulfilled, (state, action) => {
        state.loading = 'succeeded'
        state.items = action.payload.items
        state.total = action.payload.total
        state.page = action.payload.page
        state.pageSize = action.payload.pageSize
        state.totalPages = action.payload.totalPages
      })
      .addCase(fetchRequests.rejected, (state, action) => {
        state.loading = 'failed'
        state.error = action.payload ?? 'Failed to load requests'
      })
      .addCase(fetchRequestDetail.pending, (state) => {
        state.detailLoading = 'pending'
        state.detailError = null
      })
      .addCase(fetchRequestDetail.fulfilled, (state, action) => {
        state.detailLoading = 'succeeded'
        state.detail = action.payload
      })
      .addCase(fetchRequestDetail.rejected, (state, action) => {
        state.detailLoading = 'failed'
        state.detailError = action.payload ?? 'Failed to load request'
      })
      .addCase(fetchRequestComments.pending, (state) => {
        state.commentsLoading = 'pending'
      })
      .addCase(fetchRequestComments.fulfilled, (state, action) => {
        state.commentsLoading = 'succeeded'
        state.comments = action.payload
      })
      .addCase(fetchRequestComments.rejected, (state) => {
        state.commentsLoading = 'failed'
        state.comments = []
      })
      .addMatcher(
        (action) =>
          [
            createRequest.pending.type,
            approveRequest.pending.type,
            rejectRequest.pending.type,
            assignRequest.pending.type,
            updateRequestStatus.pending.type,
          ].includes(action.type),
        (state) => {
          state.mutationLoading = true
        },
      )
      .addMatcher(
        (action) =>
          [
            createRequest.fulfilled.type,
            createRequest.rejected.type,
            approveRequest.fulfilled.type,
            approveRequest.rejected.type,
            rejectRequest.fulfilled.type,
            rejectRequest.rejected.type,
            assignRequest.fulfilled.type,
            assignRequest.rejected.type,
            updateRequestStatus.fulfilled.type,
            updateRequestStatus.rejected.type,
          ].includes(action.type),
        (state) => {
          state.mutationLoading = false
        },
      )
  },
})

export const {
  setPage,
  setPageSize,
  setStatusFilter,
  setPriorityFilter,
  setCategoryFilter,
  setRequestNumberFilter,
  setActiveTab,
  clearRequestError,
  clearRequestDetail,
} = requestSlice.actions
export default requestSlice.reducer

export const selectRequestList = (state) => state.requests.items
export const selectRequestLoading = (state) => state.requests.loading
export const selectRequestError = (state) => state.requests.error
export const selectRequestPage = (state) => state.requests.page
export const selectRequestPageSize = (state) => state.requests.pageSize
export const selectRequestTotal = (state) => state.requests.total
export const selectRequestTotalPages = (state) => state.requests.totalPages
export const selectRequestStatusFilter = (state) => state.requests.status
export const selectRequestPriorityFilter = (state) => state.requests.priority
export const selectRequestCategoryFilter = (state) => state.requests.categoryId
export const selectRequestNumberFilter = (state) => state.requests.requestNumber
export const selectRequestActiveTab = (state) => state.requests.activeTab
export const selectRequestMutationLoading = (state) => state.requests.mutationLoading
export const selectRequestDetail = (state) => state.requests.detail
export const selectRequestDetailLoading = (state) => state.requests.detailLoading
export const selectRequestDetailError = (state) => state.requests.detailError
export const selectRequestComments = (state) => state.requests.comments
export const selectRequestCommentsLoading = (state) => state.requests.commentsLoading
