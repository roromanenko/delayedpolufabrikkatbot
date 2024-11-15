using delayedpolufabrikkatbot.Models;
using MongoDB.Bson;

namespace delayedpolufabrikkatbot.Interfaces
{
	public interface IPostCreationSessionService
	{
		void FinishPostCreationSession(ObjectId userId);
		bool IsCreationSessionStarted(ObjectId userId, out PostCreationSession session);
		void StartPostCreationSession(ObjectId userId);
	}
}
