using MessageProcessingAndAnomalyDetection.Models;
using MongoDB.Driver;

namespace MessageProcessingAndAnomalyDetection.Abstractions;

public interface IMongoDbContext
{
    public IMongoCollection<ServerStatistics> Statistics { get; }
}
