namespace Rabbit_MQ_Client_Library.Interfaces
{
    public interface IMessageQueuePublisher
    {
        public IServerStatistics Statistics { get; init; }
        public bool PublishMessage();
        public bool PublishMessages(int count);
    }
}