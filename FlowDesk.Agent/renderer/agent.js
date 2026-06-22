// FlowDesk Agent — renderer process
// Handles SignalR + WebRTC + token refresh lifecycle
const signalR = window.signalR;

const ICE_SERVERS = [
  { urls: "stun:stun.l.google.com:19302" },
  { urls: "stun:stun1.l.google.com:19302" },
];

let authState = {
  accessToken: null,
  refreshToken: null,
  apiUrl: null,
  hubUrl: null,
};

let connection = null;
let peerConnection = null;
let screenStream = null;
let currentSessionId = null;
let pendingSessionId = null;
let pendingSupportReadySessionId = null;
let isRefreshing = false;
let reconnectTimer = null;
let refreshTimer = null;
let isStartingShare = false;
let isStartingSignalR = false;

function mapAuthResponse(raw) {
  const body =
    raw && typeof raw === "object" && raw.data != null ? raw.data : raw;
  const data = body && typeof body === "object" ? body : {};

  return {
    accessToken:
      data.accessToken ?? data.token ?? data.access_token ?? data.Token ?? null,
    refreshToken:
      data.refreshToken ?? data.refresh_token ?? data.RefreshToken ?? null,
  };
}

async function refreshAccessToken() {
  if (isRefreshing) return false;
  isRefreshing = true;

  try {
    if (!window.agent?.refreshToken) {
      window.agent.reportDisconnected();
      window.agent.requestReLogin();
      return false;
    }

    const refreshResult = await window.agent.refreshToken(authState.refreshToken);
    if (!refreshResult?.ok) {
      window.agent.reportDisconnected();
      window.agent.requestReLogin();
      return false;
    }

    const { accessToken, refreshToken } = mapAuthResponse(refreshResult.body);

    if (!accessToken) {
      window.agent.reportDisconnected();
      window.agent.requestReLogin();
      return false;
    }

    authState.accessToken = accessToken;
    authState.refreshToken = refreshToken ?? authState.refreshToken;
    return true;
  } catch {
    return false;
  } finally {
    isRefreshing = false;
  }
}

function startTokenRefreshTimer() {
  const REFRESH_INTERVAL_MS = 14 * 60 * 1000;

  if (refreshTimer) clearInterval(refreshTimer);
  refreshTimer = setInterval(async () => {
    await refreshAccessToken();
  }, REFRESH_INTERVAL_MS);
}

function cleanupWebRTC() {
  if (screenStream) {
    screenStream.getTracks().forEach((t) => t.stop());
    screenStream = null;
  }
  if (peerConnection) {
    peerConnection.close();
    peerConnection = null;
  }
  currentSessionId = null;
  pendingSessionId = null;
  pendingSupportReadySessionId = null;
  isStartingShare = false;
  window.agent.reportSessionEnded();
}

async function startScreenShare(sessionId) {
  if (isStartingShare || currentSessionId === sessionId) return;
  isStartingShare = true;

  try {
    currentSessionId = sessionId;
    window.agent.reportSessionStarted();

    screenStream = await navigator.mediaDevices.getDisplayMedia({
      video: { frameRate: 30, cursor: "always" },
      audio: false,
    });

    const videoTracks = screenStream.getVideoTracks();
    if (videoTracks.length === 0) {
      throw new Error("No video tracks from screen capture");
    }

    peerConnection = new RTCPeerConnection({ iceServers: ICE_SERVERS });

    screenStream.getTracks().forEach((track) => {
      peerConnection.addTrack(track, screenStream);
    });

    peerConnection.onicecandidate = ({ candidate }) => {
      if (candidate && connection) {
        connection
          .invoke(
            "SendICECandidate",
            sessionId,
            JSON.stringify(candidate),
            true,
          )
          .catch(() => {});
      }
    };

    peerConnection.onconnectionstatechange = () => {
      const state = peerConnection?.connectionState;
      if (state === "disconnected" || state === "failed") {
        cleanupWebRTC();
      }
    };

    videoTracks[0].onended = () => {
      cleanupWebRTC();
    };

    await connection.invoke("AgentReady", sessionId);

    const offer = await peerConnection.createOffer();
    await peerConnection.setLocalDescription(offer);

    await connection.invoke(
      "SendWebRTCOffer",
      sessionId,
      JSON.stringify(offer),
    );
  } catch {
    cleanupWebRTC();
  } finally {
    isStartingShare = false;
  }
}

