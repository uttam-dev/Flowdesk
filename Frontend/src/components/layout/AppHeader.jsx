import { useEffect, useRef, useState } from "react";
import { useSelector } from "react-redux";
import { Link } from "react-router-dom";
import { FlowDeskLogo } from "../brand/FlowDeskLogo.jsx";
import { selectAuthUser, selectRoleNames } from "../../features/auth/authSlice.js";
import {
  startSignalR,
  onRequestEvent, offRequestEvent,
  onStatusUpdatedEvent, offStatusUpdatedEvent,
  onAssignedEvent, offAssignedEvent,
} from "../../services/signalrService.js";

function timeAgo(date) {
  const diff = Math.floor((Date.now() - date.getTime()) / 1000);
  if (diff < 60) return "just now";
  if (diff < 3600) return `${Math.floor(diff / 60)}m ago`;
  return `${Math.floor(diff / 3600)}h ago`;
}

function buildNotification(event, payload, roles) {
  const hasRole = (r) => roles.some((role) => String(role).toLowerCase() === r.toLowerCase());
  const reqId = payload.requestNumber ?? payload.RequestNumber ?? payload.requestId ?? payload.RequestId;
  const suffix = reqId ? `#${reqId}` : "a request";

  if (event === "RequestUpdated") {
    if (hasRole("Admin"))
      return { title: "New request", message: `${suffix} submitted` };
    if (hasRole("Manager"))
      return { title: "New team request", message: `${suffix} needs attention` };
    if (hasRole("Employee"))
      return { title: "Request created", message: `${suffix} submitted` };
  }

  if (event === "RequestStatusUpdated") {
    const action = payload.action ?? payload.Action ?? payload.newStatus ?? payload.NewStatus ?? "";
    const map = {
      Approved: "Request approved",
      Rejected: "Request rejected",
      InProgress: "Work started",
      Resolved: "Request resolved",
      Escalated: "Request escalated",
    };
    if (map[action]) return { title: map[action], message: `Request ${suffix}` };
  }

  if (event === "RequestAssigned") {
    if (hasRole("Support"))
      return { title: "Assigned to you", message: `${suffix} added to your queue` };
    return { title: "Request assigned", message: `${suffix} assigned` };
  }

  return null;
}

