using MessageBroker.RabbitMQ.Abstractions;
using MessageBroker.RabbitMQ.Options;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageBroker.RabbitMQ.Consumers;

public class RabbitMqConsumer<TMessage, TConsumer> : IMessageConsumer, IAsyncDisposable
    where TMessage : class
    where TConsumer : IMessageHandler<TMessage>
{
    private readonly string _consumerName;

    private readonly SemaphoreSlim _channelSemaphore = new(1, 1);

    private readonly IDeserialize _deserialize;
    private readonly IConnectionFactory _factory;
    private IConnection? _connection;
    private IChannel? _channel;

    private readonly IServiceProvider _serviceProvider;

    private readonly RabbitMqOptions _options;

    private readonly ResiliencePipeline _pipeline;

    private bool _isListening = false;
    private string? _consumerTag;

    public RabbitMqConsumer(IConnectionFactory factory,
                            IDeserialize deserialize,
                            IServiceProvider serviceProvider,
                            ResiliencePipeline pipeline,
                            RabbitMqOptions options,
                            string consumerName)
    {
        _factory = factory;
        _deserialize = deserialize;
        _serviceProvider = serviceProvider;
        _options = options;
        _pipeline = pipeline;

        _consumerName = consumerName;
    }

    public string ConsumerName => _consumerName;

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken = default)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        await _channelSemaphore.WaitAsync(cancellationToken);

        try
        {
            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            await DisposeChannelAsync(cancellationToken);

            _connection ??= await _factory.CreateConnectionAsync(cancellationToken);

            if (!_connection.IsOpen)
            {
                throw new InvalidOperationException("RabbitMQ connection is closed. Cannot create channel.");
            }

            var channelOptions = _options.Channel;

            var options = new CreateChannelOptions
                          (
                              channelOptions.PublisherConfirmationsEnabled,
                              channelOptions.PublisherConfirmationTrackingEnabled
                          );

            _channel = await _connection.CreateChannelAsync(options, cancellationToken: cancellationToken);

            return _channel;
        }
        finally
        {
            _channelSemaphore.Release();
        }
    }

    private async Task DeclareExchangeAsync(CancellationToken cancellationToken = default)
    {
        var channel = await GetChannelAsync(cancellationToken);

        var exchangeOptions = _options.Exchange;

        await channel.ExchangeDeclareAsync(exchangeOptions.ExchangeName,
                                           exchangeOptions.Type,
                                           exchangeOptions.Durable,
                                           exchangeOptions.AutoDelete,
                                           exchangeOptions.Arguments,
                                           cancellationToken: cancellationToken);
    }

    private async Task ConfigureQosAsync(CancellationToken cancellationToken = default)
    {
        var channel = await GetChannelAsync(cancellationToken);

        var queueOptions = _options.Queue;

        await channel.BasicQosAsync(queueOptions.Qos.PrefetchSize,
                                    queueOptions.Qos.PrefetchCount,
                                    queueOptions.Qos.Global,
                                    cancellationToken: cancellationToken);
    }

    private async Task DeclareQueueAsync(CancellationToken cancellationToken = default)
    {
        var channel = await GetChannelAsync(cancellationToken);

        var queueOptions = _options.Queue;

        await channel.QueueDeclareAsync(queueOptions.QueueName,
                                        queueOptions.Durable,
                                        queueOptions.Exclusive,
                                        queueOptions.AutoDelete,
                                        queueOptions.Arguments,
                                        cancellationToken: cancellationToken);
    }

    private async Task BindQueueAsync(CancellationToken cancellationToken = default)
    {
        var channel = await GetChannelAsync(cancellationToken);

        var queueOptions = _options.Queue;
        var exchangeOptions = _options.Exchange;

        await channel.QueueBindAsync(queueOptions.QueueName,
                                     exchangeOptions.ExchangeName,
                                     queueOptions.RoutingKey,
                                     cancellationToken: cancellationToken);
    }

    public async Task SubscribeAsync(CancellationToken cancellationToken = default)
    {
        if (_isListening)
        {
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<TConsumer>();

        await _pipeline.ExecuteAsync(async ct =>
        {
            await DeclareExchangeAsync(cancellationToken);

            await ConfigureQosAsync(cancellationToken);

            await DeclareQueueAsync(cancellationToken);

            await BindQueueAsync(cancellationToken);

        }, cancellationToken);

        using var channel = await GetChannelAsync(cancellationToken);

        var queueOptions = _options.Queue;

        var consumer = new AsyncEventingBasicConsumer(channel);

        var handlerKey = $"{_consumerName}-Handler";

        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            using var messageScope = _serviceProvider.CreateScope();
            var handler = messageScope.ServiceProvider.GetRequiredKeyedService<TConsumer>(handlerKey);

            try
            {
                var body = eventArgs.Body.ToArray();
                var message = _deserialize.Deserialize<TMessage>(body);

                await handler.HandleAsync(message, cancellationToken);

                await channel.BasicAckAsync(eventArgs.DeliveryTag,
                                            queueOptions.Ack.Multiple,
                                            cancellationToken: cancellationToken);
            }
            catch (Exception)
            {
                await channel.BasicNackAsync(eventArgs.DeliveryTag,
                                             queueOptions.Nack.Multiple,
                                             queueOptions.Nack.Requeue,
                                             cancellationToken: cancellationToken);

                // TODO: handle logging and exceptions
            }
        };

        var consumeOptions = _options.Consume;

        _consumerTag = await channel.BasicConsumeAsync(queueOptions.QueueName,
                                        consumeOptions.AutoAck,
                                        consumer,
                                        cancellationToken: cancellationToken);

        _isListening = true;
    }

    public async Task UnsubscribeAsync(CancellationToken cancellationToken = default)
    {
        if (!_isListening || _consumerTag is null)
        {
            return;
        }

        using var channel = await GetChannelAsync(cancellationToken);

        await channel.BasicCancelAsync(_consumerTag, cancellationToken: cancellationToken);

        await DisposeAsync();

        _isListening = false;
        _consumerTag = null;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeChannelAsync();

        await DisposeConnectionAsync();

        _channelSemaphore.Dispose();
    }

    private async Task DisposeConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is not null)
        {
            await _connection.CloseAsync(cancellationToken);
            await _connection.DisposeAsync();
        }
    }

    private async Task DisposeChannelAsync(CancellationToken cancellationToken = default)
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync(cancellationToken);
            await _channel.DisposeAsync();
        }
    }
}
