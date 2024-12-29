using delayedpolufabrikkatbot.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace delayedpolufabrikkatbot.Utilities
{
	public class CacheManager : ICacheManager
	{
		private readonly IMemoryCache _memoryCache;
		private readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(15);

		public CacheManager(IMemoryCache memoryCache)
		{
			_memoryCache = memoryCache;
		}

		public void Add<T>(object key, T value, TimeSpan? timeout = default)
		{
			_memoryCache.Set(key, value, timeout ?? _defaultTimeout);
		}

		public void Remove(object key)
		{
			_memoryCache.Remove(key);
		}

		public bool TryGet<T>(object key, out T value)
		{
			return _memoryCache.TryGetValue(key, out value);
		}
	}
}
