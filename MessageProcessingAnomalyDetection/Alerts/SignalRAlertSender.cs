using MessageProcessingAnomalyDetection.Interfaces;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;

namespace MessageProcessingAnomalyDetection.Alerts
{
    public class SignalRAlertSender : IAlertSender
    {
        private readonly HubConnection _hubConnection;
        public SignalRAlertSender()
        {
            var hubUrl = GetSignalRUrl();
            _hubConnection = new HubConnectionBuilder().WithUrl(hubUrl).Build();
        }

        private string GetSignalRUrl()
        {
            var config = new ConfigurationBuilder()
                                     .AddJsonFile("appsettings.json")
                                     .Build();
            return config.GetSection("SignalRConfig:SignalRUrl").Value ?? string.Empty;
        }

        public SignalRAlertSender(string hubUrl)
        {
            _hubConnection = new HubConnectionBuilder().WithUrl(hubUrl).Build();
        }
        ~SignalRAlertSender()
        {
            _hubConnection.DisposeAsync().Wait();
        }
        public async Task Start()
        {
            try
            {
                await _hubConnection.StartAsync();
                Console.WriteLine("SignalR connected.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR connection error: {ex.Message}");
            }
        }
        public async Task SendAlertAsync(string message)
        {
            await _hubConnection.InvokeCoreAsync("SendAlert", args: [message]);
        }
    }
}
