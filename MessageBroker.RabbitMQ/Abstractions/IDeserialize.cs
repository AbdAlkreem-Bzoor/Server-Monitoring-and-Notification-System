using System.Text.Json;

namespace MessageBroker.RabbitMQ.Abstractions;

public interface IDeserialize
{
    TMessage Deserialize<TMessage>(byte[] message);
}

public sealed class JsonMessageDeserialize : IDeserialize
{
    public TMessage Deserialize<TMessage>(byte[] message)
    {
        return JsonSerializer.Deserialize<TMessage>(message) ?? throw new Exception();
    }
}