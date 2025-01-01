using MongoDB.Bson;

namespace delayedpolufabrikkatbot.Models.Sessions
{
	public class PostCreationSession : BaseSession
	{
		public PostCreationStep CurrentStep { get; set; }
		public ObjectId UserId { get; set; }
		public ObjectId? PostId { get; set; }
	}

	public enum PostCreationStep
	{
		WaitingForTitle,
		WaitingForContent,
	}
}
