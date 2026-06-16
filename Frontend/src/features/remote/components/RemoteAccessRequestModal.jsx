import { useDispatch, useSelector } from "react-redux";
import { createPortal } from "react-dom";
import { Modal } from "../../../components/ui/Modal";
import { Button } from "../../../components/ui/Button";
import {
  initiateRemoteSession,
  selectIsInitiating,
  selectSessionStatus,
} from "../remoteSlice";

/**
 * Support-side modal.
 * Shown when support clicks "Request Remote Access" on a request.
 *
 * Props:
 *   isOpen      — bool
 *   onClose     — fn
 *   requestId   — int
 *   requestTitle — string
 *   targetUserName — string
 */
export default function RemoteAccessRequestModal({
  isOpen,
  onClose,
  requestId,
  requestNumber,
  requestTitle,
  targetUserName,
}) {
  const dispatch = useDispatch();
  const isInitiating = useSelector(selectIsInitiating);
  const sessionStatus = useSelector(selectSessionStatus);

  const isPending = sessionStatus === "pending";

  const handleRequest = async () => {
    await dispatch(initiateRemoteSession(requestId));
    // Modal stays open showing "waiting" state — closes when accepted/rejected via SignalR
  };

  if (!isOpen) return null;

  return createPortal(
    <Modal
      open={isOpen}
      onClose={!isPending ? onClose : undefined}
      title="Request Remote Access"
    >
      <div className="space-y-4">
        {!isPending ? (
          <>
            {/* Confirm state */}
            <div className="rounded-lg bg-amber-50 border border-amber-200 p-4">
              <p className="text-sm text-amber-800">
                You are requesting remote access to{" "}
                <span className="font-semibold">{targetUserName}</span>'s
                machine for:
              </p>
              <p className="mt-1 text-sm font-medium text-amber-900">
                #{requestNumber} - {requestTitle}
              </p>
            </div>

            <p className="text-sm text-gray-600">
              The user will receive a popup asking them to accept or reject. You
              will only gain access if they accept.
            </p>

            <div className="flex justify-end gap-3 pt-2">
              <Button
                variant="secondary"
                onClick={onClose}
                disabled={isInitiating}
              >
                Cancel
              </Button>
              <Button onClick={handleRequest} disabled={isInitiating}>
                {isInitiating ? "Sending..." : "Send Request"}
              </Button>
            </div>
          </>
        ) : (
          <>
            {/* Waiting state */}
            <div className="flex flex-col items-center gap-4 py-6">
              <div className="h-10 w-10 animate-spin rounded-full border-4 border-blue-500 border-t-transparent" />
              <p className="text-sm text-gray-600 text-center">
                Waiting for{" "}
                <span className="font-semibold">{targetUserName}</span> to
                accept the remote access request...
              </p>
              <p className="text-xs text-gray-400">
                They will see a popup on their screen.
              </p>
            </div>

            <div className="flex justify-end">
              <Button variant="secondary" onClick={onClose}>
                Cancel Request
              </Button>
            </div>
          </>
        )}
      </div>
    </Modal>,
    document.body,
  );
}
