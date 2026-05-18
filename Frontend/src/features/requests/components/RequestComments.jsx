import { useSelector } from 'react-redux'
import { selectAuthUser } from '../../auth/authSlice.js'

function fmtCommentDate(v) {
  if (!v) return '—'
  try {
    const d = new Date(v)
    if (Number.isNaN(d.getTime())) return String(v)
    return d.toLocaleString(undefined, {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
      hour: 'numeric',
      minute: '2-digit',
      hour12: true,
    })
  } catch {
    return String(v)
  }
}

export function RequestComments({ comments, loading }) {
  const user = useSelector(selectAuthUser)
  const currentUserId = user?.id != null ? String(user.id) : null

  if (loading === 'pending') {
    return (
      <div className="rounded-xl border border-gray-200 bg-white p-6 text-center text-sm text-gray-600 shadow-sm">
        Loading comments…
      </div>
    )
  }

  return (
    <div className="rounded-xl border border-gray-200 bg-white p-4 shadow-sm">
      <h3 className="text-sm font-semibold text-gray-900">Comments</h3>
      <div className="mt-4 max-h-[28rem] space-y-4 overflow-y-auto">
        {comments.length === 0 ? (
          <p className="text-center text-sm text-gray-500">No comments yet.</p>
        ) : (
          comments.map((c) => {
            const isMine =
              currentUserId != null &&
              c.userId != null &&
              String(c.userId) === currentUserId
            const name = c.userName || 'Unknown'
            const role = c.roleName ? ` (${c.roleName})` : ''

            return (
              <div
                key={c.commentId ?? `${c.userId}-${c.createdOn}`}
                className={`flex ${isMine ? 'justify-end' : 'justify-start'}`}
              >
                <div className="max-w-[70%] space-y-1">
                  <p className="text-sm font-bold text-gray-900">
                    {name}
                    {role}
                  </p>
                  <p className="whitespace-pre-wrap break-words text-sm text-gray-800">
                    {c.commentText}
                  </p>
                  <p className="text-xs text-gray-500">{fmtCommentDate(c.createdOn)}</p>
                </div>
              </div>
            )
          })
        )}
      </div>
    </div>
  )
}
