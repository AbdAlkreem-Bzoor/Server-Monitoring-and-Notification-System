using MongoDB.Driver;

namespace MessageProcessingAnomalyDetection.Interfaces
{
    public interface IMessageQueueReceiver
    {
        public IServerStatistics? GetMessage();
        public IEnumerable<IServerStatistics>? GetMessages();
        public IDatabaseMongoDB Database { get; set; }
    }
}
