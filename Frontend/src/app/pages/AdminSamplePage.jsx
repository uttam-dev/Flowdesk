import { Link } from 'react-router-dom'
import { AppHeader } from '../../components/layout/AppHeader.jsx'
import { Button } from '../../components/ui/Button.jsx'

/** Example page guarded by `roles={['Admin']}` in the router — adjust role names to match your API. */
export function AdminSamplePage() {
  return (
    <div className="min-h-screen bg-gray-50 font-sans">
      <AppHeader
        title="Admin sample"
        subtitle="Restricted route for elevated roles"
      >
        <Link to="/" className="w-full sm:w-auto">
          <Button type="button" variant="secondary" className="w-full sm:w-auto">
            Back to dashboard
          </Button>
        </Link>
      </AppHeader>

      <main className="mx-auto max-w-7xl p-4 md:p-6">
        <div className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm transition-shadow duration-200 hover:shadow-md md:p-6">
          <p className="text-sm text-gray-600">
            You can only see this if your JWT user includes the{' '}
            <code className="rounded-md border border-gray-200 bg-gray-50 px-1.5 py-0.5 text-xs font-medium text-gray-800">
              Admin
            </code>{' '}
            role (see{' '}
            <code className="rounded-md border border-gray-200 bg-gray-50 px-1.5 py-0.5 text-xs font-medium text-gray-800">
              app/router.jsx
            </code>
            ).
          </p>
        </div>
      </main>
    </div>
  )
}
