import { createSlice } from "@reduxjs/toolkit";

function loadSaved() {
  try {
    const saved = localStorage.getItem("chat_messages");
    return saved ? JSON.parse(saved) : [];
  } catch {
    return [];
  }
}

function saveToDisk(messages) {
  try {
    localStorage.setItem("chat_messages", JSON.stringify(messages));
  } catch {
    /* quota exceeded, ignore */
  }
}

const initialState = {
  messages: loadSaved(),
};

const chatSlice = createSlice({
  name: "chat",
  initialState,
  reducers: {
    addMessage(state, action) {
      state.messages.push(action.payload);
      if (state.messages.length > 100) {
        state.messages = state.messages.slice(-100);
      }
      saveToDisk(state.messages);
    },
    addMessages(state, action) {
      state.messages.push(...action.payload);
      if (state.messages.length > 100) {
        state.messages = state.messages.slice(-100);
      }
      saveToDisk(state.messages);
    },
    clearChat(state) {
      state.messages = [];
      localStorage.removeItem("chat_messages");
    },
    replaceLastMessage(state, action) {
      if (state.messages.length > 0) {
        state.messages[state.messages.length - 1] = action.payload;
      } else {
        state.messages.push(action.payload);
      }
      saveToDisk(state.messages);
    },
  },
});

export const { addMessage, addMessages, clearChat, replaceLastMessage } = chatSlice.actions;
export const selectChatMessages = (state) => state.chat.messages;
export default chatSlice.reducer;
