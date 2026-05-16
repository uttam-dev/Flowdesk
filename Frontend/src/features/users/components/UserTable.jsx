import { Button } from '../../../components/ui/Button.jsx'
import { Table, TableHead, TableBody, TableRow, Th, Td } from '../../../components/ui/Table.jsx'

function fmtDate(v) {
  if (!v) return '—'
  try {
    const d = new Date(v)
    if (Number.isNaN(d.getTime())) return String(v)
    return d.toLocaleString()
  } catch {
    return String(v)
  }
}

export function UserTable({
  rows,
  loading,
  currentUserId,
  onEdit,
  onActivate,
  onDeactivate,
  onDelete,
}) {
  if (loading === 'pending' && !rows.length) {
    return (
      <div className="rounded-xl border border-gray-200 bg-white p-10 text-center text-sm text-gray-600 shadow-sm">
        Loading users…
      </div>
    )
  }

  return (
    <Table>
      <TableHead>
        <TableRow className="hover:bg-transparent">
          <Th>Full name</Th>
          <Th>Email</Th>
          <Th>Role</Th>
          <Th className="hidden md:table-cell">Manager</Th>
          <Th>Active</Th>
          <Th className="hidden lg:table-cell">Created on</Th>
          <Th className="hidden xl:table-cell">Updated on</Th>
          <Th className="text-right">Actions</Th>
        </TableRow>
      </TableHead>
      <TableBody>
        {rows.length === 0 ? (
          <TableRow>
            <Td colSpan={8} className="py-10 text-center text-gray-500">
              No users found.
            </Td>
          </TableRow>
        ) : (
          rows.map((row) => {
            const isSelf =
              currentUserId != null &&
              String(row.userId) === String(currentUserId)

            return (
              <TableRow key={row.userId}>
                <Td className="max-w-[10rem] truncate font-medium">
                  {row.fullName || '—'}
                </Td>
                <Td className="max-w-[12rem] truncate text-gray-600">
                  {row.email || '—'}
                </Td>
                <Td>{row.roleName || '—'}</Td>
                <Td className="hidden max-w-[10rem] truncate md:table-cell">
                  {row.managerName || '—'}
                </Td>
                <Td>
                  <span
                    className={
                      row.isActive
                        ? 'rounded-full bg-emerald-50 px-2 py-0.5 text-xs font-medium text-emerald-800 ring-1 ring-emerald-100'
                        : 'rounded-full bg-gray-100 px-2 py-0.5 text-xs font-medium text-gray-600 ring-1 ring-gray-200'
                    }
                  >
                    {row.isActive ? 'Active' : 'Inactive'}
                  </span>
                </Td>
                <Td className="hidden text-xs text-gray-600 lg:table-cell">
                  {fmtDate(row.createdOn)}
                </Td>
                <Td className="hidden text-xs text-gray-600 xl:table-cell">
                  {fmtDate(row.updatedOn)}
                </Td>
                <Td className="text-right">
                  <div className="flex flex-wrap justify-end gap-2">
                    <Button
                      type="button"
                      variant="secondary"
                      className="min-h-0 px-2 py-1.5 text-xs"
                      onClick={() => onEdit(row)}
                    >
                      Edit
                    </Button>
                    {row.isActive ? (
                      <Button
                        type="button"
                        variant="ghost"
                        className="min-h-0 px-2 py-1.5 text-xs text-amber-800 hover:bg-amber-50"
                        onClick={() => onDeactivate(row)}
                      >
                        Deactivate
                      </Button>
                    ) : (
                      <Button
                        type="button"
                        variant="ghost"
                        className="min-h-0 px-2 py-1.5 text-xs text-emerald-800 hover:bg-emerald-50"
                        onClick={() => onActivate(row)}
                      >
                        Activate
                      </Button>
                    )}
                    {!isSelf ? (
                      <Button
                        type="button"
                        variant="ghost"
                        className="min-h-0 px-2 py-1.5 text-xs text-red-700 hover:bg-red-50"
                        onClick={() => onDelete(row)}
                      >
                        Delete
                      </Button>
                    ) : null}
                  </div>
                </Td>
              </TableRow>
            )
          })
        )}
      </TableBody>
    </Table>
  )
}
