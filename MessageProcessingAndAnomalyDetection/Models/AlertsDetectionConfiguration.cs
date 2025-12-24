namespace MessageProcessingAndAnomalyDetection.Models;

public class AlertsDetectionConfiguration
{
    public required double MemoryUsageAnomalyThresholdPercentage { get; set; }
    public required double CpuUsageAnomalyThresholdPercentage { get; set; }
    public required double MemoryUsageThresholdPercentage { get; set; }
    public required double CpuUsageThresholdPercentage { get; set; }
}          