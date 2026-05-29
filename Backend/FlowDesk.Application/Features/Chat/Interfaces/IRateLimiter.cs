namespace FlowDesk.Application.Features.Chat.Interfaces
{
    public interface IRateLimiter
    {
        bool IsAllowed(int userId, int maxRequests, TimeSpan window);
    }
}
