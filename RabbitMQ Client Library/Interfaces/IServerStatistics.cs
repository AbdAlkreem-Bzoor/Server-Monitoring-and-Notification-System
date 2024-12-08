namespace Rabbit_MQ_Client_Library.Interfaces
{
    public interface IServerStatistics
    {
        public double MemoryUsage { get; set; } // in MB
        public double AvailableMemory { get; set; } // in MB
        public double CpuUsage { get; set; } // percentage
        public DateTime Timestamp { get; set; }
    }
}