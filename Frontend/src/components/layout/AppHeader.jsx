import { Link } from 'react-router-dom'
import { FlowDeskLogo } from '../brand/FlowDeskLogo.jsx'

/**
 * Sticky app header: logo left, title stack, actions right (MasterPrompt-2).
 */
export function AppHeader({
  title,
  subtitle,
  children,
  logoHref = '/',
  showLogo = true,
}) {
  return (
    <header className="sticky top-0 z-50 border-b border-gray-200 bg-white/95 shadow-sm backdrop-blur-md transition-shadow duration-200">
      <div className="mx-auto flex max-w-7xl flex-col gap-4 p-4 md:flex-row md:items-center md:justify-between md:p-6">
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
              className={`min-w-0 ${showLogo ? 'sm:border-l sm:border-gray-200 sm:pl-5' : ''}`}
            >
              {title ? (
                <h1 className="text-xl font-semibold tracking-tight text-gray-900">
                  {title}
                </h1>
              ) : null}
              {subtitle ? (
                <p className="mt-0.5 text-sm text-gray-600">{subtitle}</p>
              ) : null}
            </div>
          ) : null}
        </div>
        {children ? (
          <div className="flex flex-wrap items-center gap-2 md:justify-end">
            {children}
          </div>
        ) : null}
      </div>
    </header>
  )
}
