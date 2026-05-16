import { Modal } from './Modal.jsx'
import { Button } from './Button.jsx'

/**
 * Use only for sensitive confirmations (e.g. activate / deactivate).
 */
export function ConfirmationModal({
  open,
  onClose,
  title,
  message,
  confirmLabel = 'Confirm',
  cancelLabel = 'Cancel',
  onConfirm,
  loading,
  tone = 'neutral',
}) {
  const confirmVariant = tone === 'danger' ? 'danger' : 'primary'

  return (
    <Modal
      open={open}
      onClose={onClose}
      title={title}
      closeOnOverlayClick={!loading}
      closeOnEscape={!loading}
      footer={
        <>
          <Button
            type="button"
            variant="secondary"
            disabled={loading}
            onClick={onClose}
          >
            {cancelLabel}
          </Button>
          <Button
            type="button"
            variant={confirmVariant}
            loading={loading}
            disabled={loading}
            onClick={onConfirm}
          >
            {confirmLabel}
          </Button>
        </>
      }
    >
      <p className="text-sm leading-relaxed text-gray-600">{message}</p>
    </Modal>
  )
}
