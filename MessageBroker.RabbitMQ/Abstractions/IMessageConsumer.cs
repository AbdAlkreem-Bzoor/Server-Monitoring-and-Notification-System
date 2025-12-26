namespace MessageBroker.RabbitMQ.Abstractions;

public interface IMessageConsumer
{
    Task SubscribeAsync(CancellationToken cancellationToken = default);
    Task UnsubscribeAsync(CancellationToken cancellationToken = default);
}