using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQClientTest;

public class RpcClient
{
    private readonly IConnection connection;
    private readonly IModel channel;
    private readonly string queueName;
    private readonly EventingBasicConsumer consumer;
    private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
    private readonly IBasicProperties props;

    public RpcClient(string queueName)
    {
        this.queueName = queueName;

        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            
        };

        connection = factory.CreateConnection();
        channel = connection.CreateModel();
        consumer = new EventingBasicConsumer(channel);

        props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        var replyQueue =channel.QueueDeclare().QueueName;
        props.ReplyTo = replyQueue;
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body;
            var response = Encoding.UTF8.GetString(body.ToArray());
            replyQueue += ea.BasicProperties.ReplyTo;
            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                respQueue.Add(response);
            }
        };
        channel.BasicConsume(
            consumer: consumer,
            queue:replyQueue ,
            autoAck: true);
    }

    public string Call(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        
        channel.BasicPublish(
            exchange: "",
            routingKey: queueName,
            basicProperties: props,
            body: messageBytes);


        return respQueue.Take();
    }

    public void Close()
    {
        connection.Close();
    }
}