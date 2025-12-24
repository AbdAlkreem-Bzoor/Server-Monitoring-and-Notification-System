using ServerStatisticsCollectionService.Abstractions;
using ServerStatisticsCollectionService.Models;
using System.Diagnostics;

namespace ServerStatisticsCollectionService.StatisticsProviders;

public sealed class WindowsStatisticsProvider : ServerStatisticsProvider
{

    public override ServerStatistics GetServerStatistics()
    {
        return new ServerStatistics
        {
            MemoryUsage = GetMemoryUsage(),
            AvailableMemory = GetAvailableMemory(),
            CpuUsage = GetCpuUsage(),
            Timestamp = DateTime.UtcNow
        };
    }

#pragma warning disable
    protected override double GetAvailableMemory()
    {
        using var availableMemoryCounter = new PerformanceCounter("Memory", "Available MBytes");

        return availableMemoryCounter.NextValue();
    }

    protected override double GetCpuUsage()
    {
        using var cpuUsageCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        return cpuUsageCounter.NextValue();
    }

    protected override double GetMemoryUsage()
    {
        var memoryUsageCounter = new PerformanceCounter("Memory", "Committed Bytes");

        return memoryUsageCounter.NextValue() / (1024.0 * 1024.0);
    }
#pragma warning disable
}
