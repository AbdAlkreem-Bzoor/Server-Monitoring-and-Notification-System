namespace MessageBroker.RabbitMQ.Options;

public sealed class NackOptions
{
    public bool Multiple { get; set; } = true;
    public bool Requeue { get; set; } = true;
}
