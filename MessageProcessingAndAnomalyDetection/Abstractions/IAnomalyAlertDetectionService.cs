using MessageProcessingAndAnomalyDetection.Models;

namespace MessageProcessingAndAnomalyDetection.Abstractions;

public interface IAnomalyAlertDetectionService
{
    bool CheckAlert(ServerStatistics current, ServerStatistics previous, out string message);
}
