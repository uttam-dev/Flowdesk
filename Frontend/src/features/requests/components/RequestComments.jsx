// Component-local: comments UI only — no auth selector needed here

function fmtCommentDate(v) {
  if (!v) return "—";
  try {
    const d = new Date(v);
    if (Number.isNaN(d.getTime())) return String(v);
    return d.toLocaleString(undefined, {
      day: "numeric",
      month: "short",
      year: "numeric",
      hour: "numeric",
      minute: "2-digit",
      hour12: true,
    });
  } catch {
    return String(v);
  }
}

export function RequestComments({ comments = [], loading }) {
  if (loading === "pending") {
    return (
      <div className="rounded-xl border border-gray-200 bg-white p-6 text-center text-sm text-gray-600 shadow-sm">
        Loading comments…
      </div>
    );
  }

  return (
    <div className="rounded-xl border border-gray-200 bg-white p-4 shadow-sm">
      <h3 className="text-sm font-semibold text-gray-900">Comments</h3>
      <div className="mt-4 max-h-[300px] overflow-y-auto pr-1 space-y-[10px]">
        {comments.length === 0 ? (
          <p className="text-center text-sm text-gray-500 py-4">
            No comments yet.
          </p>
        ) : (
          comments.map((c) => {
            // Helpful runtime check during development when API occasionally
            // returns non-boolean values for `isCurrentUser`.
            if (process.env.NODE_ENV === "development") {
              if (c.isCurrentUser !== true && c.isCurrentUser !== false) {
                console.debug(
                  "RequestComments: isCurrentUser non-boolean",
                  c.commentId ?? c.createdOn,
                  c.isCurrentUser,
                  c,
                );
              }
            }

            const isMine = c.isCurrentUser === true;
            const rawName = c.userName == null ? "-" : c.userName;
            const name = isMine ? "You" : rawName;
            const role = c.roleName ? ` (${c.roleName})` : "";

            return (
              <div
                key={c.commentId ?? `${c.createdOn}`}
                className={`flex mb-3 ${isMine ? "justify-end pr-2" : "justify-start pl-2"}`}
              >
                <div className="max-w-[60%] w-fit">
                  <div
                    className={`px-3.5 py-2.5 rounded-2xl leading-[1.4] ${
                      isMine
                        ? "bg-green-600/80 text-white rounded-br-sm"
                        : "bg-gray-100 text-gray-900 rounded-bl-sm"
                    }`}
                  >
                    {/* Name + Role */}
                    <span
                      className={`text-xs opacity-80 block mb-1 ${
                        isMine ? "text-white" : "text-gray-900"
                      }`}
                    >
                      {name}
                      {role}
                    </span>

                    {/* Message Text */}
                    <p className="text-sm font-medium whitespace-pre-wrap break-all">
                      {c.commentText}
                    </p>

                    {/* Date Time */}
                    <span
                      className={`text-[11px] opacity-60 block mt-1.5 text-right ${
                        isMine ? "text-white" : "text-gray-900"
                      }`}
                    >
                      {fmtCommentDate(c.createdOn)}
                    </span>
                  </div>
                </div>
              </div>
            );
          })
        )}
      </div>
    </div>
  );
}
