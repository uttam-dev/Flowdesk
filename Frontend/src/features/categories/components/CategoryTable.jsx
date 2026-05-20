import { Button } from "../../../components/ui/Button.jsx";
import {
  Table,
  TableHead,
  TableBody,
  TableRow,
  Th,
  Td,
} from "../../../components/ui/Table.jsx";

function fmtDate(value) {
  if (!value) return "-";
  try {
    const normalized = String(value).includes(" ")
      ? String(value).replace(" ", "T")
      : value;
    const date = new Date(normalized);
    if (Number.isNaN(date.getTime())) return String(value);
    return date.toLocaleString(undefined, {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  } catch {
    return String(value);
  }
}

export function CategoryTable({
  rows,
  loading,
  onEdit,
  onActivate,
  onDeactivate,
}) {
  if (loading === "pending" && !rows.length) {
    return (
      <div className="rounded-xl border border-gray-200 bg-white p-10 text-center text-sm text-gray-600 shadow-sm">
        Loading categories…
      </div>
    );
  }

  return (
    <Table>
      <TableHead>
        <TableRow className="hover:bg-transparent">
          <Th>Category ID</Th>
          <Th>Category name</Th>
          <Th>Approval required</Th>
          <Th>SLA Hours</Th>
          <Th>Active</Th>
          <Th>Created on</Th>
          <Th>Updated on</Th>
          <Th className="text-right">Actions</Th>
        </TableRow>
      </TableHead>
      <TableBody>
        {rows.length === 0 ? (
          <TableRow>
            <Td colSpan={8} className="py-10 text-center text-gray-500">
              No categories found.
            </Td>
          </TableRow>
        ) : (
          rows.map((row) => (
            <TableRow key={row.categoryId}>
              <Td className="font-mono text-xs">{row.categoryId}</Td>
              <Td className="max-w-[12rem] truncate font-medium">
                {row.categoryName}
              </Td>
              <Td>{row.isApprovalRequired ? "Yes" : "No"}</Td>
              <Td>{row.slaHours ?? "Not available"} hrs</Td>
              <Td>
                <span
                  className={
                    row.isActive
                      ? "rounded-full bg-emerald-50 px-2 py-0.5 text-xs font-medium text-emerald-800 ring-1 ring-emerald-100"
                      : "rounded-full bg-gray-100 px-2 py-0.5 text-xs font-medium text-gray-600 ring-1 ring-gray-200"
                  }
                >
                  {row.isActive ? "Active" : "Inactive"}
                </span>
              </Td>
              <Td className="text-xs text-gray-600">
                {fmtDate(row.createdOn)}
              </Td>
              <Td className="text-xs text-gray-600">
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
                </div>
              </Td>
            </TableRow>
          ))
        )}
      </TableBody>
    </Table>
  );
}
