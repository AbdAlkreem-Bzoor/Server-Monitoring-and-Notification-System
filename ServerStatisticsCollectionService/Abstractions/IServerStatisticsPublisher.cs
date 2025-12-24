using ServerStatisticsCollectionService.Models;

namespace ServerStatisticsCollectionService.Abstractions;

public interface IServerStatisticsPublisher
{
    Task PublishAsync(ServerStatistics statistics);
}
