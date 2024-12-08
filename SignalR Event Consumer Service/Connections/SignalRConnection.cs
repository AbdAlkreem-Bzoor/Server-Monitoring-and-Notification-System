using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using SignalR_Event_Consumer_Service.Interfaces;

namespace SignalR_Event_Consumer_Service.Connections
{
    public class SignalRConnection : IConnection
    {
        private readonly HubConnection _hubConnection;
        public SignalRConnection()
        {
            var hubUrl = GetSignalRUrl();
            var connection = new HubConnectionBuilder()
                                 .WithUrl(hubUrl)
                                 .Build();
            _hubConnection = connection;
        }
        private string GetSignalRUrl()
        {
            var config = new ConfigurationBuilder()
                                     .AddJsonFile("appsettings.json")
                                     .Build();
            return config.GetSection("SignalRConfig:SignalRUrl").Value ?? string.Empty;
        }

        public async Task StartConnection()
        {
            try
            {
                await _hubConnection.StartAsync();
                Console.WriteLine("Connection started. Waiting for messages...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to hub: {ex.Message}");
            }
        }

        public async Task StopConnection()
        {
            await _hubConnection.StopAsync();
        }

        public void SubscribeToEvents()
        {
            _hubConnection.On<string>("ReceiveMessage", (message) =>
            {
                Console.WriteLine($"Received message: {message}");
            });
        }
    }
}
