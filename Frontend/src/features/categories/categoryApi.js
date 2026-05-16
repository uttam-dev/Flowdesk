import { apiClient } from '../../services/apiClient.js'

const BASE = '/categories'

function mapCategoryRow(raw) {
  if (!raw || typeof raw !== 'object') return null
  return {
    categoryId: raw.categoryId ?? raw.CategoryId,
    categoryName: raw.categoryName ?? raw.CategoryName ?? '',
    isApprovalRequired: Boolean(
      raw.isApprovalRequired ?? raw.IsApprovalRequired,
    ),
    isActive: Boolean(raw.isActive ?? raw.IsActive),
    createdOn: raw.createdOn ?? raw.CreatedOn ?? null,
    updatedOn: raw.updatedOn ?? raw.UpdatedOn ?? null,
  }
}

function parseListPayload(data) {
  const root = data?.data !== undefined ? data.data : data
  let items = []
  let total = 0
  let pageNumber = 1
  let pageSize = 10
  let totalPages = 1

  if (Array.isArray(root)) {
    items = root.map(mapCategoryRow).filter(Boolean)
    total = items.length
    pageSize = items.length || pageSize
  } else if (root && typeof root === 'object') {
    const list =
      root.items ??
      root.Items ??
      root.data ??
      root.Data ??
      root.results ??
      root.Results
    if (Array.isArray(list)) {
      items = list.map(mapCategoryRow).filter(Boolean)
    }
    total =
      root.totalCount ??
      root.TotalCount ??
      root.total ??
      root.Total ??
      items.length
    pageNumber = root.pageNumber ?? root.PageNumber ?? pageNumber
    pageSize = root.pageSize ?? root.PageSize ?? pageSize
    totalPages =
      root.totalPages ??
      root.TotalPages ??
      Math.max(1, Math.ceil((total || 0) / (pageSize || 1)) || 1)
  }

  return { items, total, pageNumber, pageSize, totalPages }
}

/**
 * @param {{ pageNumber?: number, page?: number, pageSize?: number, search?: string, isActive?: boolean|string }} params
 */
export async function fetchCategoriesApi(params) {
  const pageNumber = params.pageNumber ?? params.page ?? 1
  const pageSize = params.pageSize ?? 10
  const search = params.search?.trim() || undefined
  const isActive = params.isActive

  const { data } = await apiClient.get(BASE, {
    params: {
      pageNumber,
      pageSize,
      IsActive: isActive === '' || isActive == null ? undefined : isActive,
      search,
    },
  })

  const parsed = parseListPayload(data)
  return {
    items: parsed.items,
    total: parsed.total,
    page: parsed.pageNumber ?? pageNumber,
    pageSize: parsed.pageSize ?? pageSize,
    totalPages: parsed.totalPages,
  }
}

/**
 * Optional create — add if your API exposes POST /categories with the same shape as PUT body.
 * @param {{ categoryName: string, isApprovalRequired: boolean }} body
 */
export async function createCategoryApi(body) {
  const { data } = await apiClient.post(BASE, {
    categoryName: body.categoryName,
    isApprovalRequired: body.isApprovalRequired,
  })
  return mapCategoryRow(data?.data ?? data) ?? null
}

/**
 * @param {string|number} id
 * @param {{ categoryName: string, isApprovalRequired: boolean }} body
 */
export async function updateCategoryApi(id, body) {
  const { data } = await apiClient.put(`${BASE}/${id}`, {
    categoryName: body.categoryName,
    isApprovalRequired: body.isApprovalRequired,
  })
  return mapCategoryRow(data?.data ?? data) ?? null
}

/** @param {string|number} id */
export async function activateCategoryApi(id) {
  const { data } = await apiClient.patch(`${BASE}/${id}/active`)
  return mapCategoryRow(data?.data ?? data) ?? { categoryId: id, isActive: true }
}

/** @param {string|number} id */
export async function deactivateCategoryApi(id) {
  const { data } = await apiClient.patch(`${BASE}/${id}/deactive`)
  return mapCategoryRow(data?.data ?? data) ?? { categoryId: id, isActive: false }
}
