using MessageBroker.RabbitMQ.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using ServerStatisticsCollectionService.Abstractions;

namespace ServerStatisticsCollectionService.Background;

public class ServerStatisticsCollectorJob : IJob
{
    private readonly IMessagePublisher _publisher;
    private readonly ServerStatisticsProvider _provider;

    public ServerStatisticsCollectorJob(
        [FromKeyedServices("ServerStatisticsPublisher")] IMessagePublisher publisher,
        ServerStatisticsProvider provider)
    {
        _publisher = publisher;
        _provider = provider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var statistics = _provider.GetServerStatistics();

        await _publisher.PublishAsync(statistics);
    }
}
