import { useCallback, useEffect, useMemo, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { Button } from "../../../components/ui/Button.jsx";
import { Modal } from "../../../components/ui/Modal.jsx";
import { apiClient } from "../../../services/apiClient.js";
import { selectAuthUser, selectRoleNames } from "../../auth/authSlice.js";
import { RequestActionModal } from "../components/RequestActionModal.jsx";
import { RequestFilters } from "../components/RequestFilters.jsx";
import { RequestTable } from "../components/RequestTable.jsx";
import {
  approveRequest,
  assignRequest,
  clearRequestError,
  fetchRequestComments,
  fetchRequestDetail,
  fetchRequests,
  rejectRequest,
  selectRequestActiveTab,
  selectRequestCategoryFilter,
  selectRequestError,
  selectRequestList,
  selectRequestLoading,
  selectRequestMutationLoading,
  selectRequestNumberFilter,
  selectRequestPage,
  selectRequestPageSize,
  selectRequestPriorityFilter,
  selectRequestStatusFilter,
  selectRequestTotal,
  selectRequestTotalPages,
  setActiveTab,
  setCategoryFilter,
  setPage,
  setPageSize,
  setPriorityFilter,
  setRequestNumberFilter,
  setStatusFilter,
  updateRequestStatus,
} from "../requestSlice.js";
import { REQUEST_TABS, REQUEST_STATUS, hasRole } from "../requestUtils.js";

export function RequestListPage() {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const roles = useSelector(selectRoleNames);
  const authUser = useSelector(selectAuthUser);
  const currentUserId = authUser?.id ?? null;
  const isManager = hasRole(roles, "Manager");
  const isSupport = hasRole(roles, "Support");

  const items = useSelector(selectRequestList);
  const loading = useSelector(selectRequestLoading);
  const error = useSelector(selectRequestError);
  const page = useSelector(selectRequestPage);
  const pageSize = useSelector(selectRequestPageSize);
  const total = useSelector(selectRequestTotal);
  const totalPages = useSelector(selectRequestTotalPages);
  const status = useSelector(selectRequestStatusFilter);
  const priority = useSelector(selectRequestPriorityFilter);
  const categoryId = useSelector(selectRequestCategoryFilter);
  const requestNumber = useSelector(selectRequestNumberFilter);
  const activeTab = useSelector(selectRequestActiveTab);
  const mutating = useSelector(selectRequestMutationLoading);

  const isEscalateEligible = (request) =>
    request?.isEscalated !== true &&
    Number(request?.status) !== REQUEST_STATUS.Resolved &&
    Number(request?.status) !== REQUEST_STATUS.Closed &&
    Number(request?.status) !== REQUEST_STATUS.Rejected &&
    Number(request?.status) !== REQUEST_STATUS.PendingApproval;

  const escalatedCount = useMemo(
    () =>
      items.filter(
        (request) =>
          request.isEscalated === true &&
          Number(request.status) !== REQUEST_STATUS.Resolved &&
          Number(request.status) !== REQUEST_STATUS.Closed,
      ).length,
    [items],
  );

  const filteredItems = useMemo(() => {
    let result = items;
    if (activeTab === "resolved") {
      result = result.filter(
        (it) =>
          Number(it.status) === REQUEST_STATUS.Resolved ||
          Number(it.status) === REQUEST_STATUS.Closed,
      );
    } else if (activeTab === "escalated") {
      result = result.filter(
        (it) =>
          it.isEscalated === true &&
          Number(it.status) !== REQUEST_STATUS.Resolved &&
          Number(it.status) !== REQUEST_STATUS.Closed,
      );
    } else if (isSupport) {
      result = result.filter(
        (it) =>
          Number(it.status) === REQUEST_STATUS.Assigned ||
          Number(it.status) === REQUEST_STATUS.InProgress ||
          Number(it.status) === REQUEST_STATUS.Resolved ||
          Number(it.status) === REQUEST_STATUS.Closed,
      );
    }
    return result;
  }, [items, isSupport, activeTab]);

  const [actionModal, setActionModal] = useState({
    open: false,
    type: null,
    row: null,
  });
  const [actionError, setActionError] = useState(null);
  const [showEscalateModal, setShowEscalateModal] = useState(false);
  const [selectedRequest, setSelectedRequest] = useState(null);
  const [escalationReason, setEscalationReason] = useState("");
  const [escalationError, setEscalationError] = useState("");
  const [escalating, setEscalating] = useState(false);

  const visibleTabs = useMemo(() => {
    if (isManager) {
      return [
        { key: "my", label: "My Requests", status: "" },
        { key: "team", label: "Team Requests", status: "" },
      ];
    }
    if (isSupport) {
      return [
        ...REQUEST_TABS.filter((tab) =>
          ["all", "assigned", "inprogress", "resolved"].includes(tab.key),
        ),
        {
          key: "escalated",
          label: `Escalated (${escalatedCount})`,
          status: "",
        },
      ];
    }
    return REQUEST_TABS;
  }, [isManager, isSupport, escalatedCount]);

  const listQuery = useCallback(
    () => ({
      page,
      pageSize,
      status,
      priority,
      categoryId,
      requestNumber,
      activeTab,
    }),
    [page, pageSize, status, priority, categoryId, requestNumber, activeTab],
  );

  useEffect(() => {
    dispatch(fetchRequests(listQuery()));
  }, [dispatch, listQuery]);

  useEffect(() => {
    if (isManager && (activeTab === "all" || activeTab === "rejected")) {
      dispatch(setActiveTab({ key: "my", status: "" }));
    }
  }, [isManager, activeTab, dispatch]);

  useEffect(() => {
    if (
      isSupport &&
      !["all", "assigned", "inprogress", "resolved", "escalated"].includes(
        activeTab,
      )
    ) {
      dispatch(setActiveTab({ key: "all", status: "" }));
    }
  }, [isSupport, activeTab, dispatch]);

  useEffect(() => {
    if (error) {
      toast.error(error);
      dispatch(clearRequestError());
    }
  }, [error, dispatch]);

  const isClientFilteredTab =
    activeTab === "resolved" || activeTab === "escalated";
  const displayTotal = isClientFilteredTab ? filteredItems.length : total;
  const displayTotalPages = isClientFilteredTab
    ? Math.ceil(filteredItems.length / pageSize)
    : totalPages;

  const safeTotalPages = Math.max(1, displayTotalPages || 1);
  const firstItem = displayTotal === 0 ? 0 : (page - 1) * pageSize + 1;
  const lastItem = Math.min(page * pageSize, displayTotal);

  async function refreshAfterAction(requestId) {
    await dispatch(fetchRequests(listQuery())).unwrap();
    if (requestId) {
      await dispatch(fetchRequestDetail(requestId))
        .unwrap()
        .catch(() => {});
      await dispatch(fetchRequestComments(requestId))
        .unwrap()
        .catch(() => {});
    }
  }

  function handleTableAction(key, row) {
    if (key === "view") {
      navigate(`/requests/${row.requestId}`);
      return;
    }
    if (key === "escalate") {
      if (!isEscalateEligible(row)) return;
      setSelectedRequest(row);
      setShowEscalateModal(true);
      return;
    }
    setActionError(null);
    setActionModal({ open: true, type: key, row });
  }

  function closeEscalateModal() {
    setShowEscalateModal(false);
    setSelectedRequest(null);
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

      await apiClient.post(`/requests/${selectedRequest.requestId}/escalate`, {
        escalationReason: escalationReason.trim(),
      });

      const requestId = selectedRequest.requestId;
      setShowEscalateModal(false);
      setSelectedRequest(null);
      setEscalationReason("");
      await refreshAfterAction(requestId);
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
    if (!actionModal.row) return;
    const id = actionModal.row.requestId;
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
      setActionModal({ open: false, type: null, row: null });
      await refreshAfterAction(id);
    } catch (e) {
      setActionError(String(e));
    }
  }

  return (
    <div className="space-y-6 overflow-x-hidden">
      <div className="flex flex-wrap gap-2 border-b border-gray-200 pb-1">
        {visibleTabs.map((tab) => (
          <button
            key={tab.key}
            type="button"
            onClick={() =>
              dispatch(setActiveTab({ key: tab.key, status: tab.status }))
            }
            className={[
              "rounded-lg px-3 py-2 text-sm font-medium transition",
              activeTab === tab.key
                ? "bg-indigo-50 text-indigo-800 ring-1 ring-indigo-100"
                : `${tab.key === "escalated" ? "text-orange-600 hover:text-orange-600" : "text-gray-600"} hover:bg-gray-50 hover:text-gray-900`,
            ].join(" ")}
          >
            {tab.label}
          </button>
        ))}
      </div>

      <RequestFilters
        roles={roles}
        pageSize={pageSize}
        status={status}
        priority={priority}
        categoryId={categoryId}
        requestNumber={requestNumber}
        hideRejected={isManager}
        activeTab={activeTab}
        onPageSizeChange={(v) => dispatch(setPageSize(v))}
        onStatusChange={(v) => dispatch(setStatusFilter(v))}
        onPriorityChange={(v) => dispatch(setPriorityFilter(v))}
        onCategoryChange={(v) => dispatch(setCategoryFilter(v))}
        onRequestNumberChange={(v) => dispatch(setRequestNumberFilter(v))}
      />

      <RequestTable
        rows={filteredItems}
        loading={loading}
        roles={roles}
        currentUserId={currentUserId}
        activeTab={activeTab}
        onAction={handleTableAction}
        emptyStateText={
          activeTab === "escalated"
            ? "No escalated requests at the moment"
            : undefined
        }
      />

      <div className="flex flex-col items-center justify-between gap-3 border-t border-gray-200 pt-4 sm:flex-row">
        <p className="text-sm text-gray-600">
          {displayTotal > 0
            ? `Showing ${firstItem}-${lastItem} of ${displayTotal}`
            : "No requests"}
          {" · "}
          Page {page} of {safeTotalPages}
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
            disabled={page >= safeTotalPages}
            onClick={() => dispatch(setPage(page + 1))}
          >
            Next
          </Button>
        </div>
      </div>

      <RequestActionModal
        open={actionModal.open}
        actionType={actionModal.type}
        saving={mutating}
        serverError={actionError}
        onClose={() => {
          if (!mutating) setActionModal({ open: false, type: null, row: null });
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
            Request:{" "}
            {selectedRequest?.requestNumber || selectedRequest?.requestId} —{" "}
            {selectedRequest?.title}
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
