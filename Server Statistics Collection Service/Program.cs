using Server_Statistics_Collection_Service.Extentions;
using Server_Statistics_Collection_Service.Interfaces;
using Server_Statistics_Collection_Service.Message_Queue.RabbitMQ;

IServerStatisticsFactory factory = new ServerStatisticsFactory();

while (true)
{
    IServerStatistics statistics = factory.GetStatistics();

    IMessageQueuePublisher publisher = new RabbitMQPublisher(statistics);

    if (publisher.PublishMessage())
    {
        Console.WriteLine(statistics.ToString());
    }
    else
    {
        Console.WriteLine("Something went wrong with the statistics!");
        break;
    }

    await Task.Delay(20000);
}