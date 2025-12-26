namespace MessageBroker.RabbitMQ.Abstractions;

public interface IMessageHandler<TMessage> where TMessage : class
{
    Task HandleAsync(TMessage message, CancellationToken ct);
}

