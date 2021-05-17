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

            //連線設定
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //Queue的設定
                channel.QueueDeclare
                    (
                        queue: "hello",    //Queue的名稱
                        durable: false,    //Queue是否持久化保存，是的話會寫入硬碟即便RabbitMQ重啟也不會遺失。
                        exclusive: false,  //Queue是否為私有訪問機制，是的話當有一個Reciever在訪問該Queue就會對其Lock。
                        autoDelete: false  //Queue是否自動刪除，是的話當最後一個Reciever段開連結後會自動刪除Queue。
                    );

                //設定我們要傳送的訊息
                string message = "Hello World!";
                var body = Encoding.UTF8.GetBytes(message);

                //設定Publisher
                channel.BasicPublish
                    (
                        exchange: "",
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
