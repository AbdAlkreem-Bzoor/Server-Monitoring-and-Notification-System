using MessageProcessingAndAnomalyDetection.Models;

namespace MessageProcessingAndAnomalyDetection.Abstractions;

public interface IHighUsageAlertDetectionService
{
    bool CheckAlert(ServerStatistics current, out string message);
}