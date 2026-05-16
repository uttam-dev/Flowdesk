import { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { toast } from "sonner";
import { Button } from "../../../components/ui/Button.jsx";
import { ConfirmationModal } from "../../../components/ui/ConfirmationModal.jsx";
import { CategoryTable } from "../components/CategoryTable.jsx";
import { CategoryFormModal } from "../components/CategoryFormModal.jsx";
import {
  activateCategory,
  clearCategoryError,
  createCategory,
  deactivateCategory,
  fetchCategories,
  selectCategoryError,
  selectCategoryIsActive,
  selectCategoryList,
  selectCategoryLoading,
  selectCategoryMutationLoading,
  selectCategoryPage,
  selectCategoryPageSize,
  selectCategoryTotal,
  selectCategoryTotalPages,
  setIsActiveFilter,
  setPage,
  setPageSize,
  updateCategory,
} from "../categorySlice.js";

export function CategoryPage() {
  const dispatch = useDispatch();
  const items = useSelector(selectCategoryList);
  const loading = useSelector(selectCategoryLoading);
  const error = useSelector(selectCategoryError);
  const page = useSelector(selectCategoryPage);
  const pageSize = useSelector(selectCategoryPageSize);
  const total = useSelector(selectCategoryTotal);
  const totalPages = useSelector(selectCategoryTotalPages);
  const isActive = useSelector(selectCategoryIsActive);
  const mutating = useSelector(selectCategoryMutationLoading);

  const [formOpen, setFormOpen] = useState(false);
  const [formMode, setFormMode] = useState("add");
  const [editing, setEditing] = useState(null);
  const [formKey, setFormKey] = useState(0);
  const [formSaving, setFormSaving] = useState(false);
  const [formServerError, setFormServerError] = useState(null);

  const [confirm, setConfirm] = useState({
    open: false,
    type: null,
    row: null,
  });
  const [confirmLoading, setConfirmLoading] = useState(false);

  useEffect(() => {
    dispatch(fetchCategories({ page, pageSize, isActive }));
  }, [dispatch, page, pageSize, isActive]);

  useEffect(() => {
    if (error) {
      toast.error(error);
      dispatch(clearCategoryError());
    }
  }, [error, dispatch]);

  const safeTotalPages = Math.max(1, totalPages || 1);
  const firstItem = total === 0 ? 0 : (page - 1) * pageSize + 1;
  const lastItem = Math.min(page * pageSize, total);
  const existingNames = items.map((i) => i.categoryName);

  function currentQuery() {
    return { page, pageSize, isActive };
  }

  function openAdd() {
    setFormKey((k) => k + 1);
    setFormMode("add");
    setEditing(null);
    setFormServerError(null);
    setFormOpen(true);
  }

  function openEdit(row) {
    setFormKey((k) => k + 1);
    setFormMode("edit");
    setEditing(row);
    setFormServerError(null);
    setFormOpen(true);
  }

  async function handleFormSubmit(values) {
    setFormSaving(true);
    setFormServerError(null);
    try {
      if (formMode === "add") {
        await dispatch(createCategory(values)).unwrap();
        toast.success("Category created");
      } else if (editing) {
        await dispatch(
          updateCategory({ id: editing.categoryId, ...values }),
        ).unwrap();
        toast.success("Category updated");
      }
      setFormOpen(false);
      await dispatch(fetchCategories(currentQuery())).unwrap();
    } catch (e) {
      setFormServerError(String(e));
    } finally {
      setFormSaving(false);
    }
  }

  function askActivate(row) {
    setConfirm({ open: true, type: "activate", row });
  }

  function askDeactivate(row) {
    setConfirm({ open: true, type: "deactivate", row });
  }

  async function runConfirm() {
    if (!confirm.row) return;
    setConfirmLoading(true);
    try {
      if (confirm.type === "activate") {
        await dispatch(activateCategory(confirm.row.categoryId)).unwrap();
        toast.success("Category activated");
      } else {
        await dispatch(deactivateCategory(confirm.row.categoryId)).unwrap();
        toast.success("Category deactivated");
      }
      setConfirm({ open: false, type: null, row: null });
      await dispatch(fetchCategories(currentQuery())).unwrap();
    } catch (e) {
      toast.error(String(e));
    } finally {
      setConfirmLoading(false);
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 xl:flex-row xl:items-end xl:justify-between">
        <div className="grid w-full gap-4 sm:grid-cols-2 xl:w-auto xl:grid-cols-[11rem_9rem]">
          <label className="grid gap-1 text-sm font-medium text-gray-700">
            Status
            <select
              className="min-h-[44px] rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm transition focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
              value={isActive}
              onChange={(ev) => dispatch(setIsActiveFilter(ev.target.value))}
            >
              <option value="">All</option>
              <option value="true">Active</option>
              <option value="false">Inactive</option>
            </select>
          </label>
          <label className="grid gap-1 text-sm font-medium text-gray-700">
            Page size
            <select
              className="min-h-[44px] rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm transition focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
              value={pageSize}
              onChange={(ev) => dispatch(setPageSize(Number(ev.target.value)))}
            >
              {[5, 10, 25, 50].map((size) => (
                <option key={size} value={size}>
                  {size}
                </option>
              ))}
            </select>
          </label>
        </div>
        <Button type="button" variant="primary" onClick={openAdd}>
          Add category
        </Button>
      </div>

      <CategoryTable
        rows={items}
        loading={loading}
        onEdit={openEdit}
        onActivate={askActivate}
        onDeactivate={askDeactivate}
      />

      <div className="flex flex-col items-center justify-between gap-3 border-t border-gray-200 pt-4 sm:flex-row">
        <p className="text-sm text-gray-600">
          {total > 0
            ? `Showing ${firstItem}-${lastItem} of ${total}`
            : "No categories"}
          {" - "}
          Page {page} of {safeTotalPages}
        </p>
        <div className="flex gap-2">
          <Button
            type="button"
            variant="secondary"
            disabled={page <= 1}
            onClick={() => dispatch(setPage(page - 1))}
          >
            Previous
          </Button>
          <Button
            type="button"
            variant="secondary"
            disabled={page >= safeTotalPages}
            onClick={() => dispatch(setPage(page + 1))}
          >
            Next
          </Button>
        </div>
      </div>

      <CategoryFormModal
        formKey={formKey}
        open={formOpen}
        onClose={() => {
          if (!formSaving) setFormOpen(false);
        }}
        mode={formMode}
        category={editing}
        existingNames={existingNames}
        saving={formSaving || mutating}
        serverError={formServerError}
        onSubmit={handleFormSubmit}
      />

      <ConfirmationModal
        open={confirm.open}
        onClose={() => {
          if (!confirmLoading)
            setConfirm({ open: false, type: null, row: null });
        }}
        title={
          confirm.type === "activate"
            ? "Activate category"
            : "Deactivate category"
        }
        message={
          confirm.type === "activate"
            ? `Activate "${confirm.row?.categoryName ?? ""}"?`
            : `Deactivate "${confirm.row?.categoryName ?? ""}"?`
        }
        confirmLabel={confirm.type === "activate" ? "Activate" : "Deactivate"}
        tone={confirm.type === "deactivate" ? "danger" : "neutral"}
        loading={confirmLoading}
        onConfirm={runConfirm}
      />
    </div>
  );
}
