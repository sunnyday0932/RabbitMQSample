using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Reciver1
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectionFactory factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "admin",
                Password = "a1234"
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //一樣是topic_logs Exchange
                channel.ExchangeDeclare(
                    exchange: "topic_logs",
                    ExchangeType.Topic);

                //這方法會隨機建立一個非持久化、自動刪除的、獨立的Queue
                var queueName = channel.QueueDeclare().QueueName;

                channel.QueueBind(queue: queueName,
                                  exchange: "topic_logs", //指定透過我們的logsExchange
                                  routingKey: "*.safe");     //只拿取*.safe的logs

                Console.WriteLine(" 等待拿取log ");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" 收到訊息： {0}", message);
                };
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine("請按任意鍵離開!");
                Console.ReadLine();
            }
        }
    }
}
