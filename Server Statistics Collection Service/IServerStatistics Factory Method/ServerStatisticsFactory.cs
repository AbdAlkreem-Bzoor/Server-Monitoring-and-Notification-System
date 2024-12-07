using Server_Statistics_Collection_Service.Interfaces;
using Server_Statistics_Collection_Service.Statistics;
using System.Diagnostics;

namespace Server_Statistics_Collection_Service.Extentions
{
    public class ServerStatisticsFactory : IServerStatisticsFactory
    {
        public IServerStatistics GetStatistics()
        {
#pragma warning disable

            var cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            var cpuUsage = cpu.NextValue();

            var memory = new PerformanceCounter("Memory", "Available MBytes");
            var availableMemory = memory.NextValue();

#pragma warning disable

            var currentProcess = Process.GetCurrentProcess();
            long memoryInBytes = currentProcess.PrivateMemorySize64;
            double memoryInMB = memoryInBytes / (1024.0 * 1024.0);

            return new ServerStatistics
            {
                MemoryUsage = memoryInMB,
                AvailableMemory = availableMemory,
                CpuUsage = cpuUsage,
                Timestamp = DateTime.Now
            };
        }
    }
}
