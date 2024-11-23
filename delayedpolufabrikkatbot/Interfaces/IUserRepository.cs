using delayedpolufabrikkatbot.Models;
using MongoDB.Bson;

namespace delayedpolufabrikkatbot.Interfaces
{
    public interface IUserRepository
    {
        Task AddIfNotExist(long id);
        public Task<User> CreateUser(User newUser);
        public Task<User> GetUserById(ObjectId userId);
        public Task<User> GetUserByTelegramId(long telegramId);
        public Task<User> AddReputationToUser(long telegramId, int reputationPoints);
    }
}