using MessageBroker.RabbitMQ.Abstractions;
using Microsoft.Extensions.Hosting;

namespace MessageBroker.RabbitMQ.Consumers;

internal class ConsumerHostedService : IHostedService
{
    private readonly IMessageConsumer _consumer;

    public ConsumerHostedService(IMessageConsumer consumer)
    {
        _consumer = consumer;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _consumer.SubscribeAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _consumer.UnsubscribeAsync(cancellationToken);
    }
}