using System.Collections.Concurrent;
using FlowDesk.Application.Features.Chat.Interfaces;

namespace FlowDesk.Application.Features.Chat.Services
{
    public class InMemoryRateLimiter : IRateLimiter
    {
        private static readonly ConcurrentDictionary<int, List<DateTime>> _store = new();

        public bool IsAllowed(int userId, int maxRequests, TimeSpan window)
        {
            var now = DateTime.UtcNow;
            var timestamps = _store.GetOrAdd(userId, _ => new List<DateTime>());

            lock (timestamps)
            {
                timestamps.RemoveAll(t => now - t > window);
                if (timestamps.Count >= maxRequests)
                    return false;

                timestamps.Add(now);
                return true;
            }
        }
    }
}
