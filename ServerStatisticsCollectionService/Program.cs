using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using ServerStatisticsCollectionService.Abstractions;
using ServerStatisticsCollectionService.Background;
using ServerStatisticsCollectionService.Publishers;
using ServerStatisticsCollectionService.StatisticsProviders;
using Shared.Models;
using System.Runtime.InteropServices;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    services.AddOptions<RabbitMqConfiguration>().BindConfiguration(nameof(RabbitMqConfiguration));

    services.AddOptions<ServerStatisticsConfiguration>().BindConfiguration(nameof(ServerStatisticsConfiguration));

    services.AddSingleton<IServerStatisticsPublisher, ServerStatisticsPublisher>();

    services.AddSingleton<ServerStatisticsProvider>(sp =>
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new WindowsStatisticsProvider();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return new LinuxStatisticsProvider();
        }

        throw new NotImplementedException();
    });

    services.AddQuartz(options =>
    {

    });

    services.ConfigureOptions<ServerStatisticsCollectorJobSetup>();

    services.AddQuartzHostedService(options =>
    {
        options.WaitForJobsToComplete = true;
    });

});

await builder.Build().RunAsync();