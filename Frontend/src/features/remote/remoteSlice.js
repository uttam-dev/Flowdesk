import { createSlice, createAsyncThunk } from "@reduxjs/toolkit";
import { toast } from "sonner";
import { apiClient } from "../../services/apiClient.js";

export const initiateRemoteSession = createAsyncThunk(
  "remote/initiate",
  async (requestId, { rejectWithValue }) => {
    try {
      const res = await apiClient.post("/remote-sessions", { requestId });
      return res.data.data;
    } catch (err) {
      return rejectWithValue(
        err.response?.data?.message || "Failed to send remote access request.",
      );
    }
  },
);

export const respondToRemoteSession = createAsyncThunk(
  "remote/respond",
  async ({ sessionId, accepted, rejectionReason }, { rejectWithValue }) => {
    try {
      const res = await apiClient.put(`/remote-sessions/${sessionId}/respond`, {
        accepted,
        rejectionReason,
      });
      return res.data.data;
    } catch (err) {
      return rejectWithValue(
        err.response?.data?.message || "Failed to respond to remote session.",
      );
    }
  },
);

export const endRemoteSession = createAsyncThunk(
  "remote/end",
  async (
    { sessionId, resolutionNotes, resolveRequest },
    { rejectWithValue },
  ) => {
    try {
      const res = await apiClient.put(`/remote-sessions/${sessionId}/end`, {
        resolutionNotes,
        resolveRequest,
      });
      return res.data.data;
    } catch (err) {
      return rejectWithValue(
        err.response?.data?.message || "Failed to end remote session.",
      );
    }
  },
);

export const fetchLatestRemoteSession = createAsyncThunk(
  "remote/fetchLatest",
  async (requestId, { rejectWithValue }) => {
    try {
      const res = await apiClient.get(
        `/remote-sessions/by-request/${requestId}/latest`,
      );
      return res.data.data;
    } catch (err) {
      return rejectWithValue(
        err.response?.data?.message || "Failed to fetch session.",
      );
    }
  },
);

export const fetchRemoteSessionHistory = createAsyncThunk(
  "remote/fetchHistory",
  async (requestId, { rejectWithValue }) => {
    try {
      const res = await apiClient.get(
        `/remote-sessions/by-request/${requestId}/history`,
      );
      return res.data.data;
    } catch (err) {
      return rejectWithValue(
        err.response?.data?.message || "Failed to fetch session history.",
      );
    }
  },
);

const initialState = {
  // Active / latest session for the current request
  currentSession: null,

  // Session history list for a request
  sessionHistory: [],

  // 'idle' | 'pending' | 'accepted' | 'active' | 'ended' | 'rejected' | 'aborted'
  sessionStatus: "idle",

  // Incoming remote request data shown to employee/manager (Accept/Reject popup)
  incomingRequest: null,

  // Controls whether the full-screen viewer is open (support side)
  isViewerOpen: false,

  // Loading states
  isInitiating: false,
  isResponding: false,
  isEnding: false,
  isFetching: false,

  error: null,
};

