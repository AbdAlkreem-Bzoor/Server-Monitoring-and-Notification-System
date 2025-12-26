using System.Text;
using System.Text.Json;

namespace MessageBroker.RabbitMQ.Abstractions;

public interface ISerializer
{
    byte[] Serialize<TMessage>(TMessage message);
}

internal sealed class JsonMessageSerializer : ISerializer
{
    public byte[] Serialize<TMessage>(TMessage message)
    {
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
    }
}