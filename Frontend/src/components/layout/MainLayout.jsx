import { useState } from "react";
import { useLocation, Outlet } from "react-router-dom";
import { Header } from "./Header.jsx";
import { Sidebar } from "./Sidebar.jsx";
import { ChatWidget } from "../../features/chat/components/ChatWidget.jsx";

export function MainLayout() {
  const location = useLocation();
  const [mobileOpen, setMobileOpen] = useState(false);
  const [collapsed, setCollapsed] = useState(false);

  const widthClass = collapsed ? "w-64 lg:w-[4.5rem]" : "w-64 lg:w-64";

  return (
    <div className="flex h-screen overflow-hidden bg-gray-50 font-sans">
      {mobileOpen ? (
        <button
          type="button"
          aria-label="Close menu"
          className="fixed inset-0 z-40 bg-gray-900/40 backdrop-blur-[1px] transition-opacity duration-200 lg:hidden"
          onClick={() => setMobileOpen(false)}
        />
      ) : null}

      <aside
        id="app-sidebar"
        className={[
          "fixed inset-y-0 left-0 z-50 h-screen overflow-y-auto border-r border-gray-200 bg-white shadow-lg transition-all duration-200 ease-out lg:static lg:z-0 lg:shadow-none",
          widthClass,
          mobileOpen ? "translate-x-0" : "-translate-x-full lg:translate-x-0",
        ].join(" ")}
      >
        <Sidebar
          collapsed={collapsed}
          mobile={mobileOpen}
          onNavigate={() => setMobileOpen(false)}
        />
      </aside>

      <div className="flex min-w-0 flex-1 flex-col">
        <Header
          pathname={location.pathname}
          onMenuClick={() => setMobileOpen((o) => !o)}
          onCollapseClick={() => setCollapsed((c) => !c)}
          collapsed={collapsed}
        />
        <main className="mx-auto w-full max-w-7xl flex-1 overflow-y-auto px-4 py-4 sm:px-4 md:px-6 lg:px-8">
          <Outlet />
        </main>
      </div>

      <ChatWidget />
    </div>
  );
}
