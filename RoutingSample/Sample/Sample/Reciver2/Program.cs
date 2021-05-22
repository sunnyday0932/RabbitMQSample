using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Reciver2
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
                //一樣是direct_logs Exchange
                channel.ExchangeDeclare(
                    exchange: "direct_logs",
                    ExchangeType.Direct);

                //這方法會隨機建立一個非持久化、自動刪除的、獨立的Queue
                var queueName = channel.QueueDeclare().QueueName;

                channel.QueueBind(queue: queueName,
                                  exchange: "direct_logs", //指定透過我們的logsExchange
                                  routingKey: "warning");     //只拿取Warning的logs

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
