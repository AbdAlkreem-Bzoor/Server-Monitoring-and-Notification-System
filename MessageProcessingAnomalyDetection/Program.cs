using MessageProcessingAnomalyDetection.Alerts;
using MessageProcessingAnomalyDetection.Interfaces;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;


IAlertSender sender = new SignalRAlertSender("http://localhost:5000/alertHub");
await sender.Start();
await sender.SendAlertAsync("!!! Success !!!");




