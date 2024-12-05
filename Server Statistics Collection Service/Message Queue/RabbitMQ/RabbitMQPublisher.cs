using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Server_Statistics_Collection_Service.Interfaces;
using Server_Statistics_Collection_Service.Statistics;
using System.Text;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Server_Statistics_Collection_Service.Message_Queue.RabbitMQ
{
    public class RabbitMQPublisher : IMessageQueuePublisher
    {
        public IServerStatistics Statistics { get; init; }
        public RabbitMQPublisher(IServerStatistics? statistics)
        {
            if (statistics is null) throw new ArgumentNullException(nameof(statistics));
            Statistics = statistics;
        }
        public RabbitMQPublisher(string statistics) : this(JsonSerializer.Deserialize<ServerStatistics>(statistics))
        {
        }
        public bool PublishMessage()
        {
            try
            {
                var config = new ConfigurationBuilder()
                                 .AddJsonFile("appsettings.json")
                                 .Build();

                var settings = new RabbitMQSettings()
                {
                    ConnectionString = config.GetSection("RabbitMQConfig:ConnectionString").Value ?? string.Empty,
                    ExchangeName = config.GetSection("RabbitMQConfig:ExchangeName").Value ?? string.Empty,
                    QueueName = config.GetSection("RabbitMQConfig:QueueName").Value ?? string.Empty,
                    RoutingKey = config.GetSection("RabbitMQConfig:RoutingKey").Value ?? string.Empty
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
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
