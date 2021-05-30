using System;
using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "admin",
                Password = "a1234"
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var replyQueueName = channel.QueueDeclare().QueueName;
                var consumer = new EventingBasicConsumer(channel);

                var props = channel.CreateBasicProperties();
                //使用Guid創建一組獨一無二的CorrelationId
                props.CorrelationId = Guid.NewGuid().ToString();
                props.ReplyTo = replyQueueName;

                var message = GetMessage(args);
                Console.WriteLine(" 兩數相加請求 ");
                var messageBytes = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish
                    (
                        exchange: "",
                        routingKey: "rpc_queue",
                        basicProperties: props,
                        body: messageBytes
                    );

                channel.BasicConsume
                    (
                        consumer: consumer,
                        queue: replyQueueName,
                        autoAck: true
                    );

                //接收回傳訊息
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var response = Encoding.UTF8.GetString(body);
                    if (ea.BasicProperties.CorrelationId == props.CorrelationId)
                    {
                        Console.WriteLine($" 兩數相加結果：{response}");
                    }
                };
                Console.WriteLine(" 請按任意鍵離開! ");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        private static string GetMessage(string[] args)
        {
            return ((args.Length > 0) ? string.Join(" ", args) : "Hello World!");
        }
    }
}
