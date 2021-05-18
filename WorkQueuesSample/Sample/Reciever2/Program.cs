using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Reciever2
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

                channel.QueueDeclare
                    (
                        queue: "WorkeTestQueue",
                        durable: true, //記得Reciever queue設定要跟Publisher一致
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                //平均分配訊息設定
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" 收到訊息： {0}", message);

                    var dots = message.Split('.').Length - 1;
                    Thread.Sleep(dots * 1000);

                    Console.WriteLine("任務完成");

                    //設定執行完成回覆Ack
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };
                channel.BasicConsume
                    (
                        queue: "WorkeTestQueue",
                        autoAck: false, //取消自動回覆
                        consumer: consumer
                    );

                Console.WriteLine("請按任意鍵離開!");
                Console.ReadLine();
            }
        }
    }
}
