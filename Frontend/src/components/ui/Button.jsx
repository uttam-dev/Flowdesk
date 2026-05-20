const base =
  "inline-flex min-h-[44px] items-center justify-center gap-2 rounded-lg px-4 py-2 text-sm font-semibold whitespace-nowrap transition-all duration-200 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-offset-2 active:scale-[0.98] disabled:pointer-events-none disabled:cursor-not-allowed disabled:opacity-50";

const variants = {
  primary:
    "bg-emerald-600 text-white shadow-sm hover:bg-emerald-700 hover:shadow-md active:bg-emerald-800 focus-visible:ring-emerald-500",
  secondary:
    "border border-gray-300 bg-white text-gray-800 shadow-sm hover:bg-gray-50 hover:shadow-md active:bg-gray-100 focus-visible:ring-gray-400",
  danger:
    "bg-red-600 text-white shadow-sm hover:bg-red-700 hover:shadow-md active:bg-red-800 focus-visible:ring-red-500",
  ghost:
    "text-gray-600 shadow-none hover:bg-gray-100 active:bg-gray-200 focus-visible:ring-gray-300",
};

export function Button({
  children,
  className = "",
  variant = "primary",
  type = "button",
  disabled,
  loading,
  ...rest
}) {
  const v = variants[variant] ?? variants.primary;

  return (
    <button
      type={type}
      className={`${base} ${v} ${className} cursor-pointer`}
      disabled={disabled || loading}
      {...rest}
    >
      {loading ? (
        <>
          <span
            className="h-4 w-4 animate-spin rounded-full border-2 border-current border-t-transparent"
            aria-hidden
          />
          <span>{children}</span>
        </>
      ) : (
        children
      )}
    </button>
  );
}
