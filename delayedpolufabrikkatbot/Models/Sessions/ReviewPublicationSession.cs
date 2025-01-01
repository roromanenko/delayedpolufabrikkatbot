using MongoDB.Bson;

namespace delayedpolufabrikkatbot.Models.Sessions
{
    public class ReviewPublicationSession : BaseSession
    {
        public long TelegramUserId { get; set; }
        public ObjectId PostId { get; set; }
        public int Reputation { get; set; }
        public PublicationResolution PublicationResolution { get; set; }

    }
    public enum PublicationResolution
    {
        Approved,
        Ignored
    }
}
