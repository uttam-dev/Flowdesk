import { useState } from "react";
import { Modal } from "../../../components/ui/Modal.jsx";
import { Button } from "../../../components/ui/Button.jsx";
import { Input } from "../../../components/ui/Input.jsx";
import { REQUEST_PRIORITY } from "../requestUtils.js";

const selectClass =
  "w-full rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm transition focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/20";

function validateForm(values) {
  const errors = {};
  const title = values.title.trim();
  const description = values.description.trim();
  if (!values.categoryId) errors.categoryId = "Category is required";
  if (!title) errors.title = "Title is required";
  if (title.length < 1 || title.length > 50)
    errors.title = "Title must be 1–50 characters";
  if (!description) errors.description = "Description is required";
  if (description.length > 150)
    errors.description = "Description must be at most 150 characters";
  if (!values.priority) errors.priority = "Priority is required";
  return errors;
}

export function RequestForm({
  categories,
  saving,
  serverError,
  onSubmit,
  submitLabel = "Submit",
}) {
  const [categoryId, setCategoryId] = useState("");
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [priority, setPriority] = useState(String(REQUEST_PRIORITY.Medium));
  const [error, setError] = useState(null);
  const [fieldErrors, setFieldErrors] = useState({});

  function handleSubmit(e) {
    e.preventDefault();
    const next = validateForm({ categoryId, title, description, priority });
    if (Object.keys(next).length) {
      setFieldErrors(next);
      setError(null);
      return;
    }
    setFieldErrors({});
    setError(null);
    onSubmit({
      categoryId: Number(categoryId),
      title: title.trim(),
      description: description.trim(),
      priority: Number(priority),
    });
  }

  const combinedError = error || serverError;

  return (
    <form className="space-y-4" onSubmit={handleSubmit}>
      {combinedError ? (
        <p
          className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700"
          role="alert"
        >
          {combinedError}
        </p>
      ) : null}

      <div className="flex flex-col gap-1.5">
        <label
          htmlFor="req-category"
          className="text-sm font-medium text-gray-700"
        >
          Category
          <span className="text-red-500 ml-1">*</span>
        </label>
        <select
          id="req-category"
          className={selectClass}
          value={categoryId}
          onChange={(ev) => {
            setCategoryId(ev.target.value);
            if (fieldErrors.categoryId) {
              setFieldErrors((prev) => ({ ...prev, categoryId: undefined }));
            }
          }}
          disabled={saving}
        >
          <option value="">Select category</option>
          {categories.map((c) => (
            <option key={c.categoryId} value={c.categoryId}>
              {c.categoryName}
            </option>
          ))}
        </select>
        {fieldErrors.categoryId ? (
          <p className="text-red-500 text-sm mt-1" role="alert">
            {fieldErrors.categoryId}
          </p>
        ) : null}
      </div>

      <Input
        id="req-title"
        label={
          <>
            Title
            <span className="text-red-500 ml-1">*</span>
          </>
        }
        value={title}
        onChange={(ev) => {
          setTitle(ev.target.value);
          if (fieldErrors.title) {
            setFieldErrors((prev) => ({ ...prev, title: undefined }));
          }
          if (error) setError(null);
        }}
        disabled={saving}
        maxLength={50}
        error={fieldErrors.title}
      />

      <div className="flex flex-col gap-1.5">
        <label
          htmlFor="req-description"
          className="text-sm font-medium text-gray-700"
        >
          Description
          <span className="text-red-500 ml-1">*</span>
        </label>
        <textarea
          id="req-description"
          className={selectClass}
          rows={4}
          value={description}
          onChange={(ev) => {
            setDescription(ev.target.value);
            if (fieldErrors.description) {
              setFieldErrors((prev) => ({ ...prev, description: undefined }));
            }
            if (error) setError(null);
          }}
          disabled={saving}
          maxLength={150}
        />
        {fieldErrors.description ? (
          <p className="text-red-500 text-sm mt-1" role="alert">
            {fieldErrors.description}
          </p>
        ) : null}
        <p className="text-xs text-gray-500">{description.trim().length}/150</p>
      </div>

      <div className="flex flex-col gap-1.5">
        <label
          htmlFor="req-priority"
          className="text-sm font-medium text-gray-700"
        >
          Priority
          <span className="text-red-500 ml-1">*</span>
        </label>
        <select
          id="req-priority"
          className={selectClass}
          value={priority}
          onChange={(ev) => {
            setPriority(ev.target.value);
            if (fieldErrors.priority) {
              setFieldErrors((prev) => ({ ...prev, priority: undefined }));
            }
          }}
          disabled={saving}
        >
          <option value={REQUEST_PRIORITY.High}>High</option>
          <option value={REQUEST_PRIORITY.Medium}>Medium</option>
          <option value={REQUEST_PRIORITY.Low}>Low</option>
        </select>
        {fieldErrors.priority ? (
          <p className="text-red-500 text-sm mt-1" role="alert">
            {fieldErrors.priority}
          </p>
        ) : null}
      </div>

      <Button
        type="submit"
        variant="primary"
        loading={saving}
        disabled={saving}
      >
        {submitLabel}
      </Button>
    </form>
  );
}

export function RequestModal({
  open,
  onClose,
  categories,
  saving,
  serverError,
  onSubmit,
}) {
  if (!open) return null;

  return (
    <Modal
      open
      onClose={saving ? () => { } : onClose}
      title="Create request"
      closeOnOverlayClick={!saving}
      closeOnEscape={!saving}
    >
      <RequestForm
        categories={categories}
        saving={saving}
        serverError={serverError}
        onSubmit={onSubmit}
        submitLabel="Create"
      />
    </Modal>
  );
}
