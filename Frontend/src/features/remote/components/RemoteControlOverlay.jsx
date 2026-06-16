import { useCallback, useEffect } from "react";
import { useSelector } from "react-redux";
import { remoteSignalR } from "../../../services/signalrService";
import { selectSessionStatus } from "../remoteSlice";

/**
 * Transparent overlay placed on top of the <video> element.
 * Captures all mouse and keyboard events from support
 * and forwards them to the Electron agent via SignalR.
 *
 * Props:
 *   sessionId   — int
 *   videoRef    — ref to the <video> element (used to compute relative coords)
 */
export default function RemoteControlOverlay({ sessionId, videoRef }) {
  const sessionStatus = useSelector(selectSessionStatus);
  const isActive = sessionStatus === "active";

  // ── Compute relative coords (0.0–1.0) from absolute mouse event ──────────
  const getRelativeCoords = useCallback(
    (e) => {
      const rect = videoRef.current?.getBoundingClientRect();
      if (!rect) return { x: 0, y: 0 };
      return {
        x: (e.clientX - rect.left) / rect.width,
        y: (e.clientY - rect.top) / rect.height,
      };
    },
    [videoRef],
  );

  // ── Mouse move ────────────────────────────────────────────────────────────
  const handleMouseMove = useCallback(
    (e) => {
      if (!isActive) return;
      const { x, y } = getRelativeCoords(e);
      remoteSignalR.sendControlEvent(sessionId, { type: "mousemove", x, y });
    },
    [isActive, sessionId, getRelativeCoords],
  );

  // ── Mouse click (left & right) ────────────────────────────────────────────
  const handleClick = useCallback(
    (e) => {
      if (!isActive) return;
      e.preventDefault();
      const { x, y } = getRelativeCoords(e);
      const button = e.button === 2 ? "right" : "left";
      const isDouble = e.detail === 2;
      remoteSignalR.sendControlEvent(sessionId, {
        type: "mouseclick",
        x,
        y,
        button,
        isDoubleClick: isDouble,
      });
    },
    [isActive, sessionId, getRelativeCoords],
  );

  // ── Scroll ────────────────────────────────────────────────────────────────
  const handleWheel = useCallback(
    (e) => {
      if (!isActive) return;
      e.preventDefault();
      const { x, y } = getRelativeCoords(e);
      const direction = e.deltaY < 0 ? "up" : "down";
      remoteSignalR.sendControlEvent(sessionId, {
        type: "scroll",
        x,
        y,
        scrollDirection: direction,
      });
    },
    [isActive, sessionId, getRelativeCoords],
  );

  // ── Keyboard ──────────────────────────────────────────────────────────────
  const handleKeyDown = useCallback(
    (e) => {
      if (!isActive) return;
      e.preventDefault(); // block browser shortcuts (Ctrl+T, Ctrl+W etc.)
      remoteSignalR.sendControlEvent(sessionId, {
        type: "keypress",
        key: e.key.toLowerCase(),
        modifier: e.ctrlKey
          ? "ctrl"
          : e.altKey
            ? "alt"
            : e.shiftKey
              ? "shift"
              : e.metaKey
                ? "command"
                : null,
      });
    },
    [isActive, sessionId],
  );

  // ── Attach keyboard listener to window while session is active ────────────
  useEffect(() => {
    if (!isActive) return;
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [isActive, handleKeyDown]);

  return (
    <div
      className="absolute inset-0 z-10"
      style={{ cursor: isActive ? "none" : "default" }}
      onMouseMove={handleMouseMove}
      onClick={handleClick}
      onContextMenu={handleClick}
      onWheel={handleWheel}
      // tabIndex needed so the div can receive focus for keyboard events
      tabIndex={0}
      aria-label="Remote control overlay"
    />
  );
}
