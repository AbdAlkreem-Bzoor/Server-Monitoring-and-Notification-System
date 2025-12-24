using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MessageProcessingAndAnomalyDetection.Models;

public class ServerStatistics
{
    public const string CollectionName = "server_statistics";

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("server_identifier")]
    public string ServerIdentifier { get; set; } = string.Empty;

    [BsonElement("memory_usage")]
    public required double MemoryUsage { get; set; } // in MB

    [BsonElement("available_memory")]
    public required double AvailableMemory { get; set; } // in MB

    [BsonElement("cpu_usage")]
    public required double CpuUsage { get; set; }

    [BsonElement("timestamp")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public required DateTime Timestamp { get; set; }
}
