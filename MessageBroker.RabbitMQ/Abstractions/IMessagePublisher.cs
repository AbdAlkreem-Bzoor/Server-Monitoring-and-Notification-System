namespace MessageBroker.RabbitMQ.Abstractions;

public interface IMessagePublisher
{
    Task PublishAsync<TMeesage>(TMeesage message, CancellationToken cancellationToken = default);
}

