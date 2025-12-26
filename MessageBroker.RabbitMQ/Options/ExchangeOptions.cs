using RabbitMQ.Client;

namespace MessageBroker.RabbitMQ.Options;

public sealed class ExchangeOptions
{
    public string ExchangeName { get; set; } = "exchange";
    public string RoutingKey { get; set; } = "routing_key";
    public string Type { get; set; } = ExchangeType.Topic;
    public bool Durable { get; set; } = true;
    public bool AutoDelete { get; set; } = false;
    public IDictionary<string, object?>? Arguments { get; set; }
}
