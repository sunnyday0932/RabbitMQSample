using System;
using System.Text;
using RabbitMQ.Client;

namespace Publisher
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
                //創建一個Exchange設定為logs
                channel.ExchangeDeclare(exchange: "LogsExchange", ExchangeType.Fanout);

                var message = GetMessage(args);
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "LogsExchange", //設定我們的Exchange為LogsExchange
                                     routingKey: "",           //不指定Queue
                                     basicProperties: null,
                                     body: body);

                Console.WriteLine(" 送出訊息： {0}", message);
            }

            Console.WriteLine("請按任意鍵離開!");
            Console.ReadLine();
        }

        private static string GetMessage(string[] args)
        {
            return ((args.Length > 0)
                   ? string.Join(" ", args)
                   : "info: Hello World!");
        }
    }
}
