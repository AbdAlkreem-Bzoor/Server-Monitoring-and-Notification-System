using MessageBroker.RabbitMQ.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Quartz;
using ServerStatisticsCollectionService.Abstractions;
using ServerStatisticsCollectionService.Background;
using ServerStatisticsCollectionService.StatisticsProviders;
using Shared.Models;
using System.Runtime.InteropServices;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    services.AddOptions<RabbitMqConfiguration>().BindConfiguration(nameof(RabbitMqConfiguration));

    services.AddOptions<ServerStatisticsConfiguration>().BindConfiguration(nameof(ServerStatisticsConfiguration));

    // services.AddSingleton<IServerStatisticsPublisher, ServerStatisticsPublisher>();

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

    services.AddRabbitMqPublisher("ServerStatisticsPublisher",
       (sp, options) =>
       {
           var configuration = sp.GetRequiredService<IOptions<RabbitMqConfiguration>>().Value;
           var serverConfiguration = sp.GetRequiredService<IOptions<ServerStatisticsConfiguration>>().Value;

           options.AddConnectionOptions(connectionOptions =>
           {
               connectionOptions.HostName = configuration.HostName;
               connectionOptions.UserName = configuration.UserName;
               connectionOptions.Password = configuration.Password;
               connectionOptions.ConnectionString = configuration.ConnectionString;
           });

           options.AddExchangeOptions(exchangeOptions =>
           {
               exchangeOptions.ExchangeName = configuration.ExchangeName;
               exchangeOptions.RoutingKey = $"ServerStatistics.{serverConfiguration.ServerIdentifier}";
           });

           options.AddPublishOptions(publishOptions =>
           {
               publishOptions.Expiration = configuration.Expiration;
           });

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