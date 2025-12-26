namespace MessageBroker.RabbitMQ.Abstractions;

public interface IMessagePublisher
{
    Task DeclareExchangeAsync(CancellationToken cancellationToken = default);
    Task PublishAsync<TMeesage>(TMeesage message, CancellationToken cancellationToken = default);
}

