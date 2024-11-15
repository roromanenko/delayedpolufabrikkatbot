using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace delayedpolufabrikkatbot.Models
{
    [Table("users")]
    public class User
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public long TelegramId { get; set; }
        public int Reputation { get; set; }
    }
}
