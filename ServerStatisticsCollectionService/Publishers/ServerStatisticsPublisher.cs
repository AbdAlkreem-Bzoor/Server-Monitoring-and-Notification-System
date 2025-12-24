using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using ServerStatisticsCollectionService.Abstractions;
using ServerStatisticsCollectionService.Models;
using Shared.Models;
using System.Text;
using System.Text.Json;

namespace ServerStatisticsCollectionService.Publishers;

public class ServerStatisticsPublisher : IServerStatisticsPublisher, IAsyncDisposable
{
    private readonly RabbitMqConfiguration _messageBrokerConfiguration;
    private readonly ServerStatisticsConfiguration _serverConfiguration;

    private IConnection? _connection;
    private IChannel? _channel;
    private string _routingKey = string.Empty;

    public ServerStatisticsPublisher(IOptions<RabbitMqConfiguration> messageBrokerConfiguration,
                                    IOptions<ServerStatisticsConfiguration> serverConfiguration)
    {
        _messageBrokerConfiguration = messageBrokerConfiguration.Value;
        _serverConfiguration = serverConfiguration.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is null)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _messageBrokerConfiguration.HostName,
                UserName = _messageBrokerConfiguration.UserName,
                Password = _messageBrokerConfiguration.Password,
                Uri = new Uri(_messageBrokerConfiguration.ConnectionString)
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
        }

        if (_channel is null)
        {
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            _routingKey = $"ServerStatistics.{_serverConfiguration.ServerIdentifier}";

            await _channel.ExchangeDeclareAsync(exchange: _messageBrokerConfiguration.ExchangeName,
                                                ExchangeType.Topic, 
                                                cancellationToken: cancellationToken);
        }
    }

    public async Task PublishAsync(ServerStatistics statistics)
    {
        await StartAsync();

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(statistics));

        var basicProperties = new BasicProperties()
        {
            Expiration = _messageBrokerConfiguration.Expiration,
            DeliveryMode = DeliveryModes.Persistent
        };

        await _channel!.BasicPublishAsync(exchange: _messageBrokerConfiguration.ExchangeName,
                                         routingKey: _routingKey,
                                         mandatory: false,
                                         basicProperties: basicProperties,
                                         body: body);

        Console.WriteLine("Pusblised");
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync(cancellationToken);
            await _channel.DisposeAsync();
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync(cancellationToken);
            await _connection.DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync() => await StopAsync();
}
