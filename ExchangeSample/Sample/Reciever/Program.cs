using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Reciever
{
    class Program
    {
        static void Main(string[] args)
        {
            //設定主機位置以及帳號密碼
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "admin",
                Password = "a1234"
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare
                    (
                        queue: "HelloExchange",
                        durable: false,
                        exclusive: false,
                        autoDelete: false
                    );

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var message = ea.Body.ToArray();
                    Console.WriteLine(" 收到訊息： {0}", Encoding.UTF8.GetString(message));
                };
                channel.BasicConsume(queue: "HelloExchange",
                                         autoAck: true,
                                         consumer: consumer);
                //建立Consumer接收
                channel.BasicConsume
                    (
                        queue: "HelloExchange",    //指定Queue的名稱。
                        autoAck: true,     //True的話會自動發送有無接收到訊息給RabbitMQ做確認。
                        consumer: consumer //Consumer內容
                    );

                Console.WriteLine("請按任意鍵離開!");
                Console.ReadLine();
            }
        }
    }
}
