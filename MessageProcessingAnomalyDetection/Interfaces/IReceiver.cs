namespace MessageProcessingAnomalyDetection.Interfaces
{
    public interface IReceiver
    {
        public IServerStatistics? GetMessage();
        public IEnumerable<IServerStatistics>? GetMessages();
    }

}
