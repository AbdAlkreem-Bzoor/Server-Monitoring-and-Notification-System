
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shared.Models;
using SignalREventConsumerService.EventsConsumers;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{

    services.AddOptions<SignalRConfiguration>().BindConfiguration(nameof(SignalRConfiguration));

    services.AddSingleton<HubConnection>(sp =>
    {
        var settings = sp.GetRequiredService<IOptions<SignalRConfiguration>>().Value;

        var hubConnectionBuilder = new HubConnectionBuilder();

        return hubConnectionBuilder.WithUrl(settings.SignalRUrl)
                                   .WithAutomaticReconnect()
                                   .Build();
    });

    services.AddHostedService<AlertHubEventConsumer>();
});

await builder.Build().RunAsync();