using MessageProcessingAndAnomalyDetection.Abstractions;
using MessageProcessingAndAnomalyDetection.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Models;
using System.Text;
using System.Text.Json;

namespace MessageProcessingAndAnomalyDetection.Consumers;

public class ServerStatisticsConsumer : BackgroundService
{
    private readonly IMongoDbContext _context;
    private readonly IAlertHubEndpoint _alertHubEndpoint;
    private readonly RabbitMqConfiguration _messageBrokerConfiguration;
    private readonly ServerStatisticsConfiguration _serverConfiguration;

    private readonly IAnomalyAlertDetectionService _anomalyAlertDetectionService;
    private readonly IHighUsageAlertDetectionService _highUsageAlertDetectionService;

    private IConnection? _connection;
    private IChannel? _channel;
    private string _routingKey = string.Empty;

    public ServerStatisticsConsumer(IMongoDbContext context,
                                    IAlertHubEndpoint alertHubEndpoint,
                                    IAnomalyAlertDetectionService anomalyAlertDetectionService,
                                    IHighUsageAlertDetectionService highUsageAlertDetectionService,
                                    IOptions<RabbitMqConfiguration> messageBrokerConfiguration,
                                    IOptions<ServerStatisticsConfiguration> serverConfiguration)
    {
        _context = context;
        _alertHubEndpoint = alertHubEndpoint;
        _anomalyAlertDetectionService = anomalyAlertDetectionService;
        _highUsageAlertDetectionService = highUsageAlertDetectionService;
        _messageBrokerConfiguration = messageBrokerConfiguration.Value;
        _serverConfiguration = serverConfiguration.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory()
        {
            HostName = _messageBrokerConfiguration.HostName,
            UserName = _messageBrokerConfiguration.UserName,
            Password = _messageBrokerConfiguration.Password,
            Uri = new Uri(_messageBrokerConfiguration.ConnectionString)
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        _routingKey = $"ServerStatistics.*";

        await _channel.ExchangeDeclareAsync(exchange: _messageBrokerConfiguration.ExchangeName,
                                            ExchangeType.Topic,
                                            cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(queue: _messageBrokerConfiguration.QueueName,
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(queue: _messageBrokerConfiguration.QueueName,
                                      exchange: _messageBrokerConfiguration.ExchangeName,
                                      routingKey: _routingKey,
                                      cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            try
            {
                var body = eventArgs.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var currentStatistics = JsonSerializer.Deserialize<ServerStatistics>(json)!;
                currentStatistics.ServerIdentifier = _serverConfiguration.ServerIdentifier;

                // MongoDB
                var previousStatistics = await _context.Statistics
                                   .Find(FilterDefinition<ServerStatistics>.Empty)
                                   .SortByDescending(x => x.Id)
                                   .Limit(1)
                                   .FirstOrDefaultAsync();

                await _context.Statistics.InsertOneAsync(currentStatistics);

                // SignalR
                if (previousStatistics is not null && _anomalyAlertDetectionService.CheckAlert(currentStatistics, previousStatistics, out string message))
                {
                    await _alertHubEndpoint.SendAlertAsync(_serverConfiguration.ServerIdentifier, message);
                }

                if (_highUsageAlertDetectionService.CheckAlert(currentStatistics, out message))
                {
                    await _alertHubEndpoint.SendAlertAsync(_serverConfiguration.ServerIdentifier, message);
                }

                await _channel.BasicAckAsync(deliveryTag: eventArgs.DeliveryTag,
                                             multiple: false,
                                             cancellationToken: stoppingToken);

                Console.WriteLine("Consumed");
            }
            catch (Exception exception)
            {
                Console.WriteLine("❌ Consumer failed:");
                Console.WriteLine(exception);

                await _channel.BasicRejectAsync(deliveryTag: eventArgs.DeliveryTag,
                                                requeue: false,
                                                cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(_messageBrokerConfiguration.QueueName,
                                   autoAck: false,
                                   consumer,
                                   cancellationToken: stoppingToken);

        await Task.CompletedTask;
    }
}
