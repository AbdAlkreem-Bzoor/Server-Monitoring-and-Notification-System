using MessageProcessingAndAnomalyDetection.Abstractions;
using MessageProcessingAndAnomalyDetection.Models;
using Microsoft.Extensions.Options;

namespace MessageProcessingAndAnomalyDetection.Services;

public class AnomalyAlertDetectionService : IAnomalyAlertDetectionService
{
    private readonly AlertsDetectionConfiguration _settings;

    public AnomalyAlertDetectionService(IOptions<AlertsDetectionConfiguration> options)
    {
        _settings = options.Value;
    }

    public bool CheckAlert(ServerStatistics current, ServerStatistics previous, out string message)
    {
        if (current.MemoryUsage > (previous.MemoryUsage * (1 + _settings.MemoryUsageAnomalyThresholdPercentage)))
        {
            message = "Memory Usage Anomaly Alert";
            return true;
        }

        if (current.CpuUsage > (previous.CpuUsage * (1 + _settings.CpuUsageAnomalyThresholdPercentage)))
        {
            message = "CPU Usage Anomaly Alert";
            return true;
        }

        message = string.Empty;
        return false;
    }
}
