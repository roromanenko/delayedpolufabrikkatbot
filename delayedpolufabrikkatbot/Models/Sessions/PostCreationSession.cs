using MongoDB.Bson;

namespace delayedpolufabrikkatbot.Models.Sessions
{
	public class PostCreationSession : BaseUserSession
	{
		public PostCreationStep CurrentStep { get; set; }

		public ObjectId? PostId { get; set; }
	}

	public enum PostCreationStep
	{
		WaitingForTitle,
		WaitingForContent,
	}
}
