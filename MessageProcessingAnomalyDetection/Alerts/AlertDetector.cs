using MessageProcessingAnomalyDetection.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MessageProcessingAnomalyDetection.Alerts
{
    public class AlertDetector : IAlertDetector
    {
        private readonly IAlertSender _sender;
        public IServerStatistics? Statistics { get; set; }
        private readonly float MemoryUsageAnomalyThresholdPercentage;
        private readonly float CpuUsageAnomalyThresholdPercentage;
        private readonly float MemoryUsageThresholdPercentage;
        private readonly float CpuUsageThresholdPercentage;
        public AlertDetector(IAlertSender sender)
        {
            _sender = sender;

            var config = new ConfigurationBuilder()
                                     .AddJsonFile("appsettings.json")
                                     .Build();
            MemoryUsageAnomalyThresholdPercentage = float.Parse(config["AnomalyDetectionConfig:MemoryUsageAnomalyThresholdPercentage"] ?? "0");
            CpuUsageAnomalyThresholdPercentage = float.Parse(config["AnomalyDetectionConfig:CpuUsageAnomalyThresholdPercentage"] ?? "0");
            MemoryUsageThresholdPercentage = float.Parse(config["AnomalyDetectionConfig:MemoryUsageThresholdPercentage"] ?? "0");
            CpuUsageThresholdPercentage = float.Parse(config["AnomalyDetectionConfig:CpuUsageThresholdPercentage"] ?? "0");
        }
        public void CheckForAlerts(IServerStatistics statistics)
        {
            if (Statistics is not null)
            {
                CheckAnomalyAlerts(statistics);
            }
            Statistics = statistics;
            CheckHighUsageAlerts();
        }
        public void CheckAnomalyAlerts(IServerStatistics statistics)
        {
            CheckMemoryUsageAnomalyAlert(statistics);
            CheckCPUUsageAnomalyAlert(statistics);
        }

        private void CheckCPUUsageAnomalyAlert(IServerStatistics statistics)
        {
            if (statistics.CpuUsage > (Statistics?.CpuUsage * (1 + CpuUsageAnomalyThresholdPercentage)))
                _sender.SendAlertAsync("CPU Usage Anomaly Alert").Wait();
        }

        private void CheckMemoryUsageAnomalyAlert(IServerStatistics statistics)
        {
            if (statistics.MemoryUsage > (Statistics?.MemoryUsage * (1 + MemoryUsageAnomalyThresholdPercentage)))
                _sender.SendAlertAsync("Memory Usage Anomaly Alert").Wait();
        }

        public void CheckHighUsageAlerts()
        {
            CheckMemoryHighUsageAlert();
            CheckCPUHighUsageAlert();
        }

        private void CheckCPUHighUsageAlert()
        {
            if (Statistics?.CpuUsage > CpuUsageThresholdPercentage)
                _sender.SendAlertAsync("CPU High Usage Alert").Wait();
        }

        private void CheckMemoryHighUsageAlert()
        {
            if ((Statistics?.MemoryUsage / (Statistics?.MemoryUsage + Statistics?.AvailableMemory)) > MemoryUsageThresholdPercentage)
                _sender.SendAlertAsync("Memory High Usage Alert").Wait();
        }
    }
}
