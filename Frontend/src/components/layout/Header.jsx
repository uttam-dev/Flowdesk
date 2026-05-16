function titleForPath(pathname) {
  if (pathname.startsWith('/categories')) return 'Categories'
  if (pathname.startsWith('/users')) return 'Users'
  if (pathname.startsWith('/admin-sample')) return 'Admin sample'
  return 'Dashboard'
}

export function Header({
  title,
  pathname,
  onMenuClick,
  onCollapseClick,
  collapsed,
}) {
  const resolved = title ?? titleForPath(pathname ?? '/')

  return (
    <header className="sticky top-0 z-30 border-b border-gray-200 bg-white/95 shadow-sm backdrop-blur-md transition-shadow duration-200">
      <div className="flex items-center gap-3 px-4 py-3 md:px-6">
        <button
          type="button"
          className="inline-flex min-h-[44px] min-w-[44px] items-center justify-center rounded-lg border border-gray-200 bg-white text-gray-800 shadow-sm transition-all duration-200 hover:bg-gray-50 hover:shadow-md active:scale-[0.98] lg:hidden"
          aria-label="Open navigation menu"
          onClick={onMenuClick}
        >
          <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" aria-hidden>
            <path strokeLinecap="round" strokeLinejoin="round" d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25h16.5" />
          </svg>
        </button>

        <button
          type="button"
          className="hidden min-h-[44px] min-w-[44px] items-center justify-center rounded-lg border border-gray-200 bg-white text-gray-800 shadow-sm transition-all duration-200 hover:bg-gray-50 hover:shadow-md active:scale-[0.98] lg:inline-flex"
          aria-label={collapsed ? 'Expand sidebar' : 'Collapse sidebar'}
          aria-pressed={collapsed}
          onClick={onCollapseClick}
        >
          <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" aria-hidden>
            {collapsed ? (
              <path strokeLinecap="round" strokeLinejoin="round" d="M11.25 4.5l7.5 7.5-7.5 7.5M18.75 12H3" />
            ) : (
              <path strokeLinecap="round" strokeLinejoin="round" d="M18.75 19.5l-7.5-7.5 7.5-7.5M12 12H3" />
            )}
          </svg>
        </button>

        <div className="min-w-0 flex-1">
          <p className="text-xs font-semibold uppercase tracking-wide text-emerald-700">
            FlowDesk
          </p>
          <h1 className="text-xl font-semibold tracking-tight text-gray-900">
            {resolved}
          </h1>
        </div>
      </div>
    </header>
  )
}
