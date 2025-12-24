using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;

namespace SignalREventConsumerService.EventsConsumers;

public class AlertHubEventConsumer : BackgroundService, IAsyncDisposable
{
    private readonly HubConnection _hubConnection;

    public AlertHubEventConsumer(HubConnection hubConnection)
    {
        _hubConnection = hubConnection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _hubConnection.On<string, string, DateTime>("Alert",
            (serverIdentifier, message, time) =>
            {
                Console.WriteLine($"An Alert with message: [{message}] occured on Server [{serverIdentifier}] at time (UTC): {time}");
            });

        await _hubConnection.StartAsync(stoppingToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _hubConnection.StopAsync();
        await _hubConnection.DisposeAsync();
    }
}