const remoteSlice = createSlice({
  name: "remote",
  initialState,
  reducers: {
    // ── Called by SignalR listeners ───────────────────────────────────────────

    // Employee/Manager receives this - shows incoming popup
    setIncomingRequest(state, action) {
      return { ...state, incomingRequest: action.payload };
    },

    clearIncomingRequest(state) {
      state.incomingRequest = null;
    },

    // Support side: target accepted → open viewer
    setSessionAccepted(state, action) {
      state.sessionStatus = "accepted";
      if (action.payload?.sessionId) {
        state.currentSession = {
          ...state.currentSession,
          id: action.payload.sessionId,
          remoteSessionId: action.payload.sessionId,
        };
      }
    },

    // Support side: target rejected
    setSessionRejected(state, action) {
      state.sessionStatus = "rejected";
      state.isViewerOpen = false;
      toast.error(
        action.payload?.rejectionReason
          ? `Remote access rejected: ${action.payload.rejectionReason}`
          : "Remote access request was rejected.",
      );
    },

    // Both sides: WebRTC stream is live
    setSessionActive(state) {
      state.sessionStatus = "active";
    },

    // Both sides: session ended
    setSessionEnded(state, action) {
      state.sessionStatus = "ended";
      state.isViewerOpen = false;
      if (action.payload?.resolved) {
        toast.success("Remote session ended. Request has been resolved.");
      } else {
        toast.info("Remote session ended.");
      }
    },

    openViewer(state) {
      state.isViewerOpen = true;
    },

    closeViewer(state) {
      state.isViewerOpen = false;
    },

    clearSession(state) {
      state.currentSession = null;
      state.sessionStatus = "idle";
      state.incomingRequest = null;
      state.isViewerOpen = false;
      state.error = null;
    },

    setCurrentSession(state, action) {
      state.currentSession = action.payload;
    },
  },

  extraReducers: (builder) => {
    // ── Initiate ──────────────────────────────────────────────────────────────
    builder
      .addCase(initiateRemoteSession.pending, (state) => {
        state.isInitiating = true;
        state.error = null;
      })
      .addCase(initiateRemoteSession.fulfilled, (state, action) => {
        state.isInitiating = false;
        state.currentSession = action.payload;
        state.sessionStatus = "pending";
        toast.info("Remote access request sent. Waiting for user to accept...");
      })
      .addCase(initiateRemoteSession.rejected, (state, action) => {
        state.isInitiating = false;
        state.error = action.payload;
        toast.error(action.payload);
      });

    // ── Respond ───────────────────────────────────────────────────────────────
    builder
      .addCase(respondToRemoteSession.pending, (state) => {
        state.isResponding = true;
      })
      .addCase(respondToRemoteSession.fulfilled, (state, action) => {
        state.isResponding = false;
        state.currentSession = action.payload;
        state.incomingRequest = null;
        state.sessionStatus = action.payload.status.toLowerCase();
      })
      .addCase(respondToRemoteSession.rejected, (state, action) => {
        state.isResponding = false;
        state.error = action.payload;
        toast.error(action.payload);
      });

    // ── End ───────────────────────────────────────────────────────────────────
    builder
      .addCase(endRemoteSession.pending, (state) => {
        state.isEnding = true;
      })
      .addCase(endRemoteSession.fulfilled, (state, action) => {
        state.isEnding = false;
        state.currentSession = action.payload;
        state.sessionStatus = "ended";
        state.isViewerOpen = false;
      })
      .addCase(endRemoteSession.rejected, (state, action) => {
        state.isEnding = false;
        state.error = action.payload;
        toast.error(action.payload);
      });

    // ── Fetch latest ──────────────────────────────────────────────────────────
    builder
      .addCase(fetchLatestRemoteSession.pending, (state) => {
        state.isFetching = true;
      })
      .addCase(fetchLatestRemoteSession.fulfilled, (state, action) => {
        state.isFetching = false;
        state.currentSession = action.payload;
        state.sessionStatus = action.payload?.status?.toLowerCase() ?? "idle";
      })
      .addCase(fetchLatestRemoteSession.rejected, (state) => {
        state.isFetching = false;
      });

    // ── Fetch history ─────────────────────────────────────────────────────────
    builder.addCase(fetchRemoteSessionHistory.fulfilled, (state, action) => {
      state.sessionHistory = action.payload ?? [];
    });
  },
});

export const {
  setIncomingRequest,
  clearIncomingRequest,
  setSessionAccepted,
  setSessionRejected,
  setSessionActive,
  setSessionEnded,
  openViewer,
  closeViewer,
  clearSession,
  setCurrentSession,
} = remoteSlice.actions;

export default remoteSlice.reducer;

// ── Selectors ─────────────────────────────────────────────────────────────────
export const selectCurrentSession = (state) => state.remote.currentSession;
export const selectSessionStatus = (state) => state.remote.sessionStatus;
export const selectIncomingRequest = (state) => state.remote.incomingRequest;
export const selectIsViewerOpen = (state) => state.remote.isViewerOpen;
export const selectIsInitiating = (state) => state.remote.isInitiating;
export const selectIsEnding = (state) => state.remote.isEnding;
export const selectSessionHistory = (state) => state.remote.sessionHistory;
export const selectIsResponding = (state) => state.remote.isResponding;
