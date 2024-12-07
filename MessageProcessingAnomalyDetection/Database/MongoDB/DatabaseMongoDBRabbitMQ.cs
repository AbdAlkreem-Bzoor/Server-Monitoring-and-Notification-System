using MessageProcessingAnomalyDetection.Interfaces;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using MessageProcessingAnomalyDetection.Statistics;
using MongoDB.Bson.IO;

namespace MessageProcessingAnomalyDetection.Database.MongoDB
{
    public class DatabaseMongoDBRabbitMQ : IDatabaseMongoDB
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        private readonly IMongoCollection<ServerStatistics> collection;

        public DatabaseMongoDBRabbitMQ()
        {
            var config = new ConfigurationBuilder()
                                     .AddJsonFile("appsettings.json")
                                     .Build();
            var settings = new
            {
                ConnectionString = config.GetSection("MongoDBConfig:ConnectionString").Value ?? string.Empty,
                DatabaseName = config.GetSection("MongoDBConfig:DatabaseName").Value ?? string.Empty,
                CollectionName = config.GetSection("MongoDBConfig:CollectionName").Value ?? string.Empty
            };

            client = new MongoClient(settings.ConnectionString);
            db = client.GetDatabase(settings.DatabaseName);
            collection = db.GetCollection<ServerStatistics>(settings.CollectionName);
        }

        public Task<bool> Delete(IServerStatistics statistics, bool deleteMany)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IServerStatistics>> GetAllStatistics()
        {
            throw new NotImplementedException();
        }

        public async Task Insert(ServerStatistics statistics)
        {
            await collection.InsertOneAsync(statistics);
        }

        public Task<bool> Update(IServerStatistics statistics)
        {
            throw new NotImplementedException();
        }
    }
}
