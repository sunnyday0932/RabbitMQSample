using System;
using System.Text;
using RabbitMQ.Client;

namespace Publisher
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
                //Exchange設定
                channel.ExchangeDeclare
                    (
                        "testExchange",
                        ExchangeType.Direct  //使用Direct方式
                    );

                channel.QueueDeclare
                    (
                        queue: "HelloExchange",
                        durable: false,
                        exclusive: false,
                        autoDelete: false
                    );

                //Queue 與 Exchange繫結處理
                channel.QueueBind
                    (
                        queue: "HelloExchange",
                        exchange: "testExchange",
                        routingKey: "hello",
                        null
                    );

                string message = "Hello World!";
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish
                    (
                        exchange: "testExchange",
                        routingKey: "hello",
                        basicProperties: null,
                        body: body
                    );

                Console.WriteLine(" 送出訊息： {0}", message);
            }

            Console.WriteLine("請按任意鍵離開!");
            Console.ReadLine();
        }
    }
}
