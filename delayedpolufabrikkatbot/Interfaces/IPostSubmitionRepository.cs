using delayedpolufabrikkatbot.Models;
using MongoDB.Bson;

namespace delayedpolufabrikkatbot.Interfaces
{
    public interface IPostSubmitionRepository
    {
        Task UpdaterPostTitleAndUserID(ObjectId userId, string postTitle);

        Task<List<PostSubmition>> GetLastPostsByUserId(ObjectId userId, int limit);
    }
}