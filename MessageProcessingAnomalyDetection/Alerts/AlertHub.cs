using Microsoft.AspNetCore.SignalR;

namespace MessageProcessingAnomalyDetection.Hubs
{
    public class AlertHub : Hub
    {
        public async override Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Connected", "connected");
            await base.OnConnectedAsync();
        }
        public async Task SendAlert(string message)
        {
            await Clients.All.SendAsync("ReceiveAlert", message);
        }
    }
}
