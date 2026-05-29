import { useCallback, useEffect, useRef, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { sendChatMessageApi } from "../services/chatApi.js";
import {
  addMessage,
  clearChat,
  replaceLastMessage,
  selectChatMessages,
} from "../chatSlice.js";
import { selectAuthUser, selectRoleNames } from "../../auth/authSlice.js";

const QUICK_ACTIONS_BY_ROLE = {
  Admin: [
    { label: "All Requests", msg: "Show all recent requests" },
    { label: "Pending", msg: "Show pending requests" },
  ],
  Manager: [
    { label: "Team Requests", msg: "Show my team requests" },
    { label: "My Requests", msg: "Show my requests" },
  ],
  Employee: [
    { label: "My Requests", msg: "Show my requests" },
    { label: "How to create?", msg: "How do I create a new request?" },
  ],
  Support: [
    { label: "Assigned", msg: "Show requests assigned to me" },
    { label: "Open Tickets", msg: "Show open support tickets" },
  ],
};

function getQuickActions(roles) {
  const all = [];
  for (const role of roles) {
    const actions = QUICK_ACTIONS_BY_ROLE[role];
    if (actions) all.push(...actions);
  }
  return all.length > 0 ? all : QUICK_ACTIONS_BY_ROLE.Employee;
}

function TypingDots() {
  return (
    <div className="flex items-center gap-1 px-1">
      <span className="h-2 w-2 animate-bounce rounded-full bg-emerald-400 [animation-delay:0ms]" />
      <span className="h-2 w-2 animate-bounce rounded-full bg-emerald-400 [animation-delay:150ms]" />
      <span className="h-2 w-2 animate-bounce rounded-full bg-emerald-400 [animation-delay:300ms]" />
    </div>
  );
}

function ChatMessage({ message, isUser }) {
  return (
    <div className={`flex ${isUser ? "justify-end" : "justify-start"}`}>
      <div
        className={`max-w-[85%] rounded-2xl px-4 py-2.5 text-sm leading-relaxed shadow-sm ${
          isUser
            ? "bg-emerald-600 text-white rounded-br-md"
            : "bg-white text-gray-800 rounded-bl-md border border-gray-200"
        }`}
      >
        <p className="whitespace-pre-wrap break-words">{message}</p>
      </div>
    </div>
  );
}

export function ChatWidget() {
  const [open, setOpen] = useState(false);
  const [input, setInput] = useState("");
  const [loading, setLoading] = useState(false);
  const [errorMsg, setErrorMsg] = useState(null);
  const dispatch = useDispatch();
  const messages = useSelector(selectChatMessages);
  const user = useSelector(selectAuthUser);
  const roles = useSelector(selectRoleNames);
  const bottomRef = useRef(null);
  const inputRef = useRef(null);

  const scrollToBottom = useCallback(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, []);

  useEffect(() => {
    if (open) scrollToBottom();
  }, [messages, open, scrollToBottom]);

  useEffect(() => {
    if (open) inputRef.current?.focus();
  }, [open]);

  const focusInput = useCallback(() => {
    if (!open) return;
    setTimeout(() => inputRef.current?.focus(), 0);
  }, [open]);

  const handleSend = useCallback(
    async (textOverride) => {
      const text = (textOverride || input).trim();
      if (!text || loading) return;

      setInput("");
      setErrorMsg(null);
      const userMsg = { id: Date.now().toString(), text, isUser: true };
      dispatch(addMessage(userMsg));
      setLoading(true);

      try {
        const { response } = await sendChatMessageApi(text);
        dispatch(
          addMessage({
            id: (Date.now() + 1).toString(),
            text: response || "I'm not sure how to respond to that.",
            isUser: false,
          }),
        );
      } catch {
        const id = (Date.now() + 1).toString();
        dispatch(
          addMessage({
            id,
            text: "Request failed.",
            isUser: false,
            error: true,
          }),
        );
        setErrorMsg(id);
      } finally {
        setLoading(false);
        focusInput();
      }
    },
    [input, loading, dispatch],
  );

  const retry = useCallback(
    async (msgId, text) => {
      setErrorMsg(null);
      setLoading(true);

      try {
        const { response } = await sendChatMessageApi(text);
        dispatch(
          replaceLastMessage({
            id: msgId,
            text: response || "I'm not sure how to respond to that.",
            isUser: false,
          }),
        );
      } catch {
        dispatch(
          replaceLastMessage({
            id: msgId,
            text: "Request failed again. Please try later.",
            isUser: false,
            error: true,
          }),
        );
        setErrorMsg(msgId);
      } finally {
        setLoading(false);
        focusInput();
      }
    },
    [dispatch],
  );

  const handleKeyDown = (e) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  return (
    <>
      <div
        className={`fixed bottom-0 right-0 z-50 flex flex-col bg-white shadow-2xl transition-all duration-300 ease-out sm:bottom-6 sm:right-6 sm:rounded-2xl sm:border sm:border-gray-200 ${
          open
            ? "inset-x-0 top-0 sm:inset-auto sm:h-[600px] sm:w-[400px]"
            : "h-0 w-0 overflow-hidden"
        }`}
      >
        {open && (
          <>
            <div className="flex shrink-0 items-center justify-between border-b border-gray-200 bg-emerald-600 px-4 py-3 sm:rounded-t-2xl">
              <div className="flex items-center gap-2.5">
                <div className="flex h-8 w-8 items-center justify-center rounded-full bg-white/20">
                  <svg
                    className="h-4 w-4 text-white"
                    fill="none"
                    viewBox="0 0 24 24"
                    strokeWidth={2}
                    stroke="currentColor"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      d="M8.625 12a.375.375 0 11-.75 0 .375.375 0 01.75 0zm0 0H8.25m4.125 0a.375.375 0 11-.75 0 .375.375 0 01.75 0zm0 0H12m4.125 0a.375.375 0 11-.75 0 .375.375 0 01.75 0zm0 0h-.375M21 12c0 4.556-4.03 8.25-9 8.25a9.764 9.764 0 01-2.555-.337A5.972 5.972 0 015.41 20.97a5.969 5.969 0 01-.474-.065 4.48 4.48 0 00.978-2.025c.09-.457-.133-.901-.467-1.226C3.93 16.178 3 14.189 3 12c0-4.556 4.03-8.25 9-8.25s9 3.694 9 8.25z"
                    />
                  </svg>
                </div>
                <div>
                  <p className="text-sm font-semibold text-white">
                    AI Assistant
                  </p>
                  {user && (
                    <p className="text-xs text-emerald-100">
                      {user.fullName || user.email}
                    </p>
                  )}
                </div>
              </div>
              <button
                type="button"
                className="flex h-8 w-8 items-center justify-center rounded-full text-white/80 hover:bg-white/10 hover:text-white transition-colors cursor-pointer"
                onClick={() => setOpen(false)}
              >
                <svg
                  className="h-5 w-5"
                  fill="none"
                  viewBox="0 0 24 24"
                  strokeWidth={2}
                  stroke="currentColor"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    d="M6 18L18 6M6 6l12 12"
                  />
                </svg>
              </button>
            </div>

            <div className="flex-1 overflow-y-auto bg-gray-50 px-4 py-4 space-y-3">
              {messages.length === 0 && (
                <p className="text-center text-xs text-gray-400 py-8">
                  Ask me anything about requests or how to use the system.
                </p>
              )}

              {messages.map((msg) => (
                <div key={msg.id}>
                  <ChatMessage message={msg.text} isUser={msg.isUser} />
                  {msg.error && (
                    <div className="flex justify-start mt-1">
                      <button
                        type="button"
                        onClick={() => retry(msg.id, input)}
                        disabled={loading}
                        className="text-xs text-emerald-600 hover:text-emerald-800 underline cursor-pointer disabled:opacity-40"
                      >
                        Retry
                      </button>
                    </div>
                  )}
                </div>
              ))}

              {loading && (
                <div className="flex justify-start">
                  <div className="rounded-2xl rounded-bl-md border border-gray-200 bg-white px-4 py-3 shadow-sm">
                    <TypingDots />
                  </div>
                </div>
              )}
              <div ref={bottomRef} />
            </div>

            {messages.length === 0 && (
              <div className="shrink-0 border-t border-gray-200 bg-gray-50 px-4 py-2">
                <div className="flex flex-wrap gap-2">
                  {getQuickActions(roles).map((act) => (
                    <button
                      key={act.msg}
                      type="button"
                      onClick={() => handleSend(act.msg)}
                      disabled={loading}
                      className="rounded-full border border-emerald-300 bg-white px-3 py-1 text-xs text-emerald-700 hover:bg-emerald-50 transition-colors cursor-pointer disabled:opacity-40"
                    >
                      {act.label}
                    </button>
                  ))}
                </div>
              </div>
            )}

            <div className="shrink-0 border-t border-gray-200 bg-white px-4 py-3 sm:rounded-b-2xl">
              <div className="flex items-end gap-2">
                <div className="relative flex-1">
                  <textarea
                    ref={inputRef}
                    value={input}
                    onChange={(e) => setInput(e.target.value)}
                    onKeyDown={handleKeyDown}
                    placeholder="Type your message..."
                    autoFocus
                    onBlur={() => {
                      if (open) focusInput();
                    }}
                    rows={1}
                    className="block w-full resize-none rounded-xl border border-gray-300 bg-gray-50 px-4 py-2.5 pr-4 text-sm text-gray-900 placeholder-gray-400 focus:border-emerald-500 focus:bg-white focus:outline-none focus:ring-1 focus:ring-emerald-500 transition-colors"
                    disabled={loading}
                  />
                </div>
                <button
                  type="button"
                  onClick={() => handleSend()}
                  disabled={!input.trim() || loading}
                  className="flex h-[42px] w-[42px] shrink-0 items-center justify-center rounded-xl bg-emerald-600 text-white shadow-sm hover:bg-emerald-700 active:bg-emerald-800 disabled:opacity-40 disabled:cursor-not-allowed transition-all cursor-pointer"
                >
                  <svg
                    className="h-5 w-5"
                    fill="none"
                    viewBox="0 0 24 24"
                    strokeWidth={2}
                    stroke="currentColor"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      d="M6 12L3.269 3.126A59.768 59.768 0 0121.485 12 59.77 59.77 0 013.27 20.876L5.999 12zm0 0h7.5"
                    />
                  </svg>
                </button>
              </div>
            </div>
          </>
        )}
      </div>

      <button
        type="button"
        onClick={() => setOpen((o) => !o)}
        className={`fixed bottom-6 right-6 z-50 flex h-14 w-14 items-center justify-center rounded-full shadow-lg transition-all duration-200 hover:scale-105 active:scale-95 cursor-pointer ${
          open ? "hidden" : "bg-emerald-600 hover:bg-emerald-700"
        }`}
      >
        <svg
          className="h-6 w-6 text-white"
          fill="none"
          viewBox="0 0 24 24"
          strokeWidth={2}
          stroke="currentColor"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            d="M8.625 12a.375.375 0 11-.75 0 .375.375 0 01.75 0zm0 0H8.25m4.125 0a.375.375 0 11-.75 0 .375.375 0 01.75 0zm0 0H12m4.125 0a.375.375 0 11-.75 0 .375.375 0 01.75 0zm0 0h-.375M21 12c0 4.556-4.03 8.25-9 8.25a9.764 9.764 0 01-2.555-.337A5.972 5.972 0 015.41 20.97a5.969 5.969 0 01-.474-.065 4.48 4.48 0 00.978-2.025c.09-.457-.133-.901-.467-1.226C3.93 16.178 3 14.189 3 12c0-4.556 4.03-8.25 9-8.25s9 3.694 9 8.25z"
          />
        </svg>
      </button>
    </>
  );
}
