import { useCallback, useEffect, useRef, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Link, useParams } from "react-router-dom";
import { toast } from "sonner";
import { Button } from "../../../components/ui/Button.jsx";
import { Modal } from "../../../components/ui/Modal.jsx";
import { apiClient } from "../../../services/apiClient.js";
import { selectRoleNames } from "../../auth/authSlice.js";
import { AuditTrail } from "../components/AuditTrail.jsx";
import { RequestActionModal } from "../components/RequestActionModal.jsx";
import { RequestComments } from "../components/RequestComments.jsx";
import {
  approveRequest,
  assignRequest,
  fetchRequestComments,
  fetchRequestDetail,
  fetchRequests,
  rejectRequest,
  addRequestComment,
  selectRequestComments,
  selectRequestCommentsLoading,
  selectRequestDetail,
  selectRequestDetailError,
  selectRequestDetailLoading,
  selectRequestMutationLoading,
  selectRequestPage,
  selectRequestPageSize,
  selectRequestStatusFilter,
  selectRequestPriorityFilter,
  selectRequestCategoryFilter,
  selectRequestNumberFilter,
  updateRequestStatus,
} from "../requestSlice.js";
import {
  REQUEST_STATUS,
  fmtDate,
  getRowActions,
  hasRole,
  priorityBadgeClass,
  priorityLabel,
  statusBadgeClass,
  statusLabel,
} from "../requestUtils.js";

// ─── Local helpers ────────────────────────────────────────────────────────────

