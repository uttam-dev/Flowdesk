import { useCallback, useEffect, useState } from "react";
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
import { RequestDetails } from "../components/RequestDetails.jsx";
import {
  approveRequest,
  assignRequest,
  fetchRequestComments,
  fetchRequestDetail,
  fetchRequests,
  rejectRequest,
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
  statusLabel,
} from "../requestUtils.js";

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

  const actions =
    detail && detail.requestId
      ? getRowActions(detail, roles).filter((a) => a.key !== "view")
      : [];
  const isAdmin = hasRole(roles, "Admin");
  const canEscalate =
    isAdmin &&
    detail?.isEscalated === false &&
    Number(detail?.status) !== REQUEST_STATUS.Resolved &&
    Number(detail?.status) !== REQUEST_STATUS.Closed;

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
      .catch(() => {});
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

  if (detailLoading === "pending" && !detail) {
    return (
      <div className="rounded-xl border border-gray-200 bg-white p-10 text-center text-sm text-gray-600 shadow-sm">
        Loading request…
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <Link
        to="/requests"
        className="inline-flex text-sm font-medium text-indigo-600 hover:text-indigo-800"
      >
        ← Back to requests
      </Link>

      <RequestDetails request={detail} />

      {showActions && actions.length > 0 ? (
        <div className="flex flex-wrap gap-2 rounded-xl border border-gray-200 bg-white p-4 shadow-sm">
          <p className="w-full text-sm font-medium text-gray-700">Actions</p>
          {actions.map((action) => (
            <Button
              key={action.key}
              type="button"
              variant={action.variant}
              onClick={() => openAction(action.key)}
            >
              {action.label}
            </Button>
          ))}
          {canEscalate ? (
            <Button
              type="button"
              variant="secondary"
              className="border-orange-300 bg-orange-50 text-orange-800 hover:bg-orange-100"
              onClick={() => setShowEscalateModal(true)}
            >
              Escalate Request
            </Button>
          ) : null}
        </div>
      ) : null}

      {isAdmin ? <AuditTrail requestId={id} /> : null}

      <RequestComments comments={comments} loading={commentsLoading} />

      {detail?.escalationHistory?.length > 0 ? (
        <div className="rounded-xl border border-gray-200 bg-white p-4 shadow-sm">
          <h3 className="text-sm font-semibold text-gray-900">
            Escalation History
          </h3>
          <div className="mt-4 space-y-3">
            {detail.escalationHistory.map((item) => (
              <div
                key={
                  item.escalationId ??
                  `${item.escalatedOn}-${item.escalatedByName}`
                }
                className="rounded-lg border border-gray-200 bg-gray-50 p-3 text-sm"
              >
                <p className="font-medium text-gray-900">
                  Escalated by: {item.escalatedByName || "—"}
                </p>
                <p className="mt-1 text-gray-600">
                  On: {fmtDate(item.escalatedOn)}
                </p>
                <p className="mt-1 text-gray-700">
                  Reason: {item.escalationReason || "—"}
                </p>
              </div>
            ))}
          </div>
        </div>
      ) : null}

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
        onClose={escalating ? () => {} : closeEscalateModal}
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
              className="border-orange-300 bg-orange-600 text-white hover:bg-orange-700"
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
            Request: {detail?.requestNumber || detail?.requestId} —{" "}
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
