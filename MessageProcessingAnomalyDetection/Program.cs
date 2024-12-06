using System;
using System.Text;
using System.Text.Json;
using MessageProcessingAnomalyDetection.Interfaces;
using MessageProcessingAnomalyDetection.Receivers;
using MessageProcessingAnomalyDetection.Statistics;
using RabbitMQ.Client;

IReceiver receiver = new RabbitMQReceiver();

for (int i = 0; i < 15; i++) receiver.GetMessage();


//var factory = new ConnectionFactory()
//{
//    Uri = new Uri("amqp://guest:guest@localhost:5672")
//};

//using var connection = factory.CreateConnection();

//using var channel = connection.CreateModel();


//var exchangeName = "Task1 Exchange: Server Statistics Collection Service";
//var queueName = "Task1 Queue";
//var routingKey = "Task1 Routing Key";

//channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
//channel.QueueDeclare(queueName, false, false, false, null);
//channel.QueueBind(queueName, exchangeName, routingKey, null);

//channel.BasicQos(0, 1, false);

//var consumer = new EventingBasicConsumer(channel);
//consumer.Received += Consumer_Received;

//async void Consumer_Received(object? sender, BasicDeliverEventArgs e)
//{
//    await Task.Delay(5000);

//    var body = e.Body.ToArray();

//    var message = Encoding.UTF8.GetString(body);

//    Console.WriteLine(JsonSerializer.Deserialize<ServerStatistics>(message));
//    Console.WriteLine("_____________________________________________________");

//    channel.BasicAck(e.DeliveryTag, false);
//}

//var consumerTag = channel.BasicConsume(queueName, false, consumer);

//Console.ReadLine();

//channel.BasicCancel(consumerTag);
