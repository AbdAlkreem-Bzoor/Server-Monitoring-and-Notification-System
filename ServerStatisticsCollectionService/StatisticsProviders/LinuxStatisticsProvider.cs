using ServerStatisticsCollectionService.Abstractions;
using ServerStatisticsCollectionService.Models;

namespace ServerStatisticsCollectionService.StatisticsProviders;

public sealed class LinuxStatisticsProvider : ServerStatisticsProvider
{
    public override ServerStatistics GetServerStatistics()
    {
        return new ServerStatistics()
        {
            CpuUsage = GetCpuUsage(),
            AvailableMemory = GetAvailableMemory(),
            MemoryUsage = GetMemoryUsage(),
            Timestamp = DateTime.UtcNow
        };
    }

    protected override double GetAvailableMemory()
    {
        var availableMemoryInfo = File.ReadLines("/proc/meminfo").Skip(2).First();

        var availableMemoryInKb =
          double.Parse(availableMemoryInfo.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);

        return availableMemoryInKb / 1024.0;
    }

    protected override double GetCpuUsage()
    {
        var cpuInfo = File.ReadLines("/proc/stat").First()
                          .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                          .Skip(1)
                          .ToArray();

        var idleTime = double.Parse(cpuInfo[3]);

        var totalTime = cpuInfo.Aggregate(0.0, (total, num) => total + double.Parse(num));

        return 1 - idleTime / totalTime;
    }

    protected override double GetMemoryUsage()
    {
        var memoryInfo = File.ReadLines("/proc/meminfo");

        var totalMemoryInfo = memoryInfo.First();

        var availableMemoryInfo = memoryInfo.Skip(2).First();

        var totalMemoryInKb = double.Parse(totalMemoryInfo.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);

        var availableMemoryInKb =
          double.Parse(availableMemoryInfo.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]);

        return (totalMemoryInKb - availableMemoryInKb) / 1024;
    }
}