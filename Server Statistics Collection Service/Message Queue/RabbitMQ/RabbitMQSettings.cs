namespace Server_Statistics_Collection_Service.Message_Queue.RabbitMQ
{
    public class RabbitMQSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string ExchangeName { get; set; } = null!;
        public string QueueName { get; set; } = null!;
        public string RoutingKey { get; set; } = null!;
    }
}
