using MessageProcessingAndAnomalyDetection.Abstractions;
using MessageProcessingAndAnomalyDetection.Connections;
using MessageProcessingAndAnomalyDetection.Consumers;
using MessageProcessingAndAnomalyDetection.Data;
using MessageProcessingAndAnomalyDetection.Models;
using MessageProcessingAndAnomalyDetection.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Shared.Models;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    services.AddOptions<ServerStatisticsConfiguration>().BindConfiguration(nameof(ServerStatisticsConfiguration));

    services.AddOptions<RabbitMqConfiguration>().BindConfiguration(nameof(RabbitMqConfiguration));

    services.AddOptions<MongoDbConfiguration>().BindConfiguration(nameof(MongoDbConfiguration));

    services.AddOptions<SignalRConfiguration>().BindConfiguration(nameof(SignalRConfiguration));

    services.AddOptions<AlertsDetectionConfiguration>().BindConfiguration(nameof(AlertsDetectionConfiguration));

    services.AddSingleton<IMongoClient>(sp =>
    {
        var settings = sp.GetRequiredService<IOptions<MongoDbConfiguration>>().Value;

        return new MongoClient(settings.ConnectionString);
    });

    services.AddScoped<IMongoDatabase>(sp =>
    {
        var settings = sp.GetRequiredService<IOptions<MongoDbConfiguration>>().Value;

        var client = sp.GetRequiredService<IMongoClient>();

        return client.GetDatabase(settings.DatabaseName);
    });

    services.AddScoped<IMongoDbContext, MongoDbContext>();

    services.AddSingleton<IAnomalyAlertDetectionService, AnomalyAlertDetectionService>();
    services.AddSingleton<IHighUsageAlertDetectionService, HighUsageAlertDetectionService>();

    services.AddSingleton<HubConnection>(sp =>
    {
        var settings = sp.GetRequiredService<IOptions<SignalRConfiguration>>().Value;

        var hubConnectionBuilder = new HubConnectionBuilder();

        return hubConnectionBuilder.WithUrl(settings.SignalRUrl)
                                   .WithAutomaticReconnect()
                                   .Build();
    });

    services.AddSingleton<IAlertHubEndpoint, AlertHubConnection>();

    services.AddHostedService<ServerStatisticsConsumer>();
});


await builder.Build().RunAsync();
