/** Served from `public/flowdesk_logo-black.webp` */
export const FLOWDESK_LOGO_SRC = "/flowdesk_logo-black.webp";

/**
 * Wordmark for headers. Black artwork on light backgrounds.
 */
export function FlowDeskLogo({ className = "", ...rest }) {
  return (
    <img
      src={FLOWDESK_LOGO_SRC}
      alt="FlowDesk"
      decoding="async"
      className={`h-6 w-auto max-h-6 max-w-[min(100%,9rem)] object-contain object-left md:h-7 md:max-w-40 ${className}`}
      {...rest}
    />
  );
}
