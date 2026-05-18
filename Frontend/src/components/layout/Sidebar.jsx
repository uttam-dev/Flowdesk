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

function canAccessRequests(roles) {
  const allowed = new Set(["employee", "manager", "admin", "support"]);
  return roles.some((r) => allowed.has(String(r).toLowerCase()));
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
  const showRequests = canAccessRequests(roles);
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

        {showRequests ? (
          <NavLink to="/requests" onClick={onNavigate} className={navClass}>
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
                  d="M9 12h3.75M9 15h3.75M9 18h3.75m3 .75H18a2.25 2.25 0 002.25-2.25V6.108c0-1.135-.845-2.098-1.976-2.192a48.424 48.424 0 00-1.123-.08m-5.801 0c-.065.21-.123.433-.177.664-.191.707-.291 1.27-.291 1.949V19.5a2.25 2.25 0 01-2.25 2.25H6.75A2.25 2.25 0 014.5 19.5V6.75a2.25 2.25 0 012.25-2.25h.75m8.25 3v1.5m0 0V6.75m0 3h-3.375"
                />
              </svg>
            </span>
            <span className={hideText ? "sr-only" : "truncate"}>
              Requests
            </span>
          </NavLink>
        ) : null}

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
          <NavLink to="/users" onClick={onNavigate} className={navClass}>
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
                  d="M15 19.128a9.38 9.38 0 002.625.372 9.337 9.337 0 004.121-.952 4.125 4.125 0 00-7.533-2.493M15 19.128v-.003c0-1.113-.285-2.16-.786-3.07M15 19.128v.106A12.318 12.318 0 018.624 21c-2.331 0-4.512-.645-6.374-1.766l-.001-.109a6.375 6.375 0 0111.964-3.07M12 6.375a3.375 3.375 0 11-6.75 0 3.375 3.375 0 016.75 0zm8.25 2.25a2.625 2.625 0 11-5.25 0 2.625 2.625 0 015.25 0z"
                />
              </svg>
            </span>
            <span className={hideText ? "sr-only" : "truncate"}>
              Users
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
