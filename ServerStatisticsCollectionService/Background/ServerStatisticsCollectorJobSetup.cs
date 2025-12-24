using Microsoft.Extensions.Options;
using Quartz;
using ServerStatisticsCollectionService.Models;
using Shared.Models;

namespace ServerStatisticsCollectionService.Background;

public class ServerStatisticsCollectorJobSetup : IConfigureOptions<QuartzOptions>
{
    private readonly ServerStatisticsConfiguration _configuration;

    public ServerStatisticsCollectorJobSetup(IOptions<ServerStatisticsConfiguration> options)
    {
        _configuration = options.Value;
    }

    public void Configure(QuartzOptions options)
    {
        var jobKey = JobKey.Create(nameof(ServerStatisticsCollectorJob));

        options.AddJob<ServerStatisticsCollectorJob>(configure =>
        {
            configure.WithIdentity(jobKey);
        });

        options.AddTrigger(trigger =>
        {
            trigger.ForJob(jobKey)
                   .WithSimpleSchedule(schedule =>
                   {
                       schedule.WithIntervalInSeconds(_configuration.SamplingIntervalSeconds)
                               .RepeatForever();
                   });
        });
    }
}