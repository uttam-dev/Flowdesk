import { apiClient } from '../../services/apiClient.js'

const BASE = '/requests'

/** @param {unknown} err */
export function parseApiError(err) {
  const d = err?.response?.data
  const errors = d?.errors ?? d?.Errors
  if (errors != null) {
    if (Array.isArray(errors) && errors.length > 0) {
      const first = errors[0]
      if (typeof first === 'string') return first
      if (first?.message) return String(first.message)
      return String(first)
    }
    if (typeof errors === 'object' && !Array.isArray(errors)) {
      const vals = Object.values(errors).flat()
      if (vals.length > 0) {
        const first = vals[0]
        if (typeof first === 'string') return first
        if (first?.message) return String(first.message)
        return String(first)
      }
    }
  }
  if (typeof d === 'string') return d
  if (d?.message) return String(d.message)
  if (d?.title) return String(d.title)
  if (err?.message) return String(err.message)
  return 'Request failed'
}

function mapRequestRow(raw) {
  if (!raw || typeof raw !== 'object') return null
  return {
    requestId: raw.requestId ?? raw.RequestId ?? raw.id ?? raw.Id,
    requestNumber: raw.requestNumber ?? raw.RequestNumber ?? '',
    title: raw.title ?? raw.Title ?? '',
    description: raw.description ?? raw.Description ?? '',
    categoryId: raw.categoryId ?? raw.CategoryId ?? null,
    categoryName: raw.categoryName ?? raw.CategoryName ?? '',
    priority: raw.priority ?? raw.Priority ?? null,
    status: raw.status ?? raw.Status ?? null,
    assignedToId: raw.assignedToId ?? raw.AssignedToId ?? null,
    assignedToName:
      raw.assignedToName ??
      raw.AssignedToName ??
      raw.assignedUserName ??
      raw.AssignedUserName ??
      '',
    approvalName:
      raw.approvalName ??
      raw.ApprovalName ??
      raw.approvedByName ??
      raw.ApprovedByName ??
      '',
    createdOn: raw.createdOn ?? raw.CreatedOn ?? null,
    updatedOn: raw.updatedOn ?? raw.UpdatedOn ?? null,
    createdById: raw.createdById ?? raw.CreatedById ?? null,
    createdByName: raw.createdByName ?? raw.CreatedByName ?? '',
    fullName: raw.fullName ?? raw.FullName ?? raw.createdByName ?? raw.CreatedByName ?? '',
    assignedUser: raw.assignedUser ?? raw.AssignedUser ?? raw.assignedToName ?? raw.AssignedToName ?? '',
    managerId: raw.managerId ?? raw.ManagerId ?? null,
  }
}

function mapCommentRow(raw) {
  if (!raw || typeof raw !== 'object') return null
  return {
    commentId: raw.commentId ?? raw.CommentId ?? raw.id ?? raw.Id,
    requestId: raw.requestId ?? raw.RequestId ?? null,
    commentText: raw.commentText ?? raw.CommentText ?? raw.text ?? raw.Text ?? '',
    userId: raw.userId ?? raw.UserId ?? raw.createdById ?? raw.CreatedById ?? null,
    userName:
      raw.userName ??
      raw.UserName ??
      raw.fullName ??
      raw.FullName ??
      raw.createdByName ??
      raw.CreatedByName ??
      '',
    roleName: raw.roleName ?? raw.RoleName ?? raw.role ?? raw.Role ?? '',
    createdOn: raw.createdOn ?? raw.CreatedOn ?? null,
  }
}

function mapCategoryOption(raw) {
  if (!raw || typeof raw !== 'object') return null
  const categoryId = raw.categoryId ?? raw.CategoryId
  const categoryName = raw.categoryName ?? raw.CategoryName ?? ''
  if (categoryId == null && !categoryName) return null
  return {
    categoryId,
    categoryName: String(categoryName),
  }
}

function mapSupportUser(raw) {
  if (!raw || typeof raw !== 'object') return null
  const id = raw.userId ?? raw.UserId ?? raw.id ?? raw.Id
  const name =
    raw.fullName ?? raw.FullName ?? raw.name ?? raw.Name ?? raw.userName ?? ''
  if (id == null && !name) return null
  const idNum = Number(id)
  return {
    userId: Number.isNaN(idNum) ? id : idNum,
    fullName: String(name),
  }
}

function mapRemarkOption(raw) {
  if (!raw || typeof raw !== 'object') return null
  const id =
    raw.masterRemarkId ??
    raw.MasterRemarkId ??
    raw.masterRemarksId ??
    raw.MasterRemarksId ??
    raw.remarkId ??
    raw.RemarkId ??
    raw.id ??
    raw.Id
  const text =
    raw.remarkText ??
    raw.RemarkText ??
    raw.text ??
    raw.Text ??
    raw.name ??
    raw.Name ??
    ''
  if (id == null && !text) return null
  const idNum = Number(id)
  return {
    masterRemarkId: Number.isNaN(idNum) ? id : idNum,
    remarkText: String(text),
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
    items = root.map(mapRequestRow).filter(Boolean)
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
      items = list.map(mapRequestRow).filter(Boolean)
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

function parseArrayPayload(data, mapper) {
  const root = data?.data !== undefined ? data.data : data
  if (Array.isArray(root)) return root.map(mapper).filter(Boolean)
  const list =
    root?.items ??
    root?.Items ??
    root?.data ??
    root?.Data ??
    root?.results ??
    root?.Results
  if (Array.isArray(list)) return list.map(mapper).filter(Boolean)
  return []
}

/**
 * @param {{ pageNumber?: number, page?: number, pageSize?: number, status?: number|string, priority?: number|string, categoryId?: number|string, requestNumber?: string }} params
 */
export async function fetchRequestsApi(params) {
  const pageNumber = params.pageNumber ?? params.page ?? 1
  const pageSize = params.pageSize ?? 10
  const status = params.status
  const priority = params.priority
  const categoryId = params.categoryId
  const requestNumber = params.requestNumber?.trim() || undefined

  const { data } = await apiClient.get(BASE, {
    params: {
      pageNumber,
      pageSize,
      status: status === '' || status == null ? undefined : status,
      priority: priority === '' || priority == null ? undefined : priority,
      categoryId: categoryId === '' || categoryId == null ? undefined : categoryId,
      requestNumber,
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
 * @param {{ pageNumber?: number, page?: number, pageSize?: number, status?: number|string, priority?: number|string, categoryId?: number|string, requestNumber?: string }} params
 */
export async function fetchTeamRequestsApi(params) {
  const pageNumber = params.pageNumber ?? params.page ?? 1
  const pageSize = params.pageSize ?? 10
  const status = params.status
  const priority = params.priority
  const categoryId = params.categoryId
  const requestNumber = params.requestNumber?.trim() || undefined

  const { data } = await apiClient.get(`${BASE}/team`, {
    params: {
      pageNumber,
      pageSize,
      status: status === '' || status == null ? undefined : status,
      priority: priority === '' || priority == null ? undefined : priority,
      categoryId: categoryId === '' || categoryId == null ? undefined : categoryId,
      requestNumber,
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

/** @param {string|number} id */
export async function fetchRequestByIdApi(id) {
  const { data } = await apiClient.get(`${BASE}/${id}`)
  return mapRequestRow(data?.data ?? data) ?? null
}

/** @param {string|number} id */
export async function fetchRequestCommentsApi(id) {
  const { data } = await apiClient.get(`${BASE}/${id}/comments`)
  return parseArrayPayload(data, mapCommentRow)
}

/**
 * @param {{ categoryId: number, title: string, description: string, priority: number }} body
 */
export async function createRequestApi(body) {
  const { data } = await apiClient.post(BASE, {
    categoryId: Number(body.categoryId),
    title: body.title,
    description: body.description,
    priority: Number(body.priority),
  })
  return mapRequestRow(data?.data ?? data) ?? null
}

/**
 * @param {string|number} id
 * @param {{ masterRemarkId?: number, commentText?: string }} body
 */
export async function approveRequestApi(id, body) {
  const payload = {}
  if (body.masterRemarkId != null && body.masterRemarkId !== '') {
    payload.masterRemarkId = Number(body.masterRemarkId)
  }
  if (body.commentText?.trim()) payload.commentText = body.commentText.trim()
  const { data } = await apiClient.patch(`${BASE}/${id}/approve`, payload)
  return mapRequestRow(data?.data ?? data) ?? { requestId: id }
}

/**
 * @param {string|number} id
 * @param {{ masterRemarkId: number, commentText: string }} body
 */
export async function rejectRequestApi(id, body) {
  const { data } = await apiClient.patch(`${BASE}/${id}/reject`, {
    masterRemarkId: Number(body.masterRemarkId),
    commentText: body.commentText.trim(),
  })
  return mapRequestRow(data?.data ?? data) ?? { requestId: id }
}

/**
 * @param {string|number} id
 * @param {{ assignToId: number, remarksId?: number, commentText?: string }} body
 */
export async function assignRequestApi(id, body) {
  const payload = { assignToId: Number(body.assignToId) }
  if (body.remarksId != null && body.remarksId !== '') {
    payload.remarksId = Number(body.remarksId)
  }
  if (body.commentText?.trim()) payload.commentText = body.commentText.trim()
  const { data } = await apiClient.post(`${BASE}/${id}/assign`, payload)
  return mapRequestRow(data?.data ?? data) ?? { requestId: id }
}

/**
 * @param {string|number} id
 * @param {{ status: number, remarksId?: number, commentText?: string }} body
 */
export async function updateRequestStatusApi(id, body) {
  const payload = { status: Number(body.status) }
  if (body.remarksId != null && body.remarksId !== '') {
    payload.remarksId = Number(body.remarksId)
  }
  if (body.commentText?.trim()) payload.commentText = body.commentText.trim()
  const { data } = await apiClient.post(`${BASE}/${id}/status`, payload)
  return mapRequestRow(data?.data ?? data) ?? { requestId: id }
}

export async function fetchActiveCategoriesApi() {
  const { data } = await apiClient.get('/categories', {
    params: { isActive: true },
  })
  return parseArrayPayload(data, mapCategoryOption)
}

export async function fetchSupportUsersApi() {
  const { data } = await apiClient.get('/users/support')
  return parseArrayPayload(data, mapSupportUser)
}

/** @param {number} actionType */
export async function fetchRemarksApi(actionType) {
  const { data } = await apiClient.get(`${BASE}/remarks`, {
    params: { actionType },
  })
  return parseArrayPayload(data, mapRemarkOption)
}
