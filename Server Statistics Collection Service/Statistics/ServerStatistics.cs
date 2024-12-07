using Server_Statistics_Collection_Service.Interfaces;
using System.Text.Json.Serialization;

namespace Server_Statistics_Collection_Service.Statistics
{
    public class ServerStatistics : IServerStatistics
    {
        public double MemoryUsage { get; set; } // in MB
        public double AvailableMemory { get; set; } // in MB
        public double CpuUsage { get; set; } // Percentage
        public DateTime Timestamp { get; set; }
        [JsonIgnore]
        public double CpuUsageInMB => (CpuUsage / 100) * AvailableMemory; // Estimated
        public override string ToString()
        {
            return $"CPU Usage: {CpuUsage}% - {CpuUsageInMB} MB\n" +
                   $"Available Memory: {AvailableMemory} MB\n" +
                   $"Memory Usage: {MemoryUsage} MB";
        }
    }

}
