namespace Rabbit_MQ_Client_Library.Interfaces
{
    public interface IMessageQueueReceiver
    {
        public IServerStatistics? GetMessage();
        public IEnumerable<IServerStatistics>? GetMessages();
        public IDatabaseMongoDB Database { get; set; }
    }
}