export function Input({
  id,
  label,
  error,
  className = '',
  inputClassName = '',
  ...rest
}) {
  return (
    <div className={`flex w-full flex-col gap-1.5 text-left ${className}`}>
      {label ? (
        <label htmlFor={id} className="text-sm font-medium text-gray-700">
          {label}
        </label>
      ) : null}
      <input
        id={id}
        className={`w-full rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm transition-all duration-200 placeholder:text-gray-400 hover:border-gray-400 focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500 disabled:cursor-not-allowed disabled:bg-gray-50 disabled:text-gray-500 disabled:hover:border-gray-300 ${error ? 'border-red-500 focus:border-red-500 focus:ring-red-500' : ''} ${inputClassName}`}
        aria-invalid={Boolean(error)}
        aria-describedby={error ? `${id}-error` : undefined}
        {...rest}
      />
      {error ? (
        <p id={`${id}-error`} className="mt-1 text-xs text-red-500" role="alert">
          {error}
        </p>
      ) : null}
    </div>
  )
}
