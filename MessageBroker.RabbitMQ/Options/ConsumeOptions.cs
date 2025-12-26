namespace MessageBroker.RabbitMQ.Options;

public sealed class ConsumeOptions
{
    public string ConsumerTag { get; set; } = "";
    public bool NoLocal { get; set; } = false;
    public bool Exclusive { get; set; } = false;
    public bool AutoAck { get; set; } = false; 
    public IDictionary<string, object>? Arguments { get; set; } = null;
    public ushort PrefetchCount { get; set; } = 10;
}