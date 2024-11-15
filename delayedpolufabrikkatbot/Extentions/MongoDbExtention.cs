using MongoDB.Driver;
using System.ComponentModel.DataAnnotations.Schema;

namespace Polufabrikkat.Core.Extentions
{
    public static class MongoDbExtention
    {
        public static IMongoCollection<T> GetCollection<T>(this IMongoDatabase database, string collectionName = null)
        {
            if (!string.IsNullOrEmpty(collectionName))
            {
                return database.GetCollection<T>(collectionName);
            }

            var attribute = (TableAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(TableAttribute));
            if (attribute == null)
            {
                return database.GetCollection<T>(typeof(T).Name);
            }
            return database.GetCollection<T>(attribute.Name);
        }
    }
}
