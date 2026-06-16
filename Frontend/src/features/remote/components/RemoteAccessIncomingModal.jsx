import { useState, useEffect, useRef } from "react";
import { createPortal } from "react-dom";
import { useDispatch, useSelector } from "react-redux";
import { Button } from "../../../components/ui/Button";
import {
  respondToRemoteSession,
  clearIncomingRequest,
  selectIncomingRequest,
  selectIsResponding,
} from "../remoteSlice.js";

const AUTO_REJECT_SECONDS = 60;

export default function RemoteAccessIncomingModal() {
  const dispatch = useDispatch();
  const incoming = useSelector(selectIncomingRequest);
  const isResponding = useSelector(selectIsResponding);

  const [showRejectInput, setShowRejectInput] = useState(false);
  const [rejectionReason, setRejectionReason] = useState("");
  const [countdown, setCountdown] = useState(AUTO_REJECT_SECONDS);

  const isOpen = !!incoming;

  const incomingRef = useRef(incoming);
  const rejectionReasonRef = useRef(rejectionReason);
  useEffect(() => {
    incomingRef.current = incoming;
  }, [incoming]);
  useEffect(() => {
    rejectionReasonRef.current = rejectionReason;
  }, [rejectionReason]);

  useEffect(() => {
    if (!isOpen) return;
    setCountdown(AUTO_REJECT_SECONDS);

    const timer = setInterval(() => {
      setCountdown((prev) => {
        if (prev <= 1) {
          clearInterval(timer);
          const cur = incomingRef.current;
          if (cur) {
            dispatch(
              respondToRemoteSession({
                sessionId: cur.sessionId,
                accepted: false,
                rejectionReason: "No response from user.",
              }),
            );
            dispatch(clearIncomingRequest());
          }
          return 0;
        }
        return prev - 1;
      });
    }, 1000);

    return () => clearInterval(timer);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isOpen]);

  useEffect(() => {
    if (!isOpen) {
      setShowRejectInput(false);
      setRejectionReason("");
    }
  }, [isOpen]);

  const handleAccept = async () => {
    await dispatch(
      respondToRemoteSession({
        sessionId: incoming.sessionId,
        accepted: true,
        rejectionReason: null,
      }),
    );
    dispatch(clearIncomingRequest());
  };

  const handleReject = async (autoReason) => {
    const reason = autoReason || rejectionReasonRef.current || "Rejected by user.";
    await dispatch(
      respondToRemoteSession({
        sessionId: incomingRef.current.sessionId,
        accepted: false,
        rejectionReason: reason,
      }),
    );
    dispatch(clearIncomingRequest());
  };

  if (!isOpen) return null;

  const modal = (
    <div className="fixed inset-0 z-[100000] flex items-end justify-center p-4 sm:items-center sm:p-6" role="dialog" aria-modal="true">
      <button
        type="button"
        className="absolute inset-0 bg-gray-900/40 backdrop-blur-[1px]"
        aria-label="Close dialog"
      />
      <div className="relative z-10 w-full max-w-lg rounded-xl border border-gray-200 bg-white shadow-lg">
        <div className="flex items-start justify-between gap-4 border-b border-gray-200 px-5 py-4">
          <h2 className="text-lg font-semibold tracking-tight text-gray-900">Remote Access Request</h2>
        </div>
        <div className="max-h-[min(70vh,32rem)] overflow-y-auto px-5 py-4 space-y-5">
          <div className="flex items-start gap-3 rounded-lg bg-blue-50 border border-blue-200 p-4">
            <div className="mt-0.5 shrink-0 text-blue-500">
              <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
              </svg>
            </div>
            <div>
              <p className="text-sm font-semibold text-blue-900">
                {incoming.supportName} is requesting remote access to your machine
              </p>
              <p className="mt-0.5 text-xs text-blue-700">{incoming.message}</p>
            </div>
          </div>

          <div className="space-y-1">
            <div className="flex justify-between text-xs text-gray-500">
              <span>Auto-rejecting in</span>
              <span className="font-medium text-red-500">{countdown}s</span>
            </div>
            <div className="h-1.5 w-full rounded-full bg-gray-200">
              <div className="h-1.5 rounded-full bg-red-400 transition-all duration-1000" style={{ width: `${(countdown / AUTO_REJECT_SECONDS) * 100}%` }} />
            </div>
          </div>

          {showRejectInput && (
            <div className="space-y-1">
              <label className="text-sm font-medium text-gray-700">
                Reason for rejection <span className="text-red-500">*</span>
              </label>
              <textarea
                placeholder="e.g. I am in a meeting right now"
                value={rejectionReason}
                onChange={(e) => setRejectionReason(e.target.value)}
                className="block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
                rows={2}
              />
            </div>
          )}

          <div className="flex justify-end gap-3 pt-1">
            {!showRejectInput ? (
              <>
                <Button variant="secondary" onClick={() => setShowRejectInput(true)} disabled={isResponding}>
                  Reject
                </Button>
                <Button onClick={handleAccept} disabled={isResponding} className="bg-green-600 hover:bg-green-700">
                  {isResponding ? "Accepting..." : "Accept"}
                </Button>
              </>
            ) : (
              <>
                <Button variant="secondary" onClick={() => setShowRejectInput(false)} disabled={isResponding}>
                  Back
                </Button>
                <Button onClick={() => handleReject()} disabled={isResponding || !rejectionReason.trim()} className="bg-red-600 hover:bg-red-700">
                  {isResponding ? "Rejecting..." : "Confirm Reject"}
                </Button>
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );

  return createPortal(modal, document.body);
}
