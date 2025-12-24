using ServerStatisticsCollectionService.Models;

namespace ServerStatisticsCollectionService.Abstractions;

public abstract class ServerStatisticsProvider
{
    public abstract ServerStatistics GetServerStatistics();
    protected abstract double GetMemoryUsage();
    protected abstract double GetAvailableMemory();
    protected abstract double GetCpuUsage();
}