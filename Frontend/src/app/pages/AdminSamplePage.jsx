/** Example page for elevated roles — adjust copy as needed. */
export function AdminSamplePage() {
  return (
    <div className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm transition-shadow duration-200 hover:shadow-md md:p-6">
      <h2 className="text-xl font-semibold tracking-tight text-gray-900">
        Admin sample
      </h2>
      <p className="mt-2 text-sm leading-relaxed text-gray-600">
        This route is restricted to users with the{' '}
        <code className="rounded-md border border-gray-200 bg-gray-50 px-1.5 py-0.5 text-xs font-medium text-gray-800">
          Admin
        </code>{' '}
        role (see <code className="rounded-md border border-gray-200 bg-gray-50 px-1.5 py-0.5 text-xs font-medium text-gray-800">app/router.jsx</code>
        ).
      </p>
    </div>
  )
}
