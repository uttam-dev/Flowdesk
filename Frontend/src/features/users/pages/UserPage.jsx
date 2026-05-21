import { useCallback, useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { toast } from "sonner";
import { Button } from "../../../components/ui/Button.jsx";
import { ConfirmationModal } from "../../../components/ui/ConfirmationModal.jsx";
import { BulkUploadButton } from "../components/BulkUploadButton.jsx";
import { BulkUploadModal } from "../components/BulkUploadModal.jsx";
import { UserTable } from "../components/UserTable.jsx";
import { UserFormModal } from "../components/UserFormModal.jsx";
import { selectAuthUser, selectRoleNames } from "../../auth/authSlice.js";
import { parseApiError } from "../userApi.js";
import {
  activateUser,
  clearUserError,
  createUser,
  deactivateUser,
  deleteUser,
  fetchUsers,
  selectUserError,
  selectUserIsActive,
  selectUserList,
  selectUserLoading,
  selectUserMutationLoading,
  selectUserPage,
  selectUserPageSize,
  selectUserTotal,
  selectUserTotalPages,
  selectUserRole,
  selectUserEmail,
  setIsActiveFilter,
  setPage,
  setPageSize,
  setRoleFilter,
  setEmailFilter,
  updateUser,
} from "../userSlice.js";

export function UserPage() {
  const dispatch = useDispatch();
  const authUser = useSelector(selectAuthUser);
  const authRoles = useSelector(selectRoleNames);
  const items = useSelector(selectUserList);
  const loading = useSelector(selectUserLoading);
  const error = useSelector(selectUserError);
  const page = useSelector(selectUserPage);
  const pageSize = useSelector(selectUserPageSize);
  const total = useSelector(selectUserTotal);
  const totalPages = useSelector(selectUserTotalPages);
  const isActive = useSelector(selectUserIsActive);
  const role = useSelector(selectUserRole);
  const email = useSelector(selectUserEmail);
  const mutating = useSelector(selectUserMutationLoading);

  const [formOpen, setFormOpen] = useState(false);
  const [formMode, setFormMode] = useState("add");
  const [editing, setEditing] = useState(null);
  const [formKey, setFormKey] = useState(0);
  const [formSaving, setFormSaving] = useState(false);
  const [formServerError, setFormServerError] = useState(null);
  const [searchEmail, setSearchEmail] = useState("");
  const [isBulkModalOpen, setIsBulkModalOpen] = useState(false);

  const [confirm, setConfirm] = useState({
    open: false,
    type: null,
    row: null,
  });
  const [confirmLoading, setConfirmLoading] = useState(false);

  const currentQuery = useCallback(
    () => ({ page, pageSize, isActive, role, email }),
    [page, pageSize, isActive, role, email],
  );

  useEffect(() => {
    dispatch(fetchUsers(currentQuery()));
  }, [dispatch, currentQuery]);

  useEffect(() => {
    if (error) {
      toast.error(error);
      dispatch(clearUserError());
    }
  }, [error, dispatch]);

  const existingEmails = items.map((i) => i.email).filter(Boolean);
  const currentUserId = authUser?.id ?? authUser?.userId ?? null;
  const currentUserRole = authUser?.role ?? authRoles[0] ?? "";
  const isAdmin =
    String(currentUserRole).toLowerCase() === "admin" ||
    authRoles.some((r) => String(r).toLowerCase() === "admin");

  function openAdd() {
    setFormKey((k) => k + 1);
    setFormMode("add");
    setEditing(null);
    setFormServerError(null);
    setFormOpen(true);
  }

  function openEdit(row) {
    setFormKey((k) => k + 1);
    setFormMode("edit");
    setEditing(row);
    setFormServerError(null);
    setFormOpen(true);
  }

  async function handleFormSubmit(values) {
    setFormSaving(true);
    setFormServerError(null);
    try {
      if (formMode === "add") {
        await dispatch(createUser(values)).unwrap();
        toast.success("User created");
      } else if (editing) {
        await dispatch(updateUser({ id: editing.userId, ...values })).unwrap();
        toast.success("User updated");
      }
      setFormOpen(false);
      await dispatch(fetchUsers(currentQuery())).unwrap();
    } catch (e) {
      setFormServerError(typeof e === "string" ? e : parseApiError(e));
    } finally {
      setFormSaving(false);
    }
  }

  function handleSearchEmail() {
    const trimmedEmail = searchEmail.trim();
    dispatch(setEmailFilter(trimmedEmail));
  }

  function handleClearSearch() {
    setSearchEmail("");
    dispatch(setEmailFilter(""));
  }

  function askActivate(row) {
    setConfirm({ open: true, type: "activate", row });
  }

  function askDeactivate(row) {
    setConfirm({ open: true, type: "deactivate", row });
  }

  function askDelete(row) {
    if (currentUserId != null && String(row.userId) === String(currentUserId)) {
      toast.error("You cannot delete your own account");
      return;
    }
    setConfirm({ open: true, type: "delete", row });
  }

  async function runConfirm() {
    if (!confirm.row) return;
    setConfirmLoading(true);
    try {
      if (confirm.type === "activate") {
        await dispatch(activateUser(confirm.row.userId)).unwrap();
        toast.success("User activated");
      } else if (confirm.type === "deactivate") {
        await dispatch(deactivateUser(confirm.row.userId)).unwrap();
        toast.success("User deactivated");
      } else if (confirm.type === "delete") {
        await dispatch(deleteUser(confirm.row.userId)).unwrap();
        toast.success("User deleted");
      }
      setConfirm({ open: false, type: null, row: null });
      await dispatch(fetchUsers(currentQuery())).unwrap();
    } catch (e) {
      toast.error(typeof e === "string" ? e : parseApiError(e));
    } finally {
      setConfirmLoading(false);
    }
  }

  function confirmTitle() {
    if (confirm.type === "delete") return "Delete user";
    if (confirm.type === "activate") return "Activate user";
    return "Deactivate user";
  }

  function confirmMessage() {
    if (confirm.type === "delete") {
      return "Are you sure you want to delete this user?";
    }
    if (confirm.type === "activate") {
      return "Are you sure you want to activate this user?";
    }
    return "Are you sure you want to deactivate this user?";
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 xl:flex-row xl:items-end xl:justify-between">
        <div className="grid w-full gap-4 sm:grid-cols-3 xl:w-auto xl:grid-cols-[11rem_11rem_9rem]">
          <label className="grid gap-1 text-sm font-medium text-gray-700">
            Status
            <select
              className="min-h-11 rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm transition focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
              value={isActive}
              onChange={(ev) => dispatch(setIsActiveFilter(ev.target.value))}
            >
              <option value="">All</option>
              <option value="true">Active</option>
              <option value="false">Inactive</option>
            </select>
          </label>
          <label className="grid gap-1 text-sm font-medium text-gray-700">
            Role
            <select
              className="min-h-11 rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm transition focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
              value={role}
              onChange={(ev) => dispatch(setRoleFilter(ev.target.value))}
            >
              <option value="">All</option>
              <option value="1">Employee</option>
              <option value="2">Manager</option>
              <option value="3">Admin</option>
              <option value="4">Support</option>
            </select>
          </label>
          <label className="grid gap-1 text-sm font-medium text-gray-700">
            Page size
            <select
              className="min-h-11 rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm transition focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
              value={pageSize}
              onChange={(ev) => dispatch(setPageSize(Number(ev.target.value)))}
            >
              {[5, 10, 25, 50].map((size) => (
                <option key={size} value={size}>
                  {size}
                </option>
              ))}
            </select>
          </label>
        </div>
        <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-end">
          {isAdmin ? (
            <BulkUploadButton onClick={() => setIsBulkModalOpen(true)} />
          ) : null}
          <Button type="button" variant="primary" onClick={openAdd}>
            Add user
          </Button>
        </div>
      </div>

      <div className="flex flex-col gap-3">
        <label className="grid gap-1 text-sm font-medium text-gray-700">
          Search by email
          <div className="flex gap-2">
            <input
              type="email"
              className="min-h-11 flex-1 rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm transition placeholder:text-gray-400 focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
              placeholder="Search by email"
              value={searchEmail}
              onChange={(ev) => setSearchEmail(ev.target.value)}
              onKeyDown={(ev) => {
                if (ev.key === "Enter") {
                  handleSearchEmail();
                }
              }}
            />
            <Button type="button" variant="primary" onClick={handleSearchEmail}>
              Search
            </Button>
            <Button
              type="button"
              variant="secondary"
              onClick={handleClearSearch}
            >
              Clear
            </Button>
          </div>
        </label>
      </div>

      <UserTable
        rows={items}
        loading={loading}
        currentUserId={currentUserId}
        onEdit={openEdit}
        onActivate={askActivate}
        onDeactivate={askDeactivate}
        onDelete={askDelete}
      />

      <div className="flex flex-col items-center justify-between gap-3 border-t border-gray-200 pt-4 sm:flex-row">
        <p className="text-sm text-gray-600">
          {total > 0
            ? `Showing ${(page - 1) * pageSize + 1}-${Math.min(page * pageSize, total)} of ${total}`
            : "No users"}
          {" — "}
          Page {page} of {Math.max(1, totalPages || 1)}
        </p>
        <div className="flex gap-2">
          <Button
            type="button"
            variant="secondary"
            disabled={page <= 1}
            onClick={() => dispatch(setPage(page - 1))}
          >
            Previous
          </Button>
          <Button
            type="button"
            variant="secondary"
            disabled={page >= Math.max(1, totalPages || 1)}
            onClick={() => dispatch(setPage(page + 1))}
          >
            Next
          </Button>
        </div>
      </div>

      <UserFormModal
        formKey={formKey}
        open={formOpen}
        onClose={() => {
          if (!formSaving) setFormOpen(false);
        }}
        mode={formMode}
        user={editing}
        existingEmails={existingEmails}
        saving={formSaving || mutating}
        serverError={formServerError}
        onSubmit={handleFormSubmit}
      />

      <BulkUploadModal
        open={isBulkModalOpen}
        onClose={() => setIsBulkModalOpen(false)}
        onCompleted={() => dispatch(fetchUsers(currentQuery()))}
      />

      <ConfirmationModal
        open={confirm.open}
        onClose={() => {
          if (!confirmLoading)
            setConfirm({ open: false, type: null, row: null });
        }}
        title={confirmTitle()}
        message={confirmMessage()}
        confirmLabel={
          confirm.type === "delete"
            ? "Delete"
            : confirm.type === "activate"
              ? "Activate"
              : "Deactivate"
        }
        tone={
          confirm.type === "delete" || confirm.type === "deactivate"
            ? "danger"
            : "neutral"
        }
        loading={confirmLoading}
        onConfirm={runConfirm}
      />
    </div>
  );
}
