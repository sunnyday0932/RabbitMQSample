using System;
using System.Linq;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Server
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
                //設定RPC Queue
                channel.QueueDeclare
                    (
                        queue: "rpc_queue",
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );
                //訊息分配設定
                channel.BasicQos
                    (
                        prefetchSize: 0,
                        prefetchCount: 1,
                        global: false
                    );

                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume
                    (
                        queue: "rpc_queue",
                        autoAck: false,
                        consumer: consumer
                    );

                Console.WriteLine(" 等待RPC請求中 ");

                consumer.Received += (model, ea) =>
                {
                    string response = null;

                    var body = ea.Body.ToArray();
                    var props = ea.BasicProperties;
                    var replyProps = channel.CreateBasicProperties();
                    //藉由CorrelationId來匹配請求
                    replyProps.CorrelationId = props.CorrelationId;

                    try
                    {
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine($"兩數相加：{message}");
                        response = AddNumber(message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("請確認錯誤訊息：" + ex.Message);
                        response = string.Empty;
                    }
                    finally
                    {
                        var responseBytes = Encoding.UTF8.GetBytes(response);

                        channel.BasicPublish
                        (
                            exchange: "",
                            routingKey: props.ReplyTo,
                            basicProperties: replyProps,
                            body: responseBytes
                         );

                        channel.BasicAck
                        (
                            deliveryTag: ea.DeliveryTag,
                            multiple: false
                        );

                        Console.WriteLine("回傳訊息成功!");
                    }
                };

                Console.WriteLine(" 請按任意鍵離開! ");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Adds the number.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private static string AddNumber(string message)
        {
            var strArry = message.Split(',');
            if (string.IsNullOrWhiteSpace(strArry.FirstOrDefault()) &&
                string.IsNullOrWhiteSpace(strArry.LastOrDefault()))
            {
                throw new Exception("請確認傳入參數");
            }

            if (int.TryParse(strArry[0], out int firstNumber).Equals(false))
            {
                throw new Exception("第一個數字轉換出錯");
            }

            if (int.TryParse(strArry[1], out int secondNumber).Equals(false))
            {
                throw new Exception("第二個數字轉換出錯");
            }

            var result = firstNumber + secondNumber;

            return result.ToString();
        }
    }
}
