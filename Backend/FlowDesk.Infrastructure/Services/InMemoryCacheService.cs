using Microsoft.Extensions.Caching.Memory;
using FlowDesk.Application.Features.Chat.Interfaces;

namespace FlowDesk.Infrastructure.Services
{
    public class InMemoryCacheService(IMemoryCache cache) : ICacheService
    {
        public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiry)
        {
            if (cache.TryGetValue(key, out T? cached) && cached is not null)
                return cached;

            var value = await factory();
            if (value is not null)
                cache.Set(key, value, expiry);
            return value;
        }
    }
}
