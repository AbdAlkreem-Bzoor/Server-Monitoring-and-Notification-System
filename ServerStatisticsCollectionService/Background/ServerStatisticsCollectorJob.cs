using Quartz;
using ServerStatisticsCollectionService.Abstractions;

namespace ServerStatisticsCollectionService.Background;

public class ServerStatisticsCollectorJob : IJob
{
    private readonly IServerStatisticsPublisher _publisher;
    private readonly ServerStatisticsProvider _provider;

    public ServerStatisticsCollectorJob(IServerStatisticsPublisher publisher,
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
