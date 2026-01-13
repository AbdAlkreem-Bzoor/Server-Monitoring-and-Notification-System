namespace MessageBroker.RabbitMQ.Options;

public sealed class RabbitMqOptions
{
    internal ConnectionOptions Connection { get; set; } = new();
    internal ExchangeOptions Exchange { get; set; } = new();
    internal QueueOptions Queue { get; set; } = new();
    internal PublishOptions Publish { get; set; } = new();
    internal ConsumeOptions Consume { get; set; } = new();
    internal ChannelOptions Channel { get; set; } = new();
}