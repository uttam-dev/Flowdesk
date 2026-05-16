import { apiClient } from '../../services/apiClient.js'

const BASE = '/users'

/** @param {unknown} err */
export function parseApiError(err) {
  const d = err?.response?.data
  const errors = d?.errors ?? d?.Errors
  if (Array.isArray(errors) && errors.length > 0) {
    const first = errors[0]
    if (typeof first === 'string') return first
    if (first?.message) return String(first.message)
    return String(first)
  }
  if (typeof d === 'string') return d
  if (d?.message) return String(d.message)
  if (d?.title) return String(d.title)
  if (err?.message) return String(err.message)
  return 'Request failed'
}

function normalizeManagerId(value) {
  if (value == null || value === '') return null
  const n = Number(value)
  return Number.isNaN(n) ? null : n
}

function mapUserRow(raw) {
  if (!raw || typeof raw !== 'object') return null
  return {
    userId: raw.userId ?? raw.UserId ?? raw.id ?? raw.Id,
    fullName: raw.fullName ?? raw.FullName ?? '',
    email: raw.email ?? raw.Email ?? '',
    roleId: raw.roleId ?? raw.RoleId ?? null,
    roleName: raw.roleName ?? raw.RoleName ?? '',
    managerId: raw.managerId ?? raw.ManagerId ?? null,
    managerName: raw.managerName ?? raw.ManagerName ?? '',
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
    items = root.map(mapUserRow).filter(Boolean)
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
      items = list.map(mapUserRow).filter(Boolean)
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

function mapRoleOption(raw) {
  if (!raw || typeof raw !== 'object') return null
  const roleId = raw.roleId ?? raw.RoleId
  const roleName =
    raw.roleName ?? raw.RoleName ?? raw.name ?? raw.Name ?? ''
  if (roleId == null && !roleName) return null
  const idNum = roleId != null ? Number(roleId) : null
  return {
    roleId: idNum != null && !Number.isNaN(idNum) ? idNum : null,
    roleName: String(roleName),
  }
}

function mapManagerOption(raw) {
  if (!raw || typeof raw !== 'object') return null
  const id =
    raw.managerId ??
    raw.ManagerId ??
    raw.userId ??
    raw.UserId ??
    raw.id ??
    raw.Id
  const name =
    raw.managerName ??
    raw.ManagerName ??
    raw.fullName ??
    raw.FullName ??
    raw.name ??
    raw.Name ??
    ''
  if (id == null && !name) return null
  const idNum = Number(id)
  return {
    id: Number.isNaN(idNum) ? null : idNum,
    name: String(name),
  }
}

/**
 * @param {{ pageNumber?: number, page?: number, pageSize?: number, isActive?: boolean|string }} params
 */
export async function fetchUsersApi(params) {
  const pageNumber = params.pageNumber ?? params.page ?? 1
  const pageSize = params.pageSize ?? 10
  const isActive = params.isActive

  const { data } = await apiClient.get(BASE, {
    params: {
      pageNumber,
      pageSize,
      IsActive: isActive === '' || isActive == null ? undefined : isActive,
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

/** @returns {Promise<Array<{ roleId: number|null, roleName: string }>>} */
export async function fetchUserRolesApi() {
  const { data } = await apiClient.get(`${BASE}/roles`)
  const root = data?.data !== undefined ? data.data : data
  if (Array.isArray(root)) {
    return root.map(mapRoleOption).filter(Boolean)
  }
  const list = root?.items ?? root?.Items ?? root?.roles ?? root?.Roles
  if (Array.isArray(list)) {
    return list.map(mapRoleOption).filter(Boolean)
  }
  return []
}

/** @returns {Promise<Array<{ id: number|null, name: string }>>} */
export async function fetchUserManagersApi() {
  const { data } = await apiClient.get(`${BASE}/managers`)
  const root = data?.data !== undefined ? data.data : data
  let list = []
  if (Array.isArray(root)) {
    list = root
  } else {
    list =
      root?.items ??
      root?.Items ??
      root?.managers ??
      root?.Managers ??
      []
  }
  if (!Array.isArray(list)) return []
  return list.map(mapManagerOption).filter(Boolean)
}

/**
 * @param {{ fullName: string, email: string, password: string, roleId: number, managerId?: number|null }} body
 */
export async function createUserApi(body) {
  const payload = {
    fullName: body.fullName,
    email: body.email,
    password: body.password,
    roleId: Number(body.roleId),
    managerId: normalizeManagerId(body.managerId),
  }
  const { data } = await apiClient.post(BASE, payload)
  return mapUserRow(data?.data ?? data) ?? null
}

/**
 * @param {string|number} id
 * @param {{ fullName: string, email: string, roleId: number, managerId?: number|null, password?: string }} body
 */
export async function updateUserApi(id, body) {
  const payload = {
    fullName: body.fullName,
    email: body.email,
    roleId: Number(body.roleId),
    managerId: normalizeManagerId(body.managerId),
  }
  if (body.password) {
    payload.password = body.password
  }
  const { data } = await apiClient.put(`${BASE}/${id}`, payload)
  return mapUserRow(data?.data ?? data) ?? null
}

/** @param {string|number} id */
export async function deleteUserApi(id) {
  await apiClient.delete(`${BASE}/${id}`)
  return { userId: id }
}

/** @param {string|number} id */
export async function activateUserApi(id) {
  const { data } = await apiClient.patch(`${BASE}/${id}/active`)
  return mapUserRow(data?.data ?? data) ?? { userId: id, isActive: true }
}

/** @param {string|number} id */
export async function deactivateUserApi(id) {
  const { data } = await apiClient.patch(`${BASE}/${id}/deactive`)
  return mapUserRow(data?.data ?? data) ?? { userId: id, isActive: false }
}
