using MessageProcessingAnomalyDetection.Interfaces;
using MessageProcessingAnomalyDetection.Statistics;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace MessageProcessingAnomalyDetection.Receivers
{
    public class RabbitMQReceiver : IReceiver
    {
        public IServerStatistics? GetMessage()
        {
            try
            {
                var config = new ConfigurationBuilder()
                                     .AddJsonFile("appsettings.json")
                                     .Build();

                var settings = new
                {
                    SamplingIntervalSeconds = int.Parse(config.GetSection("ServerStatisticsConfig:SamplingIntervalSeconds").Value ?? string.Empty),
                    ServerIdentifier = config.GetSection("ServerStatisticsConfig:ServerIdentifier").Value ?? string.Empty,
                    ConnectionString = config.GetSection("RabbitMQConfig:ConnectionString").Value ?? string.Empty,
                    ExchangeName = config.GetSection("RabbitMQConfig:ExchangeName").Value ?? string.Empty,
                    QueueName = config.GetSection("RabbitMQConfig:QueueName").Value ?? string.Empty,
                    RoutingKey = config.GetSection("RabbitMQConfig:RoutingKey").Value ?? string.Empty
                };

                var factory = new ConnectionFactory()
                {
                    Uri = new Uri(settings.ConnectionString)
                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                var exchangeName = settings.ExchangeName;
                var queueName = settings.QueueName;
                var routingKey = settings.RoutingKey;

                channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
                channel.QueueDeclare(queueName, false, false, false, null);
                channel.QueueBind(queueName, exchangeName, routingKey, null);

                var result = channel.BasicGet(queueName, false);

                if (result is not null)
                {
                    Thread.Sleep(settings.SamplingIntervalSeconds * 1);

                    var body = result.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    var serverStats = JsonSerializer.Deserialize<ServerStatistics>(message) ?? throw new Exception();
                    serverStats.ServerIdentifier = settings.ServerIdentifier;
                    Console.WriteLine(serverStats);
                    Console.WriteLine("______________________________________________");

                    channel.BasicAck(result.DeliveryTag, false);

                    return serverStats;
                }
                else
                {
                    Console.WriteLine("No messages available in the queue.");
                    return null;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Something went wrong!");
                return null;
            }
        }

        public IEnumerable<IServerStatistics> GetMessages()
        {
            var config = new ConfigurationBuilder()
                                 .AddJsonFile("appsettings.json")
                                 .Build();

            var settings = new
            {
                SamplingIntervalSeconds = int.Parse(config.GetSection("ServerStatisticsConfig:SamplingIntervalSeconds").Value ?? string.Empty),
                ServerIdentifier = config.GetSection("ServerStatisticsConfig:ServerIdentifier").Value ?? string.Empty,
                ConnectionString = config.GetSection("RabbitMQConfig:ConnectionString").Value ?? string.Empty,
                ExchangeName = config.GetSection("RabbitMQConfig:ExchangeName").Value ?? string.Empty,
                QueueName = config.GetSection("RabbitMQConfig:QueueName").Value ?? string.Empty,
                RoutingKey = config.GetSection("RabbitMQConfig:RoutingKey").Value ?? string.Empty
            };
            var factory = new ConnectionFactory()
            {
                Uri = new Uri(settings.ConnectionString)
            };

            using var connection = factory.CreateConnection();

            using var channel = connection.CreateModel();


            var exchangeName = settings.ExchangeName;
            var queueName = settings.QueueName;
            var routingKey = settings.RoutingKey;

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            channel.QueueDeclare(queueName, false, false, false, null);
            channel.QueueBind(queueName, exchangeName, routingKey, null);

            channel.BasicQos(0, 1, false);

            ICollection<ServerStatistics> statistics = new List<ServerStatistics>();

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;

            async void Consumer_Received(object? sender, BasicDeliverEventArgs e)
            {
                await Task.Delay(settings.SamplingIntervalSeconds * 1);

                var body = e.Body.ToArray();

                var message = Encoding.UTF8.GetString(body);

                var statistic = JsonSerializer.Deserialize<ServerStatistics>(message) ?? throw new Exception();

                Console.WriteLine(statistic);
                Console.WriteLine("_____________________________________________________");

                statistics.Add(statistic);

                channel.BasicAck(e.DeliveryTag, false);
            }

            var consumerTag = channel.BasicConsume(queueName, false, consumer);

            Thread.Sleep(settings.SamplingIntervalSeconds * 1);

            channel.BasicCancel(consumerTag);

            return statistics;
        }
    }
}
