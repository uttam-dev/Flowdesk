import {
  setIncomingRequest,
  setSessionAccepted,
  setSessionRejected,
  setSessionEnded,
  openViewer,
  clearSession,
} from "./remoteSlice.js";
import { selectRoleNames } from "../auth/authSlice.js";

export function registerListeners(connection, dispatch, getState) {
  const onInitiated = (payload) => {
    dispatch(setIncomingRequest(payload));
  };

  const onAccepted = (payload) => {
    dispatch(setSessionAccepted(payload));

    const roles = selectRoleNames(getState());
    const isSupport = roles.some((r) => r.toLowerCase() === "support");
    if (isSupport) {
      dispatch(openViewer());
    }
  };

  const onRejected = (payload) => dispatch(setSessionRejected(payload));

  const onEnded = (payload) => {
    dispatch(setSessionEnded(payload));
    dispatch(clearSession());
  };

  const onSupportReady = () => {};
  const onAgentStreamReady = () => {};

  connection.on("RemoteSessionInitiated", onInitiated);
  connection.on("RemoteSessionAccepted", onAccepted);
  connection.on("RemoteSessionRejected", onRejected);
  connection.on("RemoteSessionEnded", onEnded);
  connection.on("SupportReady", onSupportReady);
  connection.on("AgentStreamReady", onAgentStreamReady);

  return () => {
    connection.off("RemoteSessionInitiated", onInitiated);
    connection.off("RemoteSessionAccepted", onAccepted);
    connection.off("RemoteSessionRejected", onRejected);
    connection.off("RemoteSessionEnded", onEnded);
    connection.off("SupportReady", onSupportReady);
    connection.off("AgentStreamReady", onAgentStreamReady);
  };
}
