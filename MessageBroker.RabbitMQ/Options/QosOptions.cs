namespace MessageBroker.RabbitMQ.Options;

public sealed class QosOptions
{
    public uint PrefetchSize { get; set; } = 0;
    public ushort PrefetchCount { get; set; } = 10;
    public bool Global { get; set; } = false;
}
