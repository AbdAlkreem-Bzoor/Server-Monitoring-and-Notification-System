using MessageProcessingAndAnomalyDetection.Abstractions;
using Microsoft.AspNetCore.SignalR.Client;

namespace MessageProcessingAndAnomalyDetection.Connections;

public class AlertHubConnection : IAlertHubEndpoint, IAsyncDisposable
{
    private readonly HubConnection _hubConnection;

    public AlertHubConnection(HubConnection hubConnection)
    {
        _hubConnection = hubConnection;
    }

    private async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
        {
            await _hubConnection.StartAsync(cancellationToken);
        }
    }

    public async Task SendAlertAsync(string serverIdentifier, string message)
    {
        await StartAsync();

        await _hubConnection!.InvokeAsync("Alert", serverIdentifier, message);
    }

    private async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.StopAsync(cancellationToken);
            await _hubConnection.DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}
