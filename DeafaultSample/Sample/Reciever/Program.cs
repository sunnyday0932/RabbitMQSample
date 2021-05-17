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

                //Queue的設定
                channel.QueueDeclare
                    (
                        queue: "hello",    //Queue的名稱。
                        durable: false,    //Queue是否持久化保存，是的話會寫入硬碟即便RabbitMQ重啟也不會遺失。
                        exclusive: false,  //Queue是否為私有訪問機制，是的話當有一個Reciever在訪問該Queue就會對其Lock。
                        autoDelete: false  //Queue是否自動刪除，是的話當最後一個Reciever段開連結後會自動刪除Queue。
                    );

                //Comsumer主要處理訊息內容
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var message = ea.Body.ToArray();
                    Console.WriteLine(" 收到訊息： {0}", Encoding.UTF8.GetString(message));
                };

                //建立Consumer接收
                channel.BasicConsume
                    (
                        queue: "hello",    //指定Queue的名稱。
                        autoAck: true,     //True的話會自動發送有無接收到訊息給RabbitMQ做確認。
                        consumer: consumer //Consumer內容
                    );

                Console.WriteLine("請按任意鍵離開!");
                Console.ReadLine();
            }
        }
    }
}
