namespace Shared.Models;

public class RabbitMqConfiguration
{
    public required string HostName { get; init; }
    public required string UserName { get; init; }
    public required string Password { get; init; }
    public required string ConnectionString { get; init; }
    public required string ExchangeName { get; init; }
    public required string QueueName { get; init; }
    public required string Expiration { get; init; }
}
