using System;
using System.Linq;
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
                //創建一個Exchange設定為direct_logs 透過direct方式發送
                channel.ExchangeDeclare(
                    exchange: "direct_logs", 
                    ExchangeType.Direct);

                //我們已接收到的第一個參數來當key決定要丟給哪一個Queue
                var key = (args.Length > 0) ? args[0] : "info";

                var message = (args.Length > 1)
                              ? string.Join(" ", args.Skip(1).ToArray())
                              : "Hello World!";

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "direct_logs", //設定我們的Exchange為logs
                                     routingKey: key,   
                                     basicProperties: null,
                                     body: body);

                Console.WriteLine($" 送出訊息： '{key}' : '{message}'");
            }

            Console.WriteLine("請按任意鍵離開!");
            Console.ReadLine();
        }
    }
}