function Badge({ className, children }) {
  return (
    <span
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ring-1 ring-inset ${className}`}
    >
      {children}
    </span>
  );
}

const getSLABadgeStyle = (slaStatus) => {
  switch (slaStatus) {
    case "Within SLA":
      return "bg-emerald-50 text-emerald-800 ring-emerald-100";
    case "Nearing Breach":
      return "bg-orange-50 text-orange-800 ring-orange-100";
    case "Breached":
      return "bg-red-50 text-red-800 ring-red-100";
    default:
      return "bg-gray-100 text-gray-600 ring-gray-200";
  }
};

// ─── Small icon set (inline SVG, no new deps) ─────────────────────────────────

function Icon({ path, className = "h-4 w-4" }) {
  return (
    <svg
      className={`shrink-0 ${className}`}
      fill="none"
      viewBox="0 0 24 24"
      strokeWidth={1.5}
      stroke="currentColor"
      aria-hidden
    >
      <path strokeLinecap="round" strokeLinejoin="round" d={path} />
    </svg>
  );
}

const ICONS = {
  category:
    "M9.568 3H5.25A2.25 2.25 0 003 5.25v4.318c0 .597.237 1.17.659 1.591l9.581 9.581c.699.699 1.78.872 2.607.33a18.095 18.095 0 005.223-5.223c.542-.827.369-1.908-.33-2.607L11.16 3.66A2.25 2.25 0 009.568 3z M6 6h.008v.008H6V6z",
  user: "M15.75 6a3.75 3.75 0 11-7.5 0 3.75 3.75 0 017.5 0zM4.501 20.118a7.5 7.5 0 0114.998 0A17.933 17.933 0 0112 21.75c-2.676 0-5.216-.584-7.499-1.632z",
  check: "M9 12.75L11.25 15 15 9.75M21 12a9 9 0 11-18 0 9 9 0 0118 0z",
  calendar:
    "M6.75 3v2.25M17.25 3v2.25M3 18.75V7.5a2.25 2.25 0 012.25-2.25h13.5A2.25 2.25 0 0121 7.5v11.25m-18 0A2.25 2.25 0 005.25 21h13.5A2.25 2.25 0 0021 18.75m-18 0v-7.5A2.25 2.25 0 015.25 9h13.5A2.25 2.25 0 0121 11.25v7.5",
  clock: "M12 6v6h4.5m4.5 0a9 9 0 11-18 0 9 9 0 0118 0z",
  assign:
    "M18 18.72a9.094 9.094 0 003.741-.479 3 3 0 00-4.682-2.72m.94 3.198l.001.031c0 .225-.012.447-.037.666A11.944 11.944 0 0112 21c-2.17 0-4.207-.576-5.963-1.584A6.062 6.062 0 016 18.719m12 0a5.971 5.971 0 00-.941-3.197m0 0A5.995 5.995 0 0012 12.75a5.995 5.995 0 00-5.058 2.772m0 0a3 3 0 00-4.681 2.72 8.986 8.986 0 003.74.477m.94-3.197a5.971 5.971 0 00-.94 3.197M15 6.75a3 3 0 11-6 0 3 3 0 016 0zm6 3a2.25 2.25 0 11-4.5 0 2.25 2.25 0 014.5 0zm-13.5 0a2.25 2.25 0 11-4.5 0 2.25 2.25 0 014.5 0z",
  sla: "M3.75 13.5l10.5-11.25L12 10.5h8.25L9.75 21.75 12 13.5H3.75z",
  due: "M12 9v3.75m9-.75a9 9 0 11-18 0 9 9 0 0118 0zm-9 3.75h.008v.008H12v-.008z",
};

// ─── Meta row helper ──────────────────────────────────────────────────────────

function MetaField({ icon, label, children }) {
  return (
    <div className="flex min-w-0 flex-col gap-0.5">
      <dt className="flex items-center gap-1.5 text-xs font-medium text-gray-500">
        <Icon path={ICONS[icon]} className="h-3.5 w-3.5 text-gray-400" />
        {label}
      </dt>
      <dd className="truncate text-sm text-gray-900">{children}</dd>
    </div>
  );
}

// ─── Comments (redesigned, left-aligned thread) ───────────────────────────────

function fmtCommentDate(v) {
  if (!v) return "—";
  try {
    const d = new Date(v);
    if (Number.isNaN(d.getTime())) return String(v);
    return d.toLocaleString(undefined, {
      day: "numeric",
      month: "short",
      year: "numeric",
      hour: "numeric",
      minute: "2-digit",
      hour12: true,
    });
  } catch {
    return String(v);
  }
}

function CommentsSection({ comments = [], loading, onAddComment, addingComment }) {
  const [text, setText] = useState("");
  const textareaRef = useRef(null);

  const adjustHeight = () => {
    const el = textareaRef.current;
    if (el) {
      el.style.height = "auto";
      el.style.height = `${el.scrollHeight}px`;
    }
  };

  const submitComment = () => {
    if (!text.trim() || addingComment) return;
    onAddComment(text).then((success) => {
      if (success) {
        setText("");
        if (textareaRef.current) {
          textareaRef.current.style.height = "auto";
        }
      }
    });
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    submitComment();
  };

  const handleKeyDown = (e) => {
    if (e.key === "Enter" && (e.ctrlKey || e.metaKey)) {
      e.preventDefault();
      submitComment();
    }
  };

  const handleChange = (e) => {
    setText(e.target.value);
    adjustHeight();
  };

  if (loading === "pending") {
    return (
      <div className="rounded-xl border border-gray-200 bg-white p-6 text-center text-sm text-gray-500 shadow-sm">
        Loading comments…
      </div>
    );
  }

  return (
    <div className="rounded-xl border border-gray-200 bg-white shadow-sm flex flex-col h-full max-h-[600px]">
      <div className="border-b border-gray-100 px-5 py-3.5">
        <h3 className="text-sm font-semibold text-gray-900">
          Comments
          {comments.length > 0 && (
            <span className="ml-2 rounded-full bg-gray-100 px-2 py-0.5 text-xs font-medium text-gray-600">
              {comments.length}
            </span>
          )}
        </h3>
      </div>

      <div className="divide-y divide-gray-50 px-5 py-2 overflow-y-auto flex-1">
        {comments.length === 0 ? (
          <p className="py-8 text-center text-sm text-gray-400">
            No comments yet.
          </p>
        ) : (
          comments.map((c) => {
            const isMine = c.isCurrentUser === true;
            const rawName = c.userName == null ? "—" : c.userName;
            const name = isMine ? "You" : rawName;
            const role = c.roleName ? ` · ${c.roleName}` : "";

            return (
              <div key={c.commentId ?? `${c.createdOn}`} className="py-4">
                {/* Author row */}
                <div className="mb-2 flex items-center gap-2">
                  <span className="inline-flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-indigo-100 text-xs font-semibold text-indigo-700 ring-1 ring-inset ring-indigo-200">
                    {String(rawName).charAt(0).toUpperCase() || "?"}
                  </span>
                  <span className="text-xs font-semibold text-gray-800">
                    {name}
                  </span>
                  {role && (
                    <span className="text-xs text-gray-400">{role}</span>
                  )}
                  <span className="ml-auto shrink-0 text-xs text-gray-400">
                    {fmtCommentDate(c.createdOn)}
                  </span>
                </div>

                {/* Body */}
                <div
                  className={`rounded-lg px-4 py-3 text-sm leading-relaxed text-gray-800 ${isMine ? "bg-indigo-50" : "bg-gray-50"
                    }`}
                >
                  <p className="whitespace-pre-wrap break-words">
                    {c.commentText}
                  </p>
                </div>
              </div>
            );
          })
        )}
      </div>

      {/* Add comment box */}
      <div className="border-t border-gray-100 bg-gray-50 p-4 rounded-b-xl">
        <form onSubmit={handleSubmit} className="flex gap-3 items-start">
          <div className="flex-1 flex flex-col">
            <textarea
              ref={textareaRef}
              rows={2}
              className="w-full rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm transition focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/20 resize-none max-h-32"
              placeholder="Write a comment..."
              value={text}
              onChange={handleChange}
              onKeyDown={handleKeyDown}
              disabled={addingComment}
            />
            <span className="mt-1 text-right text-[12px] text-gray-400">
              Ctrl+Enter to send
            </span>
          </div>
          <Button
            type="submit"
            variant="primary"
            className="shrink-0 pt-2 pb-2 h-[42px]"
            disabled={!text.trim() || addingComment}
            loading={addingComment}
          >
            <span className="flex items-center gap-1.5">
              <span>Send</span>
              <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" strokeWidth="1.5" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" d="M6 12L3.269 3.126A59.768 59.768 0 0121.485 12 59.77 59.77 0 013.27 20.876L5.999 12zm0 0h7.5" />
              </svg>
            </span>
          </Button>
        </form>
      </div>
    </div>
  );
}

// ─── Main page ────────────────────────────────────────────────────────────────

export function RequestDetailsPage() {
  const { id } = useParams();
  const dispatch = useDispatch();
  const roles = useSelector(selectRoleNames);
  const detail = useSelector(selectRequestDetail);
  const detailLoading = useSelector(selectRequestDetailLoading);
  const detailError = useSelector(selectRequestDetailError);
  const comments = useSelector(selectRequestComments);
  const commentsLoading = useSelector(selectRequestCommentsLoading);
  const mutating = useSelector(selectRequestMutationLoading);

  const page = useSelector(selectRequestPage);
  const pageSize = useSelector(selectRequestPageSize);
  const status = useSelector(selectRequestStatusFilter);
  const priority = useSelector(selectRequestPriorityFilter);
  const categoryId = useSelector(selectRequestCategoryFilter);
  const requestNumber = useSelector(selectRequestNumberFilter);

  const [actionModal, setActionModal] = useState({ open: false, type: null });
  const [actionError, setActionError] = useState(null);
  const [showEscalateModal, setShowEscalateModal] = useState(false);
  const [escalationReason, setEscalationReason] = useState("");
  const [escalationError, setEscalationError] = useState("");
  const [escalating, setEscalating] = useState(false);
  const [addingComment, setAddingComment] = useState(false);

  const loadDetail = useCallback(() => {
    if (!id) return;
    dispatch(fetchRequestDetail(id));
    dispatch(fetchRequestComments(id));
  }, [dispatch, id]);

  useEffect(() => {
    loadDetail();
  }, [loadDetail]);

  useEffect(() => {
    if (detailError) {
      toast.error(detailError);
    }
  }, [detailError]);

  // ── All existing logic — untouched ──────────────────────────────────────────
  const actions =
    detail && detail.requestId
      ? getRowActions(detail, roles).filter((a) => a.key !== "view")
      : [];

  const isAdmin = hasRole(roles, "Admin");

  const canEscalate =
    isAdmin &&
    detail?.isEscalated === false &&
    Number(detail?.status) !== REQUEST_STATUS.Resolved &&
    Number(detail?.status) !== REQUEST_STATUS.Closed &&
    Number(detail?.status) !== REQUEST_STATUS.PendingApproval;

  const canAssign =
    isAdmin &&
    ((detail?.approvalName &&
      Number(detail?.status) === REQUEST_STATUS.Approved) ||
      (!detail?.approvalName &&
        Number(detail?.status) === REQUEST_STATUS.Open));

  const showActions =
    Number(detail?.status) !== REQUEST_STATUS.Closed &&
    Number(detail?.status) !== REQUEST_STATUS.Rejected;

  async function refreshAll() {
    if (!id) return;
    await dispatch(fetchRequestDetail(id)).unwrap();
    await dispatch(fetchRequestComments(id)).unwrap();
    await dispatch(
      fetchRequests({
        page,
        pageSize,
        status,
        priority,
        categoryId,
        requestNumber,
      }),
    )
      .unwrap()
      .catch(() => { });
  }

  function openAction(key) {
    setActionError(null);
    setActionModal({ open: true, type: key });
  }

  function closeEscalateModal() {
    setShowEscalateModal(false);
    setEscalationReason("");
    setEscalationError("");
  }

  async function handleEscalate() {
    if (!escalationReason.trim()) {
      setEscalationError("Reason is required");
      return;
    }
    if (escalationReason.trim().length < 10) {
      setEscalationError("Provide a meaningful reason (min 10 characters)");
      return;
    }
    try {
      setEscalating(true);
      setEscalationError("");
      await apiClient.post(`/requests/${detail.requestId}/escalate`, {
        escalationReason: escalationReason.trim(),
      });
      setShowEscalateModal(false);
      setEscalationReason("");
      await refreshAll();
      toast.success("Request escalated");
    } catch (error) {
      const message =
        error?.response?.data?.message ||
        "Failed to escalate. Please try again.";
      setEscalationError(message);
    } finally {
      setEscalating(false);
    }
  }

  async function handleActionConfirm(payload) {
    if (!id) return;
    setActionError(null);
    try {
      if (actionModal.type === "approve") {
        await dispatch(approveRequest({ id, ...payload })).unwrap();
        toast.success("Request approved");
      } else if (actionModal.type === "reject") {
        await dispatch(rejectRequest({ id, ...payload })).unwrap();
        toast.success("Request rejected");
      } else if (actionModal.type === "assign") {
        await dispatch(assignRequest({ id, ...payload })).unwrap();
        toast.success("Request assigned");
      } else if (
        actionModal.type === "start" ||
        actionModal.type === "resolve"
      ) {
        await dispatch(updateRequestStatus({ id, ...payload })).unwrap();
        toast.success(
          actionModal.type === "start" ? "Request started" : "Request resolved",
        );
      }
      setActionModal({ open: false, type: null });
      await refreshAll();
    } catch (e) {
      setActionError(String(e));
    }
  }

  async function handleAddComment(commentText) {
    if (!id) return false;
    setAddingComment(true);
    try {
      await dispatch(addRequestComment({ id, commentText })).unwrap();
      await dispatch(fetchRequestComments(id)).unwrap();
      return true;
    } catch (e) {
      toast.error(String(e) || "Failed to add comment");
      return false;
    } finally {
      setAddingComment(false);
    }
  }
  // ── End existing logic ──────────────────────────────────────────────────────

  if (detailLoading === "pending" && !detail) {
    return (
      <div className="flex min-h-[40vh] items-center justify-center">
        <div className="rounded-xl border border-gray-200 bg-white px-10 py-12 text-center shadow-sm">
          <div className="mx-auto mb-3 h-8 w-8 animate-spin rounded-full border-2 border-indigo-500 border-t-transparent" />
          <p className="text-sm text-gray-500">Loading request…</p>
        </div>
      </div>
    );
  }

  // ── Sidebar: Actions card (only when showActions && actions.length > 0) ──────
  const actionsCard =
    showActions && actions.length > 0 ? (
      <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
        <div className="border-b border-gray-100 px-4 py-3">
          <h3 className="text-sm font-semibold text-gray-900">Actions</h3>
        </div>
        <div className="flex flex-col gap-2 p-4">
          {actions.map((action) => (
            <Button
              key={action.key}
              type="button"
              variant={action.variant}
              className="w-full justify-center"
              disabled={action.disabled}
              onClick={() => openAction(action.key)}
            >
              {action.label}
            </Button>
          ))}
          {canEscalate ? (
            <Button
              type="button"
              variant="secondary"
              className="w-full justify-center border-orange-300 bg-orange-50 text-orange-800 hover:bg-orange-100"
              onClick={() => setShowEscalateModal(true)}
            >
              🚨 Escalate Request
            </Button>
          ) : null}
        </div>
      </div>
    ) : null;

  // ── Sidebar: Status card ──────────────────────────────────────────────────
  const statusCard = (
    <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
      <div className="border-b border-gray-100 px-4 py-3">
        <h3 className="text-sm font-semibold text-gray-900">Status</h3>
      </div>
      <div className="space-y-3 p-4">
        <div className="flex items-center justify-between gap-2">
          <span className="text-xs text-gray-500">Current status</span>
          <Badge className={statusBadgeClass(detail?.status)}>
            {statusLabel(detail?.status)}
          </Badge>
        </div>
        <div className="flex items-center justify-between gap-2">
          <span className="text-xs text-gray-500">Priority</span>
          <Badge className={priorityBadgeClass(detail?.priority)}>
            {priorityLabel(detail?.priority)}
          </Badge>
        </div>
        <div className="flex items-center justify-between gap-2">
          <span className="text-xs text-gray-500">SLA</span>
          <Badge className={getSLABadgeStyle(detail?.slaStatus)}>
            {detail?.slaStatus || "No SLA"}
          </Badge>
        </div>
        {detail?.isEscalated ? (
          <div className="mt-1 flex items-center gap-1.5 rounded-lg border border-orange-200 bg-orange-50 px-3 py-2 text-xs font-medium text-orange-800">
            🚨 Escalated
          </div>
        ) : null}
      </div>
    </div>
  );

  // ── Sidebar: Meta card ────────────────────────────────────────────────────
  const metaCard = (
    <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
      <div className="border-b border-gray-100 px-4 py-3">
        <h3 className="text-sm font-semibold text-gray-900">Details</h3>
      </div>
      <dl className="space-y-3 p-4">
        <MetaField icon="calendar" label="Created">
          {fmtDate(detail?.createdOn)}
        </MetaField>
        <MetaField icon="clock" label="Updated">
          {fmtDate(detail?.updatedOn)}
        </MetaField>
        <MetaField icon="due" label="Due date">
          {detail?.dueDate ? fmtDate(detail.dueDate) : "Not set"}
        </MetaField>
        <MetaField icon="assign" label="Assigned to">
          {detail?.assignedToName || "—"}
        </MetaField>
        <MetaField icon="check" label="Approved by">
          {detail?.approvalName || "—"}
        </MetaField>
      </dl>
    </div>
  );

  return (
    <div className="space-y-4 pb-10">
      {/* Back link */}
      <Link
        to="/requests"
        className="inline-flex items-center gap-1.5 text-sm font-medium text-indigo-600 hover:text-indigo-800"
      >
        <Icon path="M10.5 19.5L3 12m0 0l7.5-7.5M3 12h18" className="h-4 w-4" />
        Back to requests
      </Link>

      {/* ── Responsive 2-col layout ── */}
      <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:gap-6">
        {/* ── LEFT — main content (70%) ── */}
        <div className="min-w-0 flex-1 space-y-4">
          {/* Request info card */}
          <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
            {/* Header */}
            <div className="border-b border-gray-100 px-5 py-4">
              <p className="text-xs font-medium uppercase tracking-wide text-gray-400">
                #{detail?.requestNumber || detail?.requestId}
              </p>
              <h1 className="mt-1 text-xl font-semibold leading-snug text-gray-900">
                {detail?.title || "—"}
              </h1>
              {detail?.description && (
                <p className="mt-2 text-sm leading-relaxed text-gray-600">
                  {detail.description}
                </p>
              )}
            </div>

            {/* Escalation banner */}
            {detail?.isEscalated ? (
              <div className="border-b border-orange-100 bg-orange-50 px-5 py-3 text-sm text-orange-800">
                <p className="font-medium">
                  🚨 This request has been escalated
                </p>
                <p className="mt-0.5 text-xs text-orange-700">
                  Reason: {detail.escalationReason || "—"} · By:{" "}
                  {detail.escalatedByName || "—"} · On:{" "}
                  {fmtDate(detail.escalatedOn)}
                </p>
              </div>
            ) : null}

            {/* Metadata grid */}
            <dl className="grid grid-cols-1 gap-x-6 gap-y-4 px-5 py-5 sm:grid-cols-2">
              <MetaField icon="category" label="Category">
                {detail?.categoryName || "—"}
              </MetaField>
              <MetaField icon="user" label="Created by">
                {detail?.fullName || "—"}
              </MetaField>
              <MetaField icon="assign" label="Assigned to">
                {detail?.assignedToName || "—"}
              </MetaField>
              <MetaField icon="check" label="Approved by">
                {detail?.approvalName || "—"}
              </MetaField>
              <MetaField icon="calendar" label="Created">
                {fmtDate(detail?.createdOn)}
              </MetaField>
              <MetaField icon="clock" label="Updated">
                {fmtDate(detail?.updatedOn)}
              </MetaField>
              <MetaField icon="due" label="Due date">
                {detail?.dueDate ? fmtDate(detail.dueDate) : "Not set"}
              </MetaField>
              <MetaField icon="sla" label="SLA status">
                <Badge className={getSLABadgeStyle(detail?.slaStatus)}>
                  {detail?.slaStatus || "No SLA"}
                </Badge>
              </MetaField>
            </dl>
          </div>

          {/* Mobile-only: actions + status inline */}
          <div className="space-y-4 lg:hidden">
            {actionsCard}
            {statusCard}
          </div>

          {/* Audit trail */}
          {isAdmin ? <AuditTrail requestId={id} /> : null}

          {/* Comments */}
          <CommentsSection
            comments={comments}
            loading={commentsLoading}
            onAddComment={handleAddComment}
            addingComment={addingComment}
          />

          {/* Escalation history */}
          {detail?.escalationHistory?.length > 0 ? (
            <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
              <div className="border-b border-gray-100 px-5 py-3.5">
                <h3 className="text-sm font-semibold text-gray-900">
                  Escalation history
                </h3>
              </div>
              <div className="divide-y divide-gray-50 px-5">
                {detail.escalationHistory.map((item) => (
                  <div
                    key={
                      item.escalationId ??
                      `${item.escalatedOn}-${item.escalatedByName}`
                    }
                    className="py-4 text-sm"
                  >
                    <p className="font-medium text-gray-900">
                      {item.escalatedByName || "—"}
                    </p>
                    <p className="mt-0.5 text-xs text-gray-500">
                      {fmtDate(item.escalatedOn)}
                    </p>
                    <p className="mt-1.5 text-gray-700">
                      {item.escalationReason || "—"}
                    </p>
                  </div>
                ))}
              </div>
            </div>
          ) : null}
        </div>

        {/* ── RIGHT — sticky sidebar (30%), desktop only ── */}
        <div className="hidden w-72 shrink-0 space-y-4 lg:block lg:sticky lg:top-6 xl:w-80">
          {actionsCard}
          {statusCard}
          {metaCard}
        </div>
      </div>

      {/* ── Modals (logic untouched) ── */}
      <RequestActionModal
        open={actionModal.open}
        actionType={actionModal.type}
        saving={mutating}
        serverError={actionError}
        onClose={() => {
          if (!mutating) setActionModal({ open: false, type: null });
        }}
        onConfirm={handleActionConfirm}
      />

      <Modal
        open={showEscalateModal}
        onClose={escalating ? () => { } : closeEscalateModal}
        title="Escalate Request"
        closeOnOverlayClick={!escalating}
        closeOnEscape={!escalating}
        footer={
          <>
            <Button
              type="button"
              variant="secondary"
              disabled={escalating}
              onClick={closeEscalateModal}
            >
              Cancel
            </Button>
            <Button
              type="button"
              variant="secondary"
              loading={escalating}
              disabled={escalating}
              className="border-orange-300 bg-orange-600 text-orange-700 hover:text-white hover:bg-orange-700"
              onClick={handleEscalate}
            >
              {escalating ? "Escalating..." : "Confirm Escalate"}
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          {escalationError ? (
            <p
              className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700"
              role="alert"
            >
              {escalationError}
            </p>
          ) : null}
          <p className="text-sm text-gray-700">
            Request: {detail?.requestNumber || detail?.requestId} -{" "}
            {detail?.title}
          </p>
          <p className="rounded-lg border border-orange-200 bg-orange-50 px-3 py-2 text-sm text-orange-800">
            Escalating marks this request as urgent. This action cannot be
            undone.
          </p>
          <label className="grid gap-1 text-sm font-medium text-gray-700">
            Reason for escalation *
            <textarea
              className="w-full rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm transition focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/20"
              rows={3}
              value={escalationReason}
              onChange={(ev) => setEscalationReason(ev.target.value)}
              disabled={escalating}
              placeholder="Explain why this request needs immediate attention..."
            />
          </label>
        </div>
      </Modal>
    </div>
  );
}
