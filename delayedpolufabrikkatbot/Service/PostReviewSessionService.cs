using delayedpolufabrikkatbot.Interfaces;
using delayedpolufabrikkatbot.Models;
using Microsoft.Extensions.Caching.Memory;

namespace delayedpolufabrikkatbot.Service
{
	public class PostReviewSessionService : IPostReviewSessionService
	{
		private readonly IMemoryCache _memoryCache;
		private readonly TimeSpan _cacheTime = TimeSpan.FromDays(5);

		public PostReviewSessionService(IMemoryCache memoryCache)
		{
			_memoryCache = memoryCache;
		}

		public void AddReviewSessionItem(string key, ReviewPublicationSession item)
		{
			_memoryCache.Set(key, item, _cacheTime);
		}

		public bool TryGetReviewSessionItem(string key, out ReviewPublicationSession item)
		{
			return _memoryCache.TryGetValue(key, out item);
		}
	}
}
