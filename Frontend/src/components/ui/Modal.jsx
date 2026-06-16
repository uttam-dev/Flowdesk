import { useEffect, useId, useRef } from 'react'
import { Button } from './Button.jsx'

export function Modal({
  open,
  onClose,
  title,
  children,
  footer,
  size = 'md',
  initialFocusRef,
  closeOnOverlayClick = true,
  closeOnEscape = true,
}) {
  const titleId = useId()
  const panelRef = useRef(null)

  useEffect(() => {
    if (!open) return
    const t = window.setTimeout(() => {
      if (initialFocusRef?.current) {
        initialFocusRef.current.focus()
      } else {
        panelRef.current
          ?.querySelector('button, [href], input, select, textarea')
          ?.focus()
      }
    }, 0)
    return () => window.clearTimeout(t)
  }, [open, initialFocusRef])

  useEffect(() => {
    if (!open) return
    function onKey(e) {
      if (e.key === 'Escape' && closeOnEscape) onClose?.()
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [open, onClose, closeOnEscape])

  if (!open) return null

  const sizes = {
    sm: 'max-w-md',
    md: 'max-w-lg',
    lg: 'max-w-2xl',
  }
  const max = sizes[size] ?? sizes.md

  return (
    <div
      className="fixed inset-0 z-[100000] flex items-end justify-center p-4 sm:items-center sm:p-6"
      role="dialog"
      aria-modal="true"
      aria-labelledby={title ? titleId : undefined}
    >
      <button
        type="button"
        className="absolute inset-0 bg-gray-900/40 backdrop-blur-[1px] transition-opacity duration-200"
        aria-label="Close dialog"
        onClick={() => {
          if (closeOnOverlayClick) onClose?.()
        }}
      />
      <div
        ref={panelRef}
        className={`relative z-10 w-full ${max} rounded-xl border border-gray-200 bg-white shadow-lg transition-all duration-200`}
      >
        <div className="flex items-start justify-between gap-4 border-b border-gray-200 px-5 py-4">
          {title ? (
            <h2
              id={titleId}
              className="text-lg font-semibold tracking-tight text-gray-900"
            >
              {title}
            </h2>
          ) : (
            <span />
          )}
          <Button
            type="button"
            variant="ghost"
            className="min-h-0 shrink-0 px-2 py-1"
            onClick={onClose}
          >
            ✕
          </Button>
        </div>
        <div className="max-h-[min(70vh,32rem)] overflow-y-auto px-5 py-4">
          {children}
        </div>
        {footer ? (
          <div className="flex flex-col-reverse gap-2 border-t border-gray-200 px-5 py-4 sm:flex-row sm:justify-end">
            {footer}
          </div>
        ) : null}
      </div>
    </div>
  )
}
