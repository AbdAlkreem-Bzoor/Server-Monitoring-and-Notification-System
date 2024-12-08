using MessageProcessingAnomalyDetection.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MessageProcessingAnomalyDetection.Statistics
{
    public class ServerStatistics : IServerStatistics
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public string ServerIdentifier { get; set; } = null!;
        public double MemoryUsage { get; set; } // in MB
        public double AvailableMemory { get; set; } // in MB
        public double CpuUsage { get; set; } // percentage
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return $"CPU Usage: {CpuUsage}%\n" +
                   $"Available Memory: {AvailableMemory} MB\n" +
                   $"Memory Usage: {MemoryUsage} MB\n" +
                   $"Server Identifier: {ServerIdentifier}";
        }
    }
}
