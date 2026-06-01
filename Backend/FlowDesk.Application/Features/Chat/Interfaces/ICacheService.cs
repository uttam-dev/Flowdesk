namespace FlowDesk.Application.Features.Chat.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiry);
    }
}
