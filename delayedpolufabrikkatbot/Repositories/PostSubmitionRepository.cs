using delayedpolufabrikkatbot.Interfaces;
using delayedpolufabrikkatbot.Models;
using delayedpolufabrikkatbot.Options;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Polufabrikkat.Core.Extentions;

namespace delayedpolufabrikkatbot.Repositories
{
    public class PostSubmitionRepository : IPostSubmitionRepository
    {
        private readonly MongoClient _mongoClient;
        private readonly MongoDbOptions _mongoDbOptions;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<PostSubmition> _postsCollection;
        private readonly IMongoCollection<User> _userCollection;

        public PostSubmitionRepository(MongoClient mongoClient, IOptions<MongoDbOptions> mongoDbOptions)
        {
            _mongoClient = mongoClient;
            _mongoDbOptions = mongoDbOptions.Value;
            _database = _mongoClient.GetDatabase(_mongoDbOptions.DatabaseName);
            _postsCollection = _database.GetCollection<PostSubmition>();
        }

        public async Task UpdaterPostTitleAndUserID(ObjectId userId, string postTitle)
        {
            var newPost = new PostSubmition
            {
                UserId = userId,
                PostTitle = postTitle,
                Date = DateTime.UtcNow
            };

            await _postsCollection.InsertOneAsync(newPost);
        }

        public async Task<List<PostSubmition>> GetLastPostsByUserId(ObjectId userId, int limit)
        {
            var filter = Builders<PostSubmition>.Filter.Eq(post => post.UserId, userId);
            var sort = Builders<PostSubmition>.Sort.Descending(post => post.Date);

            var posts = await _postsCollection.Find(filter)
                                               .Sort(sort)
                                               .Limit(limit)
                                               .ToListAsync();

            return posts;
        }

        public async Task UpdatePostReputation(ObjectId postId, int reputation)
        {
            var filter = Builders<PostSubmition>.Filter.Eq(post => post.Id, postId);
            var update = Builders<PostSubmition>.Update.Set(post => post.ChangeReputation, reputation);

            await _postsCollection.UpdateOneAsync(filter, update);
        }
    }
}