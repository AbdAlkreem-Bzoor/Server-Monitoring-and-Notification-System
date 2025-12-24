using MessageProcessingAndAnomalyDetection.Abstractions;
using MessageProcessingAndAnomalyDetection.Models;
using MongoDB.Driver;

namespace MessageProcessingAndAnomalyDetection.Data;

public sealed class MongoDbContext : IMongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoDatabase database)
    {
        _database = database;
    }

    public IMongoCollection<ServerStatistics> Statistics =>
        _database.GetCollection<ServerStatistics>(ServerStatistics.CollectionName);
}
