using delayedpolufabrikkatbot.Interfaces;
using delayedpolufabrikkatbot.Models;
using delayedpolufabrikkatbot.Options;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Polufabrikkat.Core.Extentions;

namespace delayedpolufabrikkatbot.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MongoClient _mongoClient;
        private readonly MongoDbOptions _mongoDbOptions;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<User> _userCollection;

        public UserRepository(MongoClient mongoClient, IOptions<MongoDbOptions> mongoDbOptions)
        {
            _mongoClient = mongoClient;
            _mongoDbOptions = mongoDbOptions.Value;
            _database = _mongoClient.GetDatabase(_mongoDbOptions.DatabaseName);
            _userCollection = _database.GetCollection<User>();
        }

        public async Task AddIfNotExist(long telegramId)
        {
            var filter = Builders<User>.Filter.Eq(u => u.TelegramId, telegramId);
            if(await _userCollection.Find(filter).AnyAsync())
            {
                return;
            }
            var newUser = new User()
            {
                TelegramId = telegramId,
            };
            await _userCollection.InsertOneAsync(newUser);
        }

        public async Task<User> CreateUser(User newUser)
        {
            await _userCollection.InsertOneAsync(newUser);
            return newUser;
        }

        public Task<User> GetUserById(ObjectId userId)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var user = _userCollection.Find(filter).FirstOrDefaultAsync();
            return user;
        }

        public Task<User> GetUserByTelegramId(long telegramId)
        {
            var filter = Builders<User>.Filter.Eq(u => u.TelegramId, telegramId);
            return _userCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<User> AddReputationToUser(long telegramId, int reputationPoints)
        {
            // Фильтр для поиска пользователя по Telegram ID
            var filter = Builders<User>.Filter.Eq(u => u.TelegramId, telegramId);

            // Обновление репутации: увеличение текущего значения
            var update = Builders<User>.Update.Inc(u => u.Reputation, reputationPoints);

            // Возвращаем обновленный документ пользователя
            var options = new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After // Возвращать обновленный документ
            };

            return await _userCollection.FindOneAndUpdateAsync(filter, update, options);
        }
    }
}
