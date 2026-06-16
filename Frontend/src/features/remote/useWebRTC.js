import { useEffect, useRef, useCallback } from "react";
import { useDispatch } from "react-redux";
import { getConnection } from "../../services/signalrService.js";
import { setSessionActive } from "./remoteSlice.js";

const ICE_SERVERS = [
  { urls: "stun:stun.l.google.com:19302" },
  { urls: "stun:stun1.l.google.com:19302" },
];

/**
 * Support side WebRTC hook.
 * Joins the session group immediately, waits for agent offer, attaches stream to videoRef.
 *
 * @param {number} sessionId
 * @param {number} targetUserId - employee/manager whose machine is being viewed
 * @param {React.RefObject} videoRef
 */
export function useWebRTC(sessionId, targetUserId, videoRef) {
  const dispatch = useDispatch();
  const peerRef = useRef(null);
  const sessionIdRef = useRef(sessionId);

  useEffect(() => {
    sessionIdRef.current = sessionId;
  }, [sessionId]);

  const cleanup = useCallback(() => {
    if (peerRef.current) {
      peerRef.current.close();
      peerRef.current = null;
    }
    if (videoRef.current) {
      videoRef.current.srcObject = null;
    }
  }, [videoRef]);

  useEffect(() => {
    if (!sessionId || !targetUserId) return;

    const connection = getConnection();
    if (!connection) return;

    let cleanupHandlers = () => {};

    const onOffer = async ({ sessionId: offerSessionId, sdpOffer }) => {
      if (!sdpOffer || offerSessionId !== sessionIdRef.current) return;

      if (peerRef.current) {
        peerRef.current.close();
        peerRef.current = null;
      }

      const pc = new RTCPeerConnection({ iceServers: ICE_SERVERS });
      peerRef.current = pc;

      pc.ontrack = (event) => {
        if (videoRef.current && event.streams[0]) {
          videoRef.current.srcObject = event.streams[0];
          dispatch(setSessionActive());
        }
      };

      pc.onicecandidate = ({ candidate }) => {
        if (candidate) {
          connection
            .invoke(
              "SendICECandidate",
              sessionIdRef.current,
              JSON.stringify(candidate),
              false,
            )
            .catch(() => {});
        }
      };

      pc.onconnectionstatechange = () => {
        if (
          pc.connectionState === "disconnected" ||
          pc.connectionState === "failed"
        ) {
          cleanup();
        }
      };

      try {
        await pc.setRemoteDescription(
          new RTCSessionDescription(JSON.parse(sdpOffer)),
        );

        const answer = await pc.createAnswer();
        await pc.setLocalDescription(answer);

        await connection.invoke(
          "SendWebRTCAnswer",
          sessionIdRef.current,
          JSON.stringify(answer),
        );
      } catch {
        cleanup();
      }
    };

    const onICECandidate = async ({ sessionId: iceSessionId, candidate }) => {
      if (!candidate || iceSessionId !== sessionIdRef.current) return;
      if (!peerRef.current) return;
      try {
        await peerRef.current.addIceCandidate(
          new RTCIceCandidate(JSON.parse(candidate)),
        );
      } catch {
        /* ignore invalid ICE */
      }
    };

    connection.on("ReceiveWebRTCOffer", onOffer);
    connection.on("ReceiveICECandidate", onICECandidate);

    cleanupHandlers = () => {
      connection.off("ReceiveWebRTCOffer", onOffer);
      connection.off("ReceiveICECandidate", onICECandidate);
    };

    connection
      .invoke("AdminJoinSession", sessionId, targetUserId)
      .catch(() => {});

    return () => {
      cleanupHandlers();
      cleanup();
    };
  }, [sessionId, targetUserId, dispatch, videoRef, cleanup]);

  return { peerConnection: peerRef.current };
}
