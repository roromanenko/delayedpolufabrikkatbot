using delayedpolufabrikkatbot.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace delayedpolufabrikkatbot.Service
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public string GetCache(long telegramId)
        {
            string cacheValue;

            _memoryCache.TryGetValue(telegramId, out cacheValue);
            
            return cacheValue;
        }

        public string SetCache(long telegramId, string caheValue)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            };

            _memoryCache.Set(telegramId, caheValue);
            return caheValue;
        }
        public void ClearCache(long telegramId)
        {
            _memoryCache.Remove(telegramId);
        }
    }
}

