using Microsoft.AspNetCore.SignalR;

namespace SignalRHub.Hubs;

public class AlertHub : Hub
{
    public async Task Alert(string serverIdentifier, string message)
    {
        await Clients.All.SendAsync("Alert", serverIdentifier, message, DateTime.UtcNow);
    }
}
