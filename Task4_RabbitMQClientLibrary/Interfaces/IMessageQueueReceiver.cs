namespace Task4_RabbitMQClientLibrary.Interfaces
{
    public interface IMessageQueueReceiver
    {
        public IServerStatistics? GetMessage();
        public IEnumerable<IServerStatistics>? GetMessages();
        public IDatabaseMongoDB Database { get; set; }
    }
}
