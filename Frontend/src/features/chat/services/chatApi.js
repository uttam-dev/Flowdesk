import { apiClient } from "../../../services/apiClient.js";

export async function sendChatMessageApi(message) {
  const { data } = await apiClient.post("/chat", { message });
  const response = data?.data?.response ?? data?.response ?? "";
  const isCommandHandled = data?.data?.isCommandHandled ?? data?.isCommandHandled ?? false;
  return { response, isCommandHandled };
}
