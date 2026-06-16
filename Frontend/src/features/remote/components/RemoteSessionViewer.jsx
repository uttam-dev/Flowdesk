import { useRef, useState, useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import { useWebRTC } from "../useWebRTC";
import RemoteControlOverlay from "./RemoteControlOverlay";
import {
  endRemoteSession,
  selectCurrentSession,
  selectSessionStatus,
  selectIsEnding,
  closeViewer,
} from "../remoteSlice";

/**
 * Full-screen remote session viewer — support side only.
 * Shown when support accepts that target user accepted the request.
 * Contains: live video stream, control overlay, top bar, end session flow.
 */
export default function RemoteSessionViewer() {
  const dispatch = useDispatch();
  const session = useSelector(selectCurrentSession);
  const sessionStatus = useSelector(selectSessionStatus);
  const isEnding = useSelector(selectIsEnding);

  const videoRef = useRef(null);
  const sessionId = session?.remoteSessionId || session?.id;
  const targetUserId = session?.targetUserId;
  useWebRTC(sessionId, targetUserId, videoRef);

  const [showEndModal, setShowEndModal] = useState(false);
  const [resolutionNotes, setResolutionNotes] = useState("");
  const [resolveRequest, setResolveRequest] = useState(false);
  const [elapsedSeconds, setElapsedSeconds] = useState(0);

  // ── Session timer ─────────────────────────────────────────────────────────
  useEffect(() => {
    if (sessionStatus !== "active") return;
    const timer = setInterval(() => setElapsedSeconds((s) => s + 1), 1000);
    return () => clearInterval(timer);
  }, [sessionStatus]);

  const formatTime = (secs) => {
    const h = Math.floor(secs / 3600)
      .toString()
      .padStart(2, "0");
    const m = Math.floor((secs % 3600) / 60)
      .toString()
      .padStart(2, "0");
    const s = (secs % 60).toString().padStart(2, "0");
    return `${h}:${m}:${s}`;
  };

  const handleEndSession = async () => {
    await dispatch(
      endRemoteSession({
        sessionId: sessionId,
        resolutionNotes,
        resolveRequest,
      }),
    );
    setShowEndModal(false);
    dispatch(closeViewer());
  };

  if (!session) return null;

  const isLive = sessionStatus === "active";

  return (
    <div className="fixed inset-0 z-50 flex flex-col bg-gray-950">
      {/* ── Top bar ──────────────────────────────────────────────────────── */}
      <div className="flex items-center justify-between bg-gray-900 px-4 py-2 border-b border-gray-700">
        <div className="flex items-center gap-3">
          {/* Live indicator */}
          <span className="flex items-center gap-1.5">
            <span
              className={`h-2.5 w-2.5 rounded-full ${isLive ? "bg-green-400 animate-pulse" : "bg-yellow-400"}`}
            />
            <span className="text-xs font-medium text-white">
              {isLive ? "LIVE" : "CONNECTING..."}
            </span>
          </span>

          <span className="text-gray-500 text-xs">|</span>

          <span className="text-sm text-gray-300">
            <span className="text-gray-500">Request</span>{" "}
            <span className="font-medium text-white">#{session.requestId}</span>
          </span>

          <span className="text-gray-500 text-xs">|</span>

          <span className="text-sm text-gray-300">
            <span className="text-gray-500">User</span>{" "}
            <span className="font-medium text-white">
              {session.targetUserName}
            </span>
          </span>
        </div>

        <div className="flex items-center gap-4">
          {/* Timer */}
          {isLive && (
            <span className="font-mono text-sm text-green-400">
              {formatTime(elapsedSeconds)}
            </span>
          )}

          {/* End session button */}
          <button
            onClick={() => setShowEndModal(true)}
            disabled={isEnding}
            className="rounded-md bg-red-600 px-4 py-1.5 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-50 transition-colors"
          >
            {isEnding ? "Ending..." : "End Session"}
          </button>
        </div>
      </div>

      {/* ── Video area ───────────────────────────────────────────────────── */}
      <div className="relative flex-1 overflow-hidden">
        {/* Connecting overlay */}
        {!isLive && (
          <div className="absolute inset-0 z-20 flex flex-col items-center justify-center gap-3 bg-gray-950">
            <div className="h-10 w-10 animate-spin rounded-full border-4 border-blue-500 border-t-transparent" />
            <p className="text-sm text-gray-400">
              Establishing secure connection...
            </p>
          </div>
        )}

        {/* Live screen stream */}
        <video
          ref={videoRef}
          autoPlay
          playsInline
          muted
          className="h-full w-full object-contain"
        />

        {/* Control overlay — sits on top of video */}
        {isLive && (
          <RemoteControlOverlay sessionId={sessionId} videoRef={videoRef} />
        )}
      </div>

      {/* ── End Session Modal ─────────────────────────────────────────────── */}
      {showEndModal && (
        <div className="fixed inset-0 z-60 flex items-center justify-center bg-black/70">
          <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-2xl space-y-5 mx-4">
            <h3 className="text-lg font-semibold text-gray-900">
              End Remote Session
            </h3>

            {/* Resolution notes */}
            <div className="space-y-1">
              <label className="text-sm font-medium text-gray-700">
                What was done? <span className="text-gray-400">(optional)</span>
              </label>
              <textarea
                rows={4}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none"
                placeholder="e.g. Installed the required software, cleared temp files..."
                value={resolutionNotes}
                onChange={(e) => setResolutionNotes(e.target.value)}
              />
            </div>

            {/* Resolve request toggle */}
            <label className="flex items-center gap-3 cursor-pointer select-none">
              <input
                type="checkbox"
                checked={resolveRequest}
                onChange={(e) => setResolveRequest(e.target.checked)}
                className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
              />
              <span className="text-sm text-gray-700">
                Mark request as{" "}
                <span className="font-medium text-green-700">Resolved</span>
              </span>
            </label>

            {/* Actions */}
            <div className="flex justify-end gap-3 pt-1">
              <button
                onClick={() => setShowEndModal(false)}
                disabled={isEnding}
                className="rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-700 hover:bg-gray-50 disabled:opacity-50"
              >
                Cancel
              </button>
              <button
                onClick={handleEndSession}
                disabled={isEnding}
                className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-50"
              >
                {isEnding ? "Ending..." : "End Session"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
