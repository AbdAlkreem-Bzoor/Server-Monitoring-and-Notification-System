namespace MessageBroker.RabbitMQ.Options;

public sealed class RabbitMqOptions
{
    public ConnectionOptions Connection { get; set; } = new();
    public ExchangeOptions Exchange { get; set; } = new();
    public QueueOptions Queue { get; set; } = new();
    public PublishOptions Publish { get; set; } = new();
    public ConsumeOptions Consume { get; set; } = new();
    public ChannelOptions Channel { get; set; } = new();
}