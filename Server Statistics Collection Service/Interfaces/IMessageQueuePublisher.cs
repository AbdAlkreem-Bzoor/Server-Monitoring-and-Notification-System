namespace Server_Statistics_Collection_Service.Interfaces
{
    public interface IMessageQueuePublisher
    {
        public IServerStatistics Statistics { get; init; }
        public bool PublishMessage();
        public bool PublishMessages(int count);
    }
}
