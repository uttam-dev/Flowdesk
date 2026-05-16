import { createAsyncThunk, createSlice } from '@reduxjs/toolkit'
import {
  activateCategoryApi,
  createCategoryApi,
  deactivateCategoryApi,
  fetchCategoriesApi,
  updateCategoryApi,
} from './categoryApi.js'

function pickError(err) {
  const d = err?.response?.data
  if (typeof d === 'string') return d
  if (d?.message) return String(d.message)
  if (d?.title) return String(d.title)
  if (err?.message) return String(err.message)
  return 'Request failed'
}

const initialState = {
  items: [],
  loading: 'idle',
  error: null,
  page: 1,
  pageSize: 10,
  total: 0,
  totalPages: 1,
  search: '',
  isActive: '',
  mutationLoading: false,
}

export const fetchCategories = createAsyncThunk(
  'categories/fetchList',
  async ({ page, pageSize, search, isActive }, { rejectWithValue }) => {
    try {
      return await fetchCategoriesApi({ page, pageSize, search, isActive })
    } catch (err) {
      return rejectWithValue(pickError(err))
    }
  },
)

export const createCategory = createAsyncThunk(
  'categories/create',
  async (payload, { rejectWithValue }) => {
    try {
      return await createCategoryApi(payload)
    } catch (err) {
      return rejectWithValue(pickError(err))
    }
  },
)

export const updateCategory = createAsyncThunk(
  'categories/update',
  async ({ id, ...body }, { rejectWithValue }) => {
    try {
      return await updateCategoryApi(id, body)
    } catch (err) {
      return rejectWithValue(pickError(err))
    }
  },
)

export const activateCategory = createAsyncThunk(
  'categories/activate',
  async (id, { rejectWithValue }) => {
    try {
      return await activateCategoryApi(id)
    } catch (err) {
      return rejectWithValue(pickError(err))
    }
  },
)

export const deactivateCategory = createAsyncThunk(
  'categories/deactivate',
  async (id, { rejectWithValue }) => {
    try {
      return await deactivateCategoryApi(id)
    } catch (err) {
      return rejectWithValue(pickError(err))
    }
  },
)

const categorySlice = createSlice({
  name: 'categories',
  initialState,
  reducers: {
    setPage(state, action) {
      state.page = action.payload
    },
    setPageSize(state, action) {
      state.pageSize = action.payload
      state.page = 1
    },
    setSearch(state, action) {
      state.search = action.payload
      state.page = 1
    },
    setIsActiveFilter(state, action) {
      state.isActive = action.payload
      state.page = 1
    },
    clearCategoryError(state) {
      state.error = null
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchCategories.pending, (state) => {
        state.loading = 'pending'
        state.error = null
      })
      .addCase(fetchCategories.fulfilled, (state, action) => {
        state.loading = 'succeeded'
        state.items = action.payload.items
        state.total = action.payload.total
        state.page = action.payload.page
        state.pageSize = action.payload.pageSize
        state.totalPages = action.payload.totalPages
      })
      .addCase(fetchCategories.rejected, (state, action) => {
        state.loading = 'failed'
        state.error = action.payload ?? 'Failed to load categories'
      })
      .addCase(createCategory.pending, (state) => {
        state.mutationLoading = true
        state.error = null
      })
      .addCase(createCategory.fulfilled, (state) => {
        state.mutationLoading = false
      })
      .addCase(createCategory.rejected, (state, action) => {
        state.mutationLoading = false
        state.error = action.payload ?? 'Create failed'
      })
      .addCase(updateCategory.pending, (state) => {
        state.mutationLoading = true
        state.error = null
      })
      .addCase(updateCategory.fulfilled, (state, action) => {
        state.mutationLoading = false
        const row = action.payload
        if (row?.categoryId != null) {
          const idx = state.items.findIndex((i) => i.categoryId === row.categoryId)
          if (idx >= 0) state.items[idx] = { ...state.items[idx], ...row }
        }
      })
      .addCase(updateCategory.rejected, (state, action) => {
        state.mutationLoading = false
        state.error = action.payload ?? 'Update failed'
      })
      .addCase(activateCategory.fulfilled, (state, action) => {
        const row = action.payload
        if (row?.categoryId != null) {
          const idx = state.items.findIndex((i) => i.categoryId === row.categoryId)
          if (idx >= 0)
            state.items[idx] = { ...state.items[idx], ...row, isActive: true }
        }
      })
      .addCase(deactivateCategory.fulfilled, (state, action) => {
        const row = action.payload
        if (row?.categoryId != null) {
          const idx = state.items.findIndex((i) => i.categoryId === row.categoryId)
          if (idx >= 0)
            state.items[idx] = { ...state.items[idx], ...row, isActive: false }
        }
      })
  },
})

export const {
  setPage,
  setPageSize,
  setSearch,
  setIsActiveFilter,
  clearCategoryError,
} = categorySlice.actions
export default categorySlice.reducer

export const selectCategoryList = (state) => state.categories.items
export const selectCategoryLoading = (state) => state.categories.loading
export const selectCategoryError = (state) => state.categories.error
export const selectCategoryPage = (state) => state.categories.page
export const selectCategoryPageSize = (state) => state.categories.pageSize
export const selectCategoryTotal = (state) => state.categories.total
export const selectCategoryTotalPages = (state) => state.categories.totalPages
export const selectCategorySearch = (state) => state.categories.search
export const selectCategoryIsActive = (state) => state.categories.isActive
export const selectCategoryMutationLoading = (state) =>
  state.categories.mutationLoading