export function AppHeader({
  title,
  subtitle,
  children,
  logoHref = "/",
  showLogo = true,
}) {
  const user = useSelector(selectAuthUser);
  const roles = useSelector(selectRoleNames);
  const [unreadCount, setUnreadCount] = useState(0);
  const [notifications, setNotifications] = useState([]);
  const [showDropdown, setShowDropdown] = useState(false);
  const dropdownRef = useRef(null);

  useEffect(() => {
    if (!user) return;

    startSignalR();

    const addNote = (event) => (payload) => {
      const note = buildNotification(event, payload, roles);
      if (!note) return;
      setNotifications((prev) => [
        { id: Date.now(), ...note, timestamp: new Date(), read: false },
        ...prev,
      ].slice(0, 20));
      setUnreadCount((c) => c + 1);
    };

    const h1 = addNote("RequestUpdated");
    const h2 = addNote("RequestStatusUpdated");
    const h3 = addNote("RequestAssigned");

    onRequestEvent(h1);
    onStatusUpdatedEvent(h2);
    onAssignedEvent(h3);

    return () => {
      offRequestEvent(h1);
      offStatusUpdatedEvent(h2);
      offAssignedEvent(h3);
    };
  }, [user, roles]);

  useEffect(() => {
    function handleClickOutside(e) {
      if (dropdownRef.current && !dropdownRef.current.contains(e.target)) {
        setShowDropdown(false);
      }
    }
    if (showDropdown) {
      document.addEventListener("mousedown", handleClickOutside);
    }
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [showDropdown]);

  return (
    <header className="sticky top-0 z-50 border-b border-gray-200 bg-white/95 shadow-sm backdrop-blur-md transition-shadow duration-200">
      <div className="mx-auto flex max-w-7xl flex-col gap-4 px-4 py-4 sm:px-4 md:flex-row md:items-center md:justify-between md:px-6 lg:px-8">
        <div className="flex min-w-0 flex-col gap-3 sm:flex-row sm:items-center sm:gap-5">
          {showLogo ? (
            <Link
              to={logoHref}
              className="inline-flex shrink-0 rounded-md outline-offset-2 transition-opacity duration-200 hover:opacity-90 focus-visible:outline focus-visible:outline-2 focus-visible:outline-emerald-600"
            >
              <FlowDeskLogo />
            </Link>
          ) : null}
          {title || subtitle ? (
            <div
              className={`min-w-0 ${showLogo ? "sm:border-l sm:border-gray-200 sm:pl-5" : ""}`}
            >
              {title ? (
                <h1 className="text-lg md:text-xl lg:text-2xl font-semibold tracking-tight text-gray-900">
                  {title}
                </h1>
              ) : null}
              {subtitle ? (
                <p className="mt-0.5 text-sm text-gray-600">{subtitle}</p>
              ) : null}
            </div>
          ) : null}
        </div>
        <div className="flex flex-wrap items-center gap-2 md:justify-end">
          <div className="relative" ref={dropdownRef}>
            <button
              type="button"
              onClick={() => {
                setShowDropdown((v) => !v);
                setUnreadCount(0);
                setNotifications((prev) => prev.map((n) => ({ ...n, read: true })));
              }}
              className="relative rounded-lg p-2 text-gray-600 hover:bg-gray-100 transition-colors cursor-pointer"
              aria-label="Notifications"
            >
              <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" d="M14.857 17.082a23.848 23.848 0 005.454-1.31A8.967 8.967 0 0118 9.75v-.7V9A6 6 0 006 9v.75a8.967 8.967 0 01-2.312 6.022c1.733.64 3.56 1.085 5.455 1.31m5.714 0a24.255 24.255 0 01-5.714 0m5.714 0a3 3 0 11-5.714 0" />
              </svg>
              {unreadCount > 0 && (
                <span className="absolute -right-0.5 -top-0.5 flex h-4 min-w-[16px] items-center justify-center rounded-full bg-red-500 px-1 text-[10px] font-bold leading-none text-white">
                  {unreadCount > 9 ? "9+" : unreadCount}
                </span>
              )}
            </button>

            {showDropdown && (
              <div className="absolute right-0 top-full mt-2 w-80 rounded-xl border border-gray-200 bg-white shadow-lg z-50">
                <div className="flex items-center justify-between border-b border-gray-100 px-4 py-3">
                  <span className="text-sm font-semibold text-gray-900">Notifications</span>
                  <button
                    type="button"
                    onClick={() => setNotifications([])}
                    className="text-xs text-gray-500 hover:text-gray-700 cursor-pointer"
                  >
                    Clear all
                  </button>
                </div>
                <div className="max-h-[400px] overflow-y-auto">
                  {notifications.length === 0 ? (
                    <p className="py-8 text-center text-sm text-gray-400">No notifications yet</p>
                  ) : (
                    notifications.map((n) => (
                      <div
                        key={n.id}
                        className={`px-4 py-3 border-b border-gray-50 last:border-b-0 ${
                          n.read ? "bg-white" : "bg-indigo-50/40"
                        }`}
                      >
                        <p className="text-sm font-medium text-gray-900">{n.title}</p>
                        <p className="text-sm text-gray-600">{n.message}</p>
                        <p className="mt-0.5 text-xs text-gray-400">{timeAgo(new Date(n.timestamp))}</p>
                      </div>
                    ))
                  )}
                </div>
              </div>
            )}
          </div>

          {children ? (
            <div className="flex flex-wrap items-center gap-2">{children}</div>
          ) : null}
        </div>
      </div>
    </header>
  );
}
