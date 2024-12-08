namespace MessageProcessingAnomalyDetection.Interfaces
{
    public interface IAlertSender
    {
        public Task Start();
        Task SendAlertAsync(string message);
    }
}
