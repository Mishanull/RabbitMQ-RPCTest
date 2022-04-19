// See https://aka.ms/new-console-template for more information

using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory() { HostName = "localhost" };
using (var connection = factory.CreateConnection())
using (var channel = connection.CreateModel())
{
    channel.QueueDeclare(queue: "q.test", durable: false,
        exclusive: false, autoDelete: false, arguments: null);
    channel.BasicQos(0, 1, false);
    var consumer = new EventingBasicConsumer(channel);
    
    Console.WriteLine(" [x] Awaiting RPC requests");

    consumer.Received += (model, ea) =>
    {
        string response = null;

        var body = ea.Body.ToArray();
        var props = ea.BasicProperties;
        var replyProps = channel.CreateBasicProperties();
        replyProps.CorrelationId = props.CorrelationId;

        try
        {
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine(" [.] msg({0})", message);
            response = JsonSerializer.Serialize("Received message from client: "+message+". Hello there!");
        }
        catch (Exception e)
        {
            Console.WriteLine(" [.] " + e.Message);
            response = "";
        }
        finally
        {
            var responseBytes = Encoding.UTF8.GetBytes(response);
            channel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                basicProperties: replyProps, body: responseBytes);
            channel.BasicAck(deliveryTag: ea.DeliveryTag,
                multiple: false);
        }
    };
    channel.BasicConsume(queue: "q.test",
        autoAck: false, consumer: consumer);
    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
    
}

