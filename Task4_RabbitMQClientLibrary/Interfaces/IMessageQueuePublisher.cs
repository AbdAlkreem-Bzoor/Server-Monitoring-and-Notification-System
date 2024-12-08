namespace Task4_RabbitMQClientLibrary.Interfaces
{
    public interface IMessageQueuePublisher
    {
        public IServerStatistics Statistics { get; init; }
        public bool PublishMessage();
        public bool PublishMessages(int count);
    }
}
