namespace Shared.Models;

public sealed class MongoDbConfiguration
{
    public required string ConnectionString { get; init; }
    public required string DatabaseName { get; init; }
}
