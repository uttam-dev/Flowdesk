import { useState } from 'react'
import { Modal } from '../../../components/ui/Modal.jsx'
import { Button } from '../../../components/ui/Button.jsx'
import { Input } from '../../../components/ui/Input.jsx'
import { REQUEST_PRIORITY } from '../requestUtils.js'

const selectClass =
  'w-full rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm transition focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/20'

function validateForm(values) {
  const title = values.title.trim()
  const description = values.description.trim()
  if (!values.categoryId) return 'Category is required'
  if (!title) return 'Title is required'
  if (title.length < 1 || title.length > 100) return 'Title must be 1–100 characters'
  if (!description) return 'Description is required'
  if (description.length > 500) return 'Description must be at most 500 characters'
  if (!values.priority) return 'Priority is required'
  return null
}

export function RequestForm({
  categories,
  saving,
  serverError,
  onSubmit,
  submitLabel = 'Submit',
}) {
  const [categoryId, setCategoryId] = useState('')
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [priority, setPriority] = useState(String(REQUEST_PRIORITY.Medium))
  const [error, setError] = useState(null)

  function handleSubmit(e) {
    e.preventDefault()
    const err = validateForm({ categoryId, title, description, priority })
    if (err) {
      setError(err)
      return
    }
    setError(null)
    onSubmit({
      categoryId: Number(categoryId),
      title: title.trim(),
      description: description.trim(),
      priority: Number(priority),
    })
  }

  const combinedError = error || serverError

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
        <label htmlFor="req-category" className="text-sm font-medium text-gray-700">
          Category
        </label>
        <select
          id="req-category"
          className={selectClass}
          value={categoryId}
          onChange={(ev) => setCategoryId(ev.target.value)}
          disabled={saving}
        >
          <option value="">Select category</option>
          {categories.map((c) => (
            <option key={c.categoryId} value={c.categoryId}>
              {c.categoryName}
            </option>
          ))}
        </select>
      </div>

      <Input
        id="req-title"
        label="Title"
        value={title}
        onChange={(ev) => {
          setTitle(ev.target.value)
          if (error) setError(null)
        }}
        disabled={saving}
        maxLength={100}
      />

      <div className="flex flex-col gap-1.5">
        <label htmlFor="req-description" className="text-sm font-medium text-gray-700">
          Description
        </label>
        <textarea
          id="req-description"
          className={selectClass}
          rows={4}
          value={description}
          onChange={(ev) => {
            setDescription(ev.target.value)
            if (error) setError(null)
          }}
          disabled={saving}
          maxLength={500}
        />
        <p className="text-xs text-gray-500">{description.length}/500</p>
      </div>

      <div className="flex flex-col gap-1.5">
        <label htmlFor="req-priority" className="text-sm font-medium text-gray-700">
          Priority
        </label>
        <select
          id="req-priority"
          className={selectClass}
          value={priority}
          onChange={(ev) => setPriority(ev.target.value)}
          disabled={saving}
        >
          <option value={REQUEST_PRIORITY.High}>High</option>
          <option value={REQUEST_PRIORITY.Medium}>Medium</option>
          <option value={REQUEST_PRIORITY.Low}>Low</option>
        </select>
      </div>

      <Button type="submit" variant="primary" loading={saving} disabled={saving}>
        {submitLabel}
      </Button>
    </form>
  )
}

export function RequestModal({
  open,
  onClose,
  categories,
  saving,
  serverError,
  onSubmit,
}) {
  if (!open) return null

  return (
    <Modal
      open
      onClose={saving ? () => {} : onClose}
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
  )
}
