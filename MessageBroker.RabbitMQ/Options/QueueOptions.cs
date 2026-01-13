namespace MessageBroker.RabbitMQ.Options;

public sealed class QueueOptions
{
    public string QueueName { get; set; } = "queue";
    public string RoutingKey { get; set; } = "routing_key";
    internal QosOptions Qos { get; set; } = new();
    internal AckOptions Ack { get; set; } = new();
    internal NackOptions Nack { get; set; } = new();
    public bool Durable { get; set; } = true;
    public bool Exclusive { get; set; } = false;
    public bool AutoDelete { get; set; } = false;
    public IDictionary<string, object?> Arguments { get; set; } = new Dictionary<string, object?>
    {
        { "x-queue-type", "quorum" } // Modern default for reliability
    };
}
