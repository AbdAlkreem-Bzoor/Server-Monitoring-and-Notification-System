namespace Shared.Models;

public class ServerStatisticsConfiguration
{
    public required int SamplingIntervalSeconds { get; init; }
    public required string ServerIdentifier { get; init; }
}