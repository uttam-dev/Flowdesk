using FlowDesk.Application.Features.Chat.DTOs;

namespace FlowDesk.Application.Features.Chat.Interfaces
{
    public interface IChatbotService
    {
        Task<string> GetResponseAsync(
            string message,
            string systemPrompt,
            string contextData,
            IReadOnlyList<ConversationMessage> conversationHistory,
            int maxTokens,
            CancellationToken cancellationToken = default);
    }
}
