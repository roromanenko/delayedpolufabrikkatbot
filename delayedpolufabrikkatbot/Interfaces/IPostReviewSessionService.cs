using delayedpolufabrikkatbot.Models;
using Microsoft.Extensions.Caching.Memory;

namespace delayedpolufabrikkatbot.Interfaces
{
	public interface IPostReviewSessionService
	{
		void AddReviewSessionItem(string key, ReviewPublicationSession item);
		bool TryGetReviewSessionItem(string key, out ReviewPublicationSession item);
	}
}
