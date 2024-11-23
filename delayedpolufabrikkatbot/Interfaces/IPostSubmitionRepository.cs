using delayedpolufabrikkatbot.Models;
using MongoDB.Bson;

namespace delayedpolufabrikkatbot.Interfaces
{
    public interface IPostSubmitionRepository
    {
        Task<ObjectId> CreatePostWithTitleAndUserID(ObjectId userId, string postTitle);

        Task<List<PostSubmition>> GetLastPostsByUserId(ObjectId userId, int limit);

        Task UpdatePostReputation(ObjectId postId, int reputation);
    }
}