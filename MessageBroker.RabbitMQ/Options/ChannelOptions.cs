namespace MessageBroker.RabbitMQ.Options;

public sealed class ChannelOptions
{
    public bool PublisherConfirmationsEnabled { get; set; } = true;
    public bool PublisherConfirmationTrackingEnabled { get; set; } = true;
}