using MessageBroker.RabbitMQ.Abstractions;
using MessageProcessingAndAnomalyDetection.Abstractions;
using MessageProcessingAndAnomalyDetection.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Shared.Models;

namespace MessageProcessingAndAnomalyDetection.Handlers;

public sealed class AlertHandler : IMessageHandler<ServerStatistics>
{
    private readonly IAnomalyAlertDetectionService _anomalyAlertDetectionService;
    private readonly IHighUsageAlertDetectionService _highUsageAlertDetectionService;

    private readonly IMongoDbContext _context;
    private readonly IAlertHubEndpoint _alertHubEndpoint;

    private readonly ServerStatisticsConfiguration _serverConfiguration;

    public AlertHandler(IAnomalyAlertDetectionService anomalyAlertDetectionService,
                        IHighUsageAlertDetectionService highUsageAlertDetectionService,
                        IMongoDbContext context,
                        IAlertHubEndpoint alertHubEndpoint,
                        IOptions<ServerStatisticsConfiguration> options)
    {
        _anomalyAlertDetectionService = anomalyAlertDetectionService;
        _highUsageAlertDetectionService = highUsageAlertDetectionService;
        _context = context;
        _alertHubEndpoint = alertHubEndpoint;
        _serverConfiguration = options.Value;
    }

    public async Task HandleAsync(ServerStatistics serverStatistics, CancellationToken ct)
    {
        // MongoDB
        var previousStatistics = await _context.Statistics
                           .Find(FilterDefinition<ServerStatistics>.Empty)
                           .SortByDescending(x => x.Id)
                           .Limit(1)
                           .FirstOrDefaultAsync();

        await _context.Statistics.InsertOneAsync(serverStatistics);

        Console.WriteLine("--- MongoDB ---");

        // SignalR
        if (previousStatistics is not null && _anomalyAlertDetectionService.CheckAlert(serverStatistics, previousStatistics, out string message))
        {
            await _alertHubEndpoint.SendAlertAsync(_serverConfiguration.ServerIdentifier, message);
        }

        if (_highUsageAlertDetectionService.CheckAlert(serverStatistics, out message))
        {
            await _alertHubEndpoint.SendAlertAsync(_serverConfiguration.ServerIdentifier, message);
        }

        Console.WriteLine("--- SignalR ---");
    }
}
