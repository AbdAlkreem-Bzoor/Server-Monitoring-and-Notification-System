using MessageProcessingAnomalyDetection.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessingAnomalyDetection.Statistics
{
    public class ServerStatistics : IServerStatistics
    {
        public string ServerIdentifier { get; set; } = null!;
        public double MemoryUsage { get; set; } // in MB
        public double AvailableMemory { get; set; } // in MB
        public double CpuUsage { get; set; } // percentage
        public DateTime Timestamp { get; set; }
        public override string ToString()
        {
            return $"CPU Usage: {CpuUsage}%\n" +
                   $"Available Memory: {AvailableMemory} MB\n" +
                   $"Memory Usage: {MemoryUsage} MB" +
                   $"Server Identifier: {ServerIdentifier}";
        }
    }
}
