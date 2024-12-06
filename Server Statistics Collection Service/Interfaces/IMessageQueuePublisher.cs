namespace Server_Statistics_Collection_Service.Interfaces
{
    public interface IMessageQueuePublisher : IPublisher
    {
        public IServerStatistics Statistics { get; init; }
    }
}
