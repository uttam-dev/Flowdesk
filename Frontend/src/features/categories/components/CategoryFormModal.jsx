import { useState } from "react";
import { Modal } from "../../../components/ui/Modal.jsx";
import { Button } from "../../../components/ui/Button.jsx";
import { Input } from "../../../components/ui/Input.jsx";

function validateName(name) {
  const t = name.trim();
  if (!t) return "Category name is required";
  if (t.length < 3) return "Minimum 3 characters required";
  if (t.length > 100) return "Use up to 100 characters";
  return null;
}

function CategoryFormModalInner({
  onClose,
  mode,
  category,
  existingNames,
  saving,
  serverError,
  onSubmit,
}) {
  const [name, setName] = useState(() => category?.categoryName ?? "");
  const [approval, setApproval] = useState(() =>
    category?.isApprovalRequired ? "yes" : "no",
  );
  const [fieldError, setFieldError] = useState(null);
  const [error, setError] = useState(null);

  function handleSubmit(e) {
    e.preventDefault();
    const nameErr = validateName(name);
    if (nameErr) {
      setFieldError(nameErr);
      setError(null);
      return;
    }
    const trimmed = name.trim();
    const others = (existingNames ?? []).filter(
      (n) =>
        n.toLowerCase() !== (category?.categoryName ?? "").trim().toLowerCase(),
    );
    if (others.some((n) => n.toLowerCase() === trimmed.toLowerCase())) {
      setFieldError("Name must be unique");
      setError(null);
      return;
    }
    setFieldError(null);
    setError(null);
    onSubmit({
      categoryName: trimmed,
      isApprovalRequired: approval === "yes",
    });
  }

  const title = mode === "add" ? "Add category" : "Edit category";
  const combinedError = error || serverError;

  return (
    <Modal
      open
      onClose={saving ? () => {} : onClose}
      title={title}
      closeOnOverlayClick={!saving}
      closeOnEscape={!saving}
      footer={
        <>
          <Button
            type="button"
            variant="secondary"
            disabled={saving}
            onClick={onClose}
          >
            Cancel
          </Button>
          <Button
            type="button"
            variant="primary"
            loading={saving}
            disabled={saving}
            onClick={handleSubmit}
          >
            Save
          </Button>
        </>
      }
    >
      <form className="space-y-4" onSubmit={handleSubmit}>
        {combinedError ? (
          <p
            className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700"
            role="alert"
          >
            {combinedError}
          </p>
        ) : null}
        <Input
          id="categoryName"
          label={
            <>
              Category name
              <span className="text-red-500 ml-1">*</span>
            </>
          }
          value={name}
          onChange={(ev) => {
            setName(ev.target.value);
            if (fieldError) setFieldError(null);
            if (error) setError(null);
          }}
          disabled={saving}
          maxLength={100}
          autoComplete="off"
          error={fieldError}
        />
        <div className="flex flex-col gap-1.5">
          <label
            htmlFor="approval"
            className="text-sm font-medium text-gray-700"
          >
            Approval required
          </label>
          <select
            id="approval"
            className="w-full rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm transition-all duration-200 focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500 disabled:bg-gray-50"
            value={approval}
            onChange={(ev) => setApproval(ev.target.value)}
            disabled={saving}
          >
            <option value="no">No</option>
            <option value="yes">Yes</option>
          </select>
        </div>
      </form>
    </Modal>
  );
}

export function CategoryFormModal({
  open,
  onClose,
  mode,
  category,
  existingNames,
  saving,
  serverError,
  onSubmit,
  formKey = 0,
}) {
  if (!open) return null;

  return (
    <CategoryFormModalInner
      key={`${formKey}-${mode}-${category?.categoryId ?? "new"}`}
      onClose={onClose}
      mode={mode}
      category={category}
      existingNames={existingNames}
      saving={saving}
      serverError={serverError}
      onSubmit={onSubmit}
    />
  );
}
