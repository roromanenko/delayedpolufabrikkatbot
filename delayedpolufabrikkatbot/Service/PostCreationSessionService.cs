using delayedpolufabrikkatbot.Interfaces;
using delayedpolufabrikkatbot.Models;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;

namespace delayedpolufabrikkatbot.Service
{
	public class PostCreationSessionService : IPostCreationSessionService
	{
		private readonly IMemoryCache _memoryCache;

		public PostCreationSessionService(IMemoryCache memoryCache)
		{
			_memoryCache = memoryCache;
		}

		public void FinishPostCreationSession(ObjectId userId)
		{
			_memoryCache.Remove(userId);
		}

		public bool IsCreationSessionStarted(ObjectId userId, out PostCreationSession session)
		{
			return _memoryCache.TryGetValue(userId, out session);
		}

		public void StartPostCreationSession(ObjectId userId)
		{
			_memoryCache.Set(userId,
				new PostCreationSession { CurrentStep = PostCreationStep.WaitingForTitle },
				TimeSpan.FromMinutes(15));
		}
	}
}