async function handleControlEvent(event) {
  if (!event?.type) return;

  const screen = await window.agent.getScreenSize();
  const absX = Math.round((event.x ?? 0) * screen.width);
  const absY = Math.round((event.y ?? 0) * screen.height);

  switch (event.type.toLowerCase()) {
    case "mousemove":
      window.agent.mouseMove(absX, absY);
      break;
    case "mouseclick":
      window.agent.mouseClick(
        absX,
        absY,
        event.button || "left",
        !!event.isDoubleClick,
      );
      break;
    case "keypress":
      window.agent.keyPress(event.key, event.modifier || null);
      break;
    case "scroll":
      window.agent.scroll(absX, absY, event.scrollDirection || "down");
      break;
    default:
      break;
  }
}

function buildConnection() {
  return new signalR.HubConnectionBuilder()
    .withUrl(authState.hubUrl, {
      accessTokenFactory: () => authState.accessToken,
      transport:
        signalR.HttpTransportType.WebSockets |
        signalR.HttpTransportType.LongPolling,
    })
    .withAutomaticReconnect({
      nextRetryDelayInMilliseconds: (ctx) => {
        const delays = [0, 2000, 5000, 10000, 30000];
        return delays[ctx.previousRetryCount] ?? 60000;
      },
    })
    .configureLogging(signalR.LogLevel.Warning)
    .build();
}

function registerHandlers() {
  // Popup is shown in the browser — agent only needs streaming/control handlers
  connection.on("RemoteSessionInitiated", () => {});

  connection.on("RemoteSessionAccepted", async ({ sessionId }) => {
    if (!sessionId) return;
    pendingSessionId = sessionId;
    try {
      await connection.invoke("AgentJoinSession", sessionId);
    } catch {
      pendingSessionId = null;
      return;
    }

    if (pendingSupportReadySessionId === sessionId) {
      pendingSupportReadySessionId = null;
      await startScreenShare(sessionId);
    }
  });

  connection.on("SupportReady", async ({ sessionId }) => {
    if (!sessionId) return;
    if (currentSessionId === sessionId) return;

    if (pendingSessionId === sessionId) {
      await startScreenShare(sessionId);
      return;
    }

    pendingSupportReadySessionId = sessionId;
  });

  connection.on("ReceiveWebRTCAnswer", async ({ sessionId, sdpAnswer }) => {
    if (!peerConnection || !sdpAnswer) return;
    if (currentSessionId && sessionId && sessionId !== currentSessionId) return;
    if (peerConnection.signalingState !== "have-local-offer") return;
    try {
      await peerConnection.setRemoteDescription(
        new RTCSessionDescription(JSON.parse(sdpAnswer)),
      );
    } catch {
      /* ignore invalid SDP */
    }
  });

  connection.on("ReceiveICECandidate", async ({ sessionId, candidate }) => {
    if (!peerConnection || !candidate) return;
    if (currentSessionId && sessionId && sessionId !== currentSessionId) return;
    try {
      await peerConnection.addIceCandidate(
        new RTCIceCandidate(JSON.parse(candidate)),
      );
    } catch {
      /* ignore invalid ICE */
    }
  });

  connection.on("ExecuteControlEvent", (event) => {
    handleControlEvent(event);
  });

  connection.on("RemoteSessionEnded", () => {
    cleanupWebRTC();
  });

  connection.onreconnecting(() => {
    window.agent.reportDisconnected();
  });

  connection.onreconnected(() => {
    window.agent.reportConnected();
  });

  connection.onclose(async () => {
    window.agent.reportDisconnected();
    cleanupWebRTC();

    const refreshed = await refreshAccessToken();
    if (refreshed) {
      scheduleReconnect();
    }
  });
}

function scheduleReconnect() {
  if (reconnectTimer) clearTimeout(reconnectTimer);
  reconnectTimer = setTimeout(async () => {
    await startSignalR();
  }, 3000);
}

async function startSignalR() {
  if (isStartingSignalR) return;
  isStartingSignalR = true;

  try {
    if (connection && connection.state !== signalR.HubConnectionState.Disconnected) {
      return;
    }

    if (!connection) {
      connection = buildConnection();
      registerHandlers();
    }

    await connection.start();
    window.agent.reportConnected();
    startTokenRefreshTimer();
  } catch {
    window.agent.reportDisconnected();
    scheduleReconnect();
  } finally {
    isStartingSignalR = false;
  }
}

window.agent.onStart((data) => {
  authState.accessToken = data.accessToken;
  authState.refreshToken = data.refreshToken;
  authState.apiUrl = data.apiUrl;
  authState.hubUrl = data.hubUrl;

  startSignalR();
});
