/**
 * Normalizes login / refresh payloads, including:
 * `{ statusCode, message, data: { name, email, role, accessToken, refreshToken } }`
 * @param {unknown} raw
 * @returns {{ accessToken: string, refreshToken: string | null, user: import('./types').AuthUser | null }}
 */
export function mapAuthResponse(raw) {
  const body =
    raw && typeof raw === "object" && "data" in raw && raw.data != null
      ? raw.data
      : raw;
  const b = body && typeof body === "object" ? body : {};

  const accessToken =
    b.accessToken ?? b.token ?? b.access_token ?? b.Token ?? null;
  const refreshToken =
    b.refreshToken ?? b.refresh_token ?? b.RefreshToken ?? null;

  const userRaw = b.user ?? b.User ?? b.userAccount ?? b.account ?? null;
  let user = null;
  if (userRaw && typeof userRaw === "object") {
    user = {
      id: userRaw.id ?? userRaw.userId ?? userRaw.UserId ?? null,
      email: userRaw.email ?? userRaw.Email ?? null,
      name: userRaw.name ?? userRaw.fullName ?? userRaw.FullName ?? null,
      roles: normalizeRoles(
        userRaw.roles ?? userRaw.Roles ?? userRaw.role ?? userRaw.Role,
      ),
      permissions: normalizeRoles(
        userRaw.permissions ??
          userRaw.Permissions ??
          userRaw.permission ??
          userRaw.Permission,
      ),
    };
  } else if (b.email || b.Email || b.name || b.Name) {
    user = {
      id: b.id ?? b.userId ?? null,
      email: b.email ?? b.Email ?? null,
      name: b.name ?? b.Name ?? b.fullName ?? null,
      roles: normalizeRoles(
        b.roles ?? b.Roles ?? b.role ?? b.Role ?? b.roleName,
      ),
      permissions: normalizeRoles(b.permissions ?? b.Permissions),
    };
  }

  return {
    accessToken,
    refreshToken,
    user,
  };
}

function normalizeRoles(value) {
  if (!value && value !== 0) return [];
  if (Array.isArray(value)) return value.map(String);
  if (typeof value === "string") return [value];
  return [];
}
