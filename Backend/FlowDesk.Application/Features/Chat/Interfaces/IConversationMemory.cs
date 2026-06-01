using FlowDesk.Application.Features.Chat.DTOs;

namespace FlowDesk.Application.Features.Chat.Interfaces
{
    public interface IConversationMemory
    {
        IReadOnlyList<ConversationMessage> GetHistory(int userId);
        void AddMessage(int userId, string role, string content);
        void Clear(int userId);
    }
}
