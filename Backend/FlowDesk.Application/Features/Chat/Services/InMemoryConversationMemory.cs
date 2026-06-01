using System.Collections.Concurrent;
using FlowDesk.Application.Features.Chat.DTOs;
using FlowDesk.Application.Features.Chat.Interfaces;

namespace FlowDesk.Application.Features.Chat.Services
{
    public class InMemoryConversationMemory : IConversationMemory
    {
        private static readonly ConcurrentDictionary<int, List<ConversationMessage>> _store = new();
        private const int MaxMessages = 8;

        public IReadOnlyList<ConversationMessage> GetHistory(int userId)
        {
            return _store.TryGetValue(userId, out var messages)
                ? messages.AsReadOnly()
                : Array.Empty<ConversationMessage>();
        }

        public void AddMessage(int userId, string role, string content)
        {
            var trimmed = content.Length > 300 ? content[..300] + "..." : content;

            var messages = _store.GetOrAdd(userId, _ => new List<ConversationMessage>());
            lock (messages)
            {
                messages.Add(new ConversationMessage { Role = role, Content = trimmed });
                if (messages.Count > MaxMessages)
                    messages.RemoveAt(0);
            }
        }

        public void Clear(int userId)
        {
            _store.TryRemove(userId, out _);
        }
    }
}
