namespace MessageProcessingAnomalyDetection.Interfaces
{
    public interface IAlertDetector
    {
        public void CheckForAlerts(IServerStatistics statistics);
    }
}
