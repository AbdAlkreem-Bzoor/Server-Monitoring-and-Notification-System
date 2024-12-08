using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Task4_RabbitMQClientLibrary.Interfaces;
using Task4_RabbitMQClientLibrary.Statistics;

namespace Task4_RabbitMQClientLibrary.Database.MongoDB
{
    public class DatabaseMongoDBRabbitMQ : IDatabaseMongoDB
    {
        private readonly MongoClient client;
        private readonly IMongoDatabase db;
        private readonly IMongoCollection<IServerStatistics> collection;

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
            collection = db.GetCollection<IServerStatistics>(settings.CollectionName);
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
