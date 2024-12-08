using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Rabbit_MQ_Client_Library.Interfaces;

namespace Rabbit_MQ_Client_Library.Publishers
{
    public class RabbitMQPublisher : IMessageQueuePublisher
    {
        public IServerStatistics Statistics { get; init; }
        public RabbitMQPublisher(IServerStatistics? statistics)
        {
            ArgumentNullException.ThrowIfNull(statistics);
            Statistics = statistics;
        }
        public RabbitMQPublisher(string statistics) : this(JsonSerializer.Deserialize<IServerStatistics>(statistics))
        {

        }
        public bool PublishMessage()
        {
            try
            {
                var config = new ConfigurationBuilder()
                                 .AddJsonFile("appsettings.json")
                                 .Build();

                var settings = new
                {
                    SamplingIntervalSeconds = int.Parse(config["ServerStatisticsConfig:SamplingIntervalSeconds"] ?? string.Empty),
                    ServerIdentifier = config["ServerStatisticsConfig:ServerIdentifier"] ?? string.Empty,
                    ConnectionString = config["RabbitMQConfig:ConnectionString"] ?? string.Empty,
                    ExchangeName = config["RabbitMQConfig:ExchangeName"] ?? string.Empty,
                    QueueName = config["RabbitMQConfig:QueueName"] ?? string.Empty,
                    RoutingKey = config["RabbitMQConfig:RoutingKey"] ?? string.Empty
                };

                var factory = new ConnectionFactory()
                {
                    Uri = new Uri(settings?.ConnectionString ?? string.Empty)
                };

                using var connection = factory.CreateConnection();

                using var channel = connection.CreateModel();

                channel.ExchangeDeclare(
                                        settings?.ExchangeName,
                                        ExchangeType.Direct
                                       );

                channel.QueueDeclare(
                                     settings?.QueueName,
                                     false,
                                     false,
                                     false,
                                     null
                                    );

                channel.QueueBind(
                                  settings?.QueueName,
                                  settings?.ExchangeName,
                                  settings?.RoutingKey,
                                  null
                                 );

                var message = JsonSerializer.Serialize(Statistics);
                var messageBody = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(
                                     settings?.ExchangeName,
                                     settings?.RoutingKey,
                                     null,
                                     messageBody
                                    );
                Thread.Sleep((settings?.SamplingIntervalSeconds ?? 0) * 1);
                Console.WriteLine(Statistics);
                Console.WriteLine("_______________________________________");
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool PublishMessages(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!PublishMessage()) return false;
            }
            return true;
        }
    }
}