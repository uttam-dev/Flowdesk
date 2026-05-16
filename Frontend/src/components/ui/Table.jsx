export function Table({ children, className = '' }) {
  return (
    <div className="overflow-x-auto rounded-xl border border-gray-200 bg-white shadow-sm transition-shadow duration-200 hover:shadow-md">
      <table className={`min-w-full divide-y divide-gray-200 text-left text-sm ${className}`}>
        {children}
      </table>
    </div>
  )
}

export function TableHead({ children }) {
  return <thead className="bg-gray-50">{children}</thead>
}

export function TableBody({ children }) {
  return <tbody className="divide-y divide-gray-200 bg-white">{children}</tbody>
}

export function TableRow({ children, className = '' }) {
  return <tr className={`transition-colors duration-150 hover:bg-gray-50/80 ${className}`}>{children}</tr>
}

export function Th({ children, className = '' }) {
  return (
    <th
      scope="col"
      className={`whitespace-nowrap px-4 py-3 text-xs font-semibold uppercase tracking-wide text-gray-600 ${className}`}
    >
      {children}
    </th>
  )
}

export function Td({ children, className = '', ...rest }) {
  return (
    <td className={`whitespace-nowrap px-4 py-3 text-gray-800 ${className}`} {...rest}>
      {children}
    </td>
  )
}
