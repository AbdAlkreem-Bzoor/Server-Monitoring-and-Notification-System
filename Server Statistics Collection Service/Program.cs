using Server_Statistics_Collection_Service.Extentions;
using Server_Statistics_Collection_Service.Interfaces;
using Server_Statistics_Collection_Service.Message_Queue.RabbitMQ;

IServerStatisticsFactory factory = new ServerStatisticsFactory();


IServerStatistics statistics = factory.GetStatistics();

IMessageQueuePublisher publisher = new RabbitMQPublisher(statistics);


if (!publisher.PublishMessages(20))
{
    Console.WriteLine("Something went wrong with the statistics!");
}

