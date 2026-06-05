import { useEffect, useId, useRef, useState } from "react";
import { Modal } from "../../../components/ui/Modal.jsx";
import { Button } from "../../../components/ui/Button.jsx";
import { Input } from "../../../components/ui/Input.jsx";
import { PasswordInput } from "../../../components/ui/PasswordInput.jsx";
import { fetchUserManagersApi, fetchUserRolesApi } from "../userApi.js";
import { apiClient } from "../../../services/apiClient.js";

function roleNeedsManager(roleName) {
  return String(roleName || "").toLowerCase() === "employee";
}

function SearchableManagerSelect({
  managers,
  value,
  onChange,
  disabled,
  error,
}) {
  const listId = useId();
  const rootRef = useRef(null);
  const inputRef = useRef(null);
  const [open, setOpen] = useState(false);
  const [query, setQuery] = useState("");

  const selected = managers.find((m) => m.id === value);
  const filtered = managers.filter((m) =>
    m.name.toLowerCase().includes(query.trim().toLowerCase()),
  );

  useEffect(() => {
    if (!open) return;
    function onDocClick(e) {
      if (rootRef.current && !rootRef.current.contains(e.target)) {
        setOpen(false);
        setQuery("");
      }
    }
    document.addEventListener("mousedown", onDocClick);
    return () => document.removeEventListener("mousedown", onDocClick);
  }, [open]);

  useEffect(() => {
    if (open) {
      window.setTimeout(() => inputRef.current?.focus(), 0);
    }
  }, [open]);

  return (
    <div ref={rootRef} className="relative">
      <button
        type="button"
        id="managerId"
        disabled={disabled}
        aria-expanded={open}
        aria-haspopup="listbox"
        aria-controls={listId}
        className={`flex min-h-[44px] w-full items-center justify-between rounded-lg border bg-white px-3 py-2 text-left text-sm text-gray-900 shadow-sm transition-all duration-200 focus:outline-none focus:ring-2 disabled:cursor-not-allowed disabled:bg-gray-50 ${error
          ? "border-red-500 focus:border-red-500 focus:ring-red-500"
          : "border-gray-300 focus:border-indigo-500 focus:ring-indigo-500"
          }`}
        onClick={() => {
          if (!disabled) setOpen((o) => !o);
        }}
      >
        <span className={selected ? "" : "text-gray-400"}>
          {selected?.name ?? "Select manager"}
        </span>
        <svg
          className={`h-4 w-4 shrink-0 text-gray-500 transition ${open ? "rotate-180" : ""}`}
          fill="none"
          viewBox="0 0 24 24"
          strokeWidth={1.5}
          stroke="currentColor"
          aria-hidden
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="M19.5 8.25l-7.5 7.5-7.5-7.5"
          />
        </svg>
      </button>

      {open ? (
        <div
          className="absolute z-20 mt-1 w-full overflow-hidden rounded-lg border border-gray-300 bg-white shadow-lg"
          role="listbox"
          id={listId}
        >
          <div className="border-b border-gray-200 p-2">
            <input
              ref={inputRef}
              type="text"
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              placeholder="Search managers…"
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-900 shadow-sm transition focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500"
              autoComplete="off"
            />
          </div>
          <ul className="max-h-48 overflow-y-auto py-1">
            {filtered.length === 0 ? (
              <li className="px-3 py-2 text-sm text-gray-500">
                No managers found
              </li>
            ) : (
              filtered.map((m) => (
                <li key={m.id}>
                  <button
                    type="button"
                    role="option"
                    aria-selected={value === m.id}
                    className={`w-full px-3 py-2 text-left text-sm transition hover:bg-gray-50 ${value === m.id
                      ? "bg-indigo-50 font-medium text-indigo-900"
                      : "text-gray-800"
                      }`}
                    onClick={() => {
                      onChange(m.id);
                      setOpen(false);
                      setQuery("");
                    }}
                  >
                    {m.name}
                  </button>
                </li>
              ))
            )}
          </ul>
        </div>
      ) : null}
      {error ? (
        <p className="text-red-500 text-sm mt-1" role="alert">
          {error}
        </p>
      ) : null}
    </div>
  );
}

function validatePassword(password) {
  if (!password || password.length < 8) {
    return "Password must be at least 8 characters";
  }
  if (!/[A-Z]/.test(password)) return "Include at least one uppercase letter";
  if (!/[a-z]/.test(password)) return "Include at least one lowercase letter";
  if (!/[0-9]/.test(password)) return "Include at least one number";
  if (!/[^A-Za-z0-9]/.test(password)) {
    return "Include at least one special character";
  }
  return null;
}

