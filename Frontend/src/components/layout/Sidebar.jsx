import { useDispatch, useSelector } from "react-redux";
import { NavLink } from "react-router-dom";
import { FlowDeskLogo } from "../brand/FlowDeskLogo.jsx";
import { Button } from "../ui/Button.jsx";
import {
  logout,
  selectAuthUser,
  selectRoleNames,
} from "../../features/auth/authSlice.js";

function isAdminRole(roles) {
  return roles.some((r) => String(r).toLowerCase() === "admin");
}

function navClass({ isActive }) {
  return [
    "flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-all duration-200",
    isActive
      ? "bg-emerald-50 text-emerald-900 ring-1 ring-inset ring-emerald-100"
      : "text-gray-700 hover:bg-gray-50 hover:text-gray-900",
  ].join(" ");
}

export function Sidebar({ collapsed, onNavigate, mobile }) {
  const dispatch = useDispatch();
  const user = useSelector(selectAuthUser);
  const roles = useSelector(selectRoleNames);
  const isAdmin = isAdminRole(roles);
  const hideText = collapsed && !mobile;

  return (
    <div className="flex h-full flex-col border-r border-gray-200 bg-white">
      <div className="border-b border-gray-100 px-3 py-4">
        <NavLink
          to="/"
          end
          onClick={onNavigate}
          className="flex justify-center overflow-hidden"
          aria-label="FlowDesk dashboard"
        >
          <span
            aria-hidden={hideText}
            className={[
              "inline-flex origin-left transition-all duration-300 ease-in-out",
              hideText
                ? "max-w-0 scale-95 opacity-0"
                : "max-w-40 scale-100 opacity-100",
            ].join(" ")}
          >
            <FlowDeskLogo />
          </span>
        </NavLink>
      </div>

      <nav
        className="flex-1 space-y-1 overflow-y-auto p-2"
        aria-label="Sidebar"
      >
        <NavLink to="/" end onClick={onNavigate} className={navClass}>
          <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-gray-100 text-gray-600 ring-1 ring-gray-200/80">
            <svg
              className="h-5 w-5"
              fill="none"
              viewBox="0 0 24 24"
              strokeWidth={1.5}
              stroke="currentColor"
              aria-hidden
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M2.25 12l8.954-8.955c.44-.439 1.152-.439 1.591 0L21.75 12M4.5 9.75v10.125c0 .621.504 1.125 1.125 1.125H9.75v-4.875c0-.621.504-1.125 1.125-1.125h2.25c.621 0 1.125.504 1.125 1.125V21h4.125c.621 0 1.125-.504 1.125-1.125V9.75M8.25 21h8.25"
              />
            </svg>
          </span>
          <span className={hideText ? "sr-only" : "truncate"}>Dashboard</span>
        </NavLink>

        {isAdmin ? (
          <NavLink to="/categories" onClick={onNavigate} className={navClass}>
            <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-gray-100 text-gray-600 ring-1 ring-gray-200/80">
              <svg
                className="h-5 w-5"
                fill="none"
                viewBox="0 0 24 24"
                strokeWidth={1.5}
                stroke="currentColor"
                aria-hidden
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M2.25 7.125A2.25 2.25 0 014.5 4.875h15a2.25 2.25 0 012.25 2.25v9.75a2.25 2.25 0 01-2.25 2.25h-15a2.25 2.25 0 01-2.25-2.25v-9.75zM9 9h6"
                />
              </svg>
            </span>
            <span className={hideText ? "sr-only" : "truncate"}>
              Categories
            </span>
          </NavLink>
        ) : null}

        {isAdmin ? (
          <NavLink to="/admin-sample" onClick={onNavigate} className={navClass}>
            <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-gray-100 text-gray-600 ring-1 ring-gray-200/80">
              <svg
                className="h-5 w-5"
                fill="none"
                viewBox="0 0 24 24"
                strokeWidth={1.5}
                stroke="currentColor"
                aria-hidden
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M9 12.75L11.25 15 15 9.75m-3-7.036A11.959 11.959 0 013.598 6 11.99 11.99 0 003 9.749c0 5.592 3.824 10.29 9 11.623 5.176-1.332 9-6.03 9-11.622 0-1.31-.21-2.571-.598-3.751h-.152c-3.196 0-6.1-1.248-8.25-3.285z"
                />
              </svg>
            </span>
            <span className={hideText ? "sr-only" : "truncate"}>
              Admin sample
            </span>
          </NavLink>
        ) : null}
      </nav>

      <div className="border-t border-gray-100 p-2">
        <p
          className={`truncate px-2 text-xs font-medium text-gray-500 ${hideText ? "sr-only" : ""}`}
        >
          Signed in
        </p>
        <p
          className={`truncate px-2 text-sm font-semibold text-gray-900 ${hideText ? "sr-only" : ""}`}
        >
          {user?.name || user?.email || "User"}
        </p>
        <Button
          type="button"
          variant="secondary"
          className="mt-2 inline-flex w-full items-center justify-center gap-2"
          onClick={() => dispatch(logout())}
        >
          <svg
            className="h-4 w-4 shrink-0"
            fill="none"
            viewBox="0 0 24 24"
            strokeWidth={1.5}
            stroke="currentColor"
            aria-hidden
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              d="M15.75 9V5.25A2.25 2.25 0 0013.5 3h-6a2.25 2.25 0 00-2.25 2.25v13.5A2.25 2.25 0 007.5 21h6a2.25 2.25 0 002.25-2.25V15M18 12H9"
            />
          </svg>
          <span className={hideText ? "sr-only" : ""}>Sign out</span>
        </Button>
      </div>
    </div>
  );
}
