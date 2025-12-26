using RabbitMQ.Client;

namespace MessageBroker.RabbitMQ.Options;

public sealed class PublishOptions
{
    public DeliveryModes DeliveryMode { get; set; } = DeliveryModes.Persistent;
    public bool Mandatory { get; set; } = false;
    public byte Priority { get; set; } = 0;
    public string? Expiration { get; set; }

    public string? MessageId { get; } = Guid.NewGuid().ToString();
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    public string? CorrelationId { get; set; }
    public string? ReplyTo { get; set; }

    public string? Type { get; set; }
    public string? AppId { get; set; }
    public IDictionary<string, object?>? Headers { get; set; }
}