function validateEmail(email) {
  const t = email.trim();
  if (!t) return "Email is required";
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(t)) return "Enter a valid email";
  return null;
}

function validateFullName(name) {
  const t = name.trim();
  if (!t) return "Full name is required";
  if (t.length < 3) return "Minimum 3 characters required";
  if (!/^[A-Za-z ]+$/.test(t)) return "Only letters and spaces allowed";
  if (t.length > 70) return "Use up to 70 characters";
  return null;
}

function UserFormModalInner({
  onClose,
  mode,
  user,
  existingEmails,
  saving,
  serverError,
  onSubmit,
}) {
  const [activeTab, setActiveTab] = useState("details")

  const [fullName, setFullName] = useState(() => user?.fullName ?? "");
  const [email, setEmail] = useState(() => user?.email ?? "");
  const [password, setPassword] = useState("");
  const [roleId, setRoleId] = useState(() =>
    user?.roleId != null ? Number(user.roleId) : "",
  );
  const [managerId, setManagerId] = useState(() =>
    user?.managerId != null ? Number(user.managerId) : null,
  );
  const [roles, setRoles] = useState([]);
  const [managers, setManagers] = useState([]);
  const [lookupsLoading, setLookupsLoading] = useState(true);
  const [fieldErrors, setFieldErrors] = useState({});
  const [error, setError] = useState(null);

  const [newPassword, setNewPassword] = useState("")
  const [confirmPassword, setConfirmPassword] = useState("")
  const [passErrors, setPassErrors] = useState({})
  const [passLoading, setPassLoading] = useState(false)
  const [passSuccess, setPassSuccess] = useState("")

  const selectedRole = roles.find((r) => r.roleId === roleId);
  const managerEnabled = roleNeedsManager(selectedRole?.roleName);

  useEffect(() => {
    let cancelled = false;
    setLookupsLoading(true);
    Promise.all([fetchUserRolesApi(), fetchUserManagersApi()])
      .then(([roleList, managerList]) => {
        if (cancelled) return;
        setRoles(roleList);
        setManagers(managerList);
      })
      .catch(() => {
        if (!cancelled) setError("Failed to load form options");
      })
      .finally(() => {
        if (!cancelled) setLookupsLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, []);

  useEffect(() => {
    if (!managerEnabled) {
      setManagerId(null);
    }
  }, [managerEnabled, roleId]);

  useEffect(() => {
    if (user?.roleId != null || !user?.roleName || !roles.length) return;
    const match = roles.find((r) => r.roleName === user.roleName);
    if (match?.roleId != null) setRoleId(match.roleId);
  }, [user, roles]);

  const validatePasswordTab = () => {
    const e = {}

    if (!newPassword)
      e.newPassword = "New password is required"
    else if (newPassword.length < 8)
      e.newPassword = "Minimum 8 characters required"
    else if (!/[A-Z]/.test(newPassword))
      e.newPassword = "Must contain at least one uppercase letter"
    else if (!/[a-z]/.test(newPassword))
      e.newPassword = "Must contain at least one lowercase letter"
    else if (!/[0-9]/.test(newPassword))
      e.newPassword = "Must contain at least one number"
    else if (!/[^A-Za-z0-9]/.test(newPassword))
      e.newPassword = "Must contain at least one special character"

    if (!confirmPassword)
      e.confirmPassword = "Please confirm the password"
    else if (newPassword !== confirmPassword)
      e.confirmPassword = "Passwords do not match"

    setPassErrors(e)
    return Object.keys(e).length === 0
  }

  const handlePasswordReset = async () => {
    if (!validatePasswordTab()) return

    try {
      setPassLoading(true)
      setPassErrors({})
      setPassSuccess("")

      await apiClient.post(
        `/users/${user.userId}/reset-password`,
        { newPassword }
      )

      setPassSuccess("Password updated successfully")
      setNewPassword("")
      setConfirmPassword("")
    } catch (err) {
      const msg = err?.response?.data?.message
        || "Failed to update password. Try again."
      setPassErrors({ api: msg })
    } finally {
      setPassLoading(false)
    }
  }

  function handleSubmit(e) {
    e.preventDefault();
    const next = {};
    const nameErr = validateFullName(fullName);
    if (nameErr) next.fullName = nameErr;

    const emailErr = validateEmail(email);
    if (emailErr) next.email = emailErr;
    else {
      const trimmed = email.trim();
      const others = (existingEmails ?? []).filter(
        (em) => em.toLowerCase() !== (user?.email ?? "").trim().toLowerCase(),
      );
      if (others.some((em) => em.toLowerCase() === trimmed.toLowerCase())) {
        next.email = "Email must be unique";
      }
    }

    if (mode === "add") {
      const pwErr = validatePassword(password);
      if (pwErr) next.password = pwErr;
    }

    if (roleId === "" || roleId == null) next.roleId = "Role is required";

    if (managerEnabled && (managerId == null || managerId === "")) {
      next.managerId = "Manager is required for Employee role";
    }

    if (Object.keys(next).length) {
      setFieldErrors(next);
      return;
    }

    setFieldErrors({});
    setError(null);

    const payload = {
      fullName: fullName.trim(),
      email: email.trim(),
      roleId: Number(roleId),
      managerId: managerEnabled && managerId != null ? Number(managerId) : null,
    };
    if (mode === "add") {
      payload.password = password;
    }
    onSubmit(payload);
  }

  const title = mode === "add" ? "Add user" : "Edit user";
  const combinedError = error || serverError;

  return (
    <Modal
      open
      onClose={saving ? () => { } : onClose}
      title={title}
      size="lg"
      closeOnOverlayClick={!saving}
      closeOnEscape={!saving}
      footer={
        activeTab === "details" ? (
          <>
            <Button
              type="button"
              variant="secondary"
              disabled={saving}
              onClick={onClose}
            >
              Cancel
            </Button>
            <Button
              type="button"
              variant="primary"
              loading={saving}
              disabled={saving || lookupsLoading}
              onClick={handleSubmit}
            >
              Save
            </Button>
          </>
        ) : (
          <>
            <Button
              type="button"
              variant="secondary"
              disabled={passLoading}
              onClick={onClose}
            >
              Cancel
            </Button>
            <Button
              type="button"
              variant="primary"
              loading={passLoading}
              disabled={passLoading}
              onClick={handlePasswordReset}
            >
              {passLoading ? "Updating..." : "Update Password"}
            </Button>
          </>
        )
      }
    >
      {/* Tab bar — only shown in edit mode */}
      {mode === "edit" && (
        <div className="mb-4 flex gap-1 rounded-lg bg-gray-100 p-1">
          <button
            type="button"
            onClick={() => setActiveTab("details")}
            className={`flex-1 rounded-md px-3 py-1.5 text-sm font-medium transition-all duration-200 ${activeTab === "details"
              ? "bg-white text-gray-900 shadow-sm"
              : "text-gray-500 hover:text-gray-700"
              }`}
          >
            User Details
          </button>
          <button
            type="button"
            onClick={() => setActiveTab("password")}
            className={`flex-1 rounded-md px-3 py-1.5 text-sm font-medium transition-all duration-200 ${activeTab === "password"
              ? "bg-white text-gray-900 shadow-sm"
              : "text-gray-500 hover:text-gray-700"
              }`}
          >
            Reset Password
          </button>
        </div>
      )}

      {/* ── Tab 1: User Details ── */}
      {activeTab === "details" && (
        <form className="space-y-4" onSubmit={handleSubmit}>
          {combinedError ? (
            <p
              className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700"
              role="alert"
            >
              {combinedError}
            </p>
          ) : null}

          <div className="flex flex-col gap-1">
            <Input
              id="fullName"
              label={
                <>
                  Full name
                  <span className="text-red-500 ml-1">*</span>
                </>
              }
              value={fullName}
              onChange={(ev) => {
                setFullName(ev.target.value);
                if (fieldErrors.fullName) {
                  setFieldErrors((prev) => ({ ...prev, fullName: undefined }));
                }
              }}
              disabled={saving}
              maxLength={70}
              autoComplete="name"
              error={fieldErrors.fullName}
            />
            <p className="text-xs text-gray-500">{fullName.trim().length}/70</p>
          </div>

          <Input
            id="email"
            label={
              <>
                Email
                <span className="text-red-500 ml-1">*</span>
              </>
            }
            type="email"
            value={email}
            onChange={(ev) => {
              setEmail(ev.target.value);
              if (fieldErrors.email) {
                setFieldErrors((prev) => ({ ...prev, email: undefined }));
              }
            }}
            disabled={saving}
            autoComplete="off"
            error={fieldErrors.email}
          />

          {mode === "add" ? (
            <PasswordInput
              id="password"
              label={
                <>
                  Password
                  <span className="text-red-500 ml-1">*</span>
                </>
              }
              value={password}
              onChange={(ev) => {
                setPassword(ev.target.value);
                if (fieldErrors.password) {
                  setFieldErrors((prev) => ({ ...prev, password: undefined }));
                }
              }}
              disabled={saving}
              autoComplete="new-password"
              error={fieldErrors.password}
            />
          ) : null}

          <div className="flex flex-col gap-1.5">
            <label htmlFor="roleId" className="text-sm font-medium text-gray-700">
              Role
              <span className="text-red-500 ml-1">*</span>
            </label>
            <select
              id="roleId"
              className="w-full rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm transition-all duration-200 focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500 disabled:bg-gray-50"
              value={roleId}
              onChange={(ev) => {
                const next =
                  ev.target.value === "" ? "" : Number(ev.target.value);
                setRoleId(next);
                if (fieldErrors.roleId) {
                  setFieldErrors((prev) => ({ ...prev, roleId: undefined }));
                }
              }}
              disabled={saving || lookupsLoading}
            >
              <option value="">Select role</option>
              {roles.map((r) => (
                <option key={r.roleId} value={r.roleId ?? ""}>
                  {r.roleName}
                </option>
              ))}
            </select>
            {fieldErrors.roleId ? (
              <p className="text-red-500 text-sm mt-1" role="alert">
                {fieldErrors.roleId}
              </p>
            ) : null}
          </div>

          <div className="flex flex-col gap-1.5">
            <label
              htmlFor="managerId"
              className="text-sm font-medium text-gray-700"
            >
              Manager
              {managerEnabled ? (
                <span className="text-red-500 ml-1">*</span>
              ) : null}
              {!managerEnabled ? (
                <span className="ml-1 font-normal text-gray-500">
                  (not applicable)
                </span>
              ) : null}
            </label>
            {managerEnabled ? (
              <SearchableManagerSelect
                managers={managers}
                value={managerId}
                onChange={(id) => {
                  setManagerId(id);
                  if (fieldErrors.managerId) {
                    setFieldErrors((prev) => ({ ...prev, managerId: undefined }));
                  }
                }}
                disabled={saving || lookupsLoading}
                error={fieldErrors.managerId}
              />
            ) : (
              <select
                id="managerId"
                className="w-full cursor-not-allowed rounded-lg border border-gray-200 bg-gray-50 px-3 py-2 text-sm text-gray-500"
                value=""
                disabled
              >
                <option value="">—</option>
              </select>
            )}
          </div>
        </form>
      )}

      {/* ── Tab 2: Reset Password ── */}
      {activeTab === "password" && (
        <div className="space-y-4">
          {passSuccess && (
            <div
              className="rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-800"
              role="alert"
            >
              {passSuccess}
            </div>
          )}

          {passErrors.api && (
            <div
              className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700"
              role="alert"
            >
              {passErrors.api}
            </div>
          )}

          <PasswordInput
            id="newPassword"
            label={
              <>
                New Password <span className="text-red-500">*</span>
              </>
            }
            value={newPassword}
            onChange={(e) => {
              setNewPassword(e.target.value)
              if (passErrors.newPassword)
                setPassErrors((prev) => ({ ...prev, newPassword: undefined }))
            }}
            disabled={passLoading}
            autoComplete="new-password"
            error={passErrors.newPassword}
          />

          <PasswordInput
            id="confirmPassword"
            label={
              <>
                Confirm Password <span className="text-red-500">*</span>
              </>
            }
            value={confirmPassword}
            onChange={(e) => {
              setConfirmPassword(e.target.value)
              if (passErrors.confirmPassword)
                setPassErrors((prev) => ({ ...prev, confirmPassword: undefined }))
            }}
            disabled={passLoading}
            autoComplete="new-password"
            error={passErrors.confirmPassword}
          />
        </div>
      )}
    </Modal>
  );
}

export function UserFormModal({
  open,
  onClose,
  mode,
  user,
  existingEmails,
  saving,
  serverError,
  onSubmit,
  formKey = 0,
}) {
  if (!open) return null;

  return (
    <UserFormModalInner
      key={`${formKey}-${mode}-${user?.userId ?? "new"}`}
      onClose={onClose}
      mode={mode}
      user={user}
      existingEmails={existingEmails}
      saving={saving}
      serverError={serverError}
      onSubmit={onSubmit}
    />
  );
}
