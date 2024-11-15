using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace delayedpolufabrikkatbot.Models
{
    [Table("posts")]
    public class PostSubmition
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ObjectId UserId { get; set; }
        public string? PostTitle { get; set; }
        public string? Data { get; set; }
        public int ChangeReputation { get; set; }
        public DateTime Date { get; set; }
    }
}
