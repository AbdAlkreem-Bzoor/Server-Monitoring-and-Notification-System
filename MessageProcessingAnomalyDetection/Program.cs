using MessageProcessingAnomalyDetection.Alerts;
using MessageProcessingAnomalyDetection.Interfaces;


IAlertSender sender = new SignalRAlertSender("http://localhost:5000/alertHub");
await sender.Start();
await sender.SendAlertAsync("!!! Success !!!");




