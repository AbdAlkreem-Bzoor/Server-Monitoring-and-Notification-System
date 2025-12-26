using MessageBroker.RabbitMQ.Abstractions;
using MessageBroker.RabbitMQ.Options;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace MessageBroker.RabbitMQ.Publishers;

internal sealed class RabbitMqPublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly string _publisherKey;

    private readonly SemaphoreSlim _channelSemaphore = new(1, 1);

    private readonly ISerializer _serializer;
    private readonly IConnectionFactory _factory;
    private IConnection? _connection;
    private IChannel? _channel;

    private readonly RabbitMqOptions _options;

    private readonly ResiliencePipeline _pipeline;

    private bool _exchangeDeclared = false;

    public RabbitMqPublisher(IConnectionFactory factory,
                             ISerializer serializer,
                             ResiliencePipeline pipeline,
                             RabbitMqOptions options,
                             string publisherName)
    {
        _factory = factory;
        _serializer = serializer;
        _pipeline = pipeline;
        _options = options;

        _publisherKey = publisherName;
    }

    public string PublisherName => _publisherKey;

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
        if (_exchangeDeclared)
        {
            return;
        }

        var channel = await GetChannelAsync(cancellationToken);

        var exchangeOptions = _options.Exchange;

        await channel.ExchangeDeclareAsync(exchangeOptions.ExchangeName,
                                           exchangeOptions.Type,
                                           exchangeOptions.Durable,
                                           exchangeOptions.AutoDelete,
                                           exchangeOptions.Arguments,
                                           cancellationToken: cancellationToken);

        _exchangeDeclared = true;
    }

    public async Task PublishAsync<TMeesage>(TMeesage message,
                                             CancellationToken cancellationToken = default)
    {
        await DeclareExchangeAsync(cancellationToken);

        var body = _serializer.Serialize(message);

        var publishOptions = _options.Publish;

        var basicProperties = new BasicProperties()
        {
            Expiration = publishOptions.Expiration,
            DeliveryMode = publishOptions.DeliveryMode,
            Priority = publishOptions.Priority,

            MessageId = publishOptions.MessageId,
            Timestamp = new AmqpTimestamp(publishOptions.Timestamp.ToUnixTimeSeconds()),

            CorrelationId = publishOptions.CorrelationId,
            ReplyTo = publishOptions.ReplyTo,

            Type = publishOptions.Type,
            AppId = publishOptions.AppId,
            Headers = publishOptions.Headers,
        };

        await _pipeline.ExecuteAsync(async ct =>
        {
            var channel = await GetChannelAsync(ct);

            await _channelSemaphore.WaitAsync(ct);
            try
            {
                var exchangeOptions = _options.Exchange;

                await channel.BasicPublishAsync
                                          (
                                            exchangeOptions.ExchangeName,
                                            exchangeOptions.RoutingKey,
                                            publishOptions.Mandatory,
                                            basicProperties,
                                            body,
                                            cancellationToken: ct
                                          );

            }
            catch (PublishException)
            {
                // TODO: handle logging and exceptions
                throw;
            }
            finally
            {
                _channelSemaphore.Release();
            }
        }, cancellationToken);


    }

    public async ValueTask DisposeAsync()
    {
        await DisposeChannelAsync(default);

        await DisposeConnectionAsync(default);

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
