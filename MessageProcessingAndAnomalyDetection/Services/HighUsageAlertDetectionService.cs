using MessageProcessingAndAnomalyDetection.Abstractions;
using MessageProcessingAndAnomalyDetection.Models;
using Microsoft.Extensions.Options;

namespace MessageProcessingAndAnomalyDetection.Services;

public class HighUsageAlertDetectionService : IHighUsageAlertDetectionService
{
    private readonly AlertsDetectionConfiguration _settings;

    public HighUsageAlertDetectionService(IOptions<AlertsDetectionConfiguration> options)
    {
        _settings = options.Value;
    }

    public bool CheckAlert(ServerStatistics current, out string message)
    {
        if ((current.MemoryUsage / (current.MemoryUsage + current.AvailableMemory)) > _settings.MemoryUsageThresholdPercentage)
        {
            message = "Memory High Usage Alert";
            return true;
        }

        if (current.CpuUsage > _settings.CpuUsageThresholdPercentage)
        {
            message = "CPU High Usage Alert";
            return true;
        }

        message = string.Empty;
        return false;
    }
}
