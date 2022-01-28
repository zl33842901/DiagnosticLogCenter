using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.Analyzer
{
    public class MqConnAndChannel : IDisposable
    {
        private readonly AnalyzerConfig rabbitSetting;
        private readonly Action<object, BasicDeliverEventArgs> consumeAction;
        private ConnectionFactory Factory;
        private IConnection Connection;
        public IModel Channel { get; private set; }
        private IBasicProperties props;
        public MqConnAndChannel(AnalyzerConfig rabbitSetting, Action<object, BasicDeliverEventArgs> consumeAction)
        {
            this.rabbitSetting = rabbitSetting;
            this.consumeAction = consumeAction;
            LoadRbt();
        }

        private void LoadRbt()
        {
            Factory = new ConnectionFactory();
            Factory.HostName = rabbitSetting.RabbitMqHost;
            Factory.UserName = rabbitSetting.RabbitMqUserName;
            Factory.Password = rabbitSetting.RabbitMqPassword;
            Factory.AutomaticRecoveryEnabled = true;
            Connection = Factory.CreateConnection();
            Connection.ConnectionShutdown += ReConnect;
            Channel = Connection.CreateModel();
            Channel.ExchangeDeclare(rabbitSetting.RabbitMqExchangeName, "direct", durable: true, autoDelete: false, arguments: null);

            Channel.QueueDeclare(rabbitSetting.RabbitMqQueueName, durable: true, autoDelete: false, exclusive: false, arguments: null);
            Channel.QueueBind(rabbitSetting.RabbitMqQueueName, rabbitSetting.RabbitMqExchangeName, routingKey: rabbitSetting.RabbitMqRoutingKeyName);

            props = Channel.CreateBasicProperties();
            props.Persistent = true;
            var consumer = new EventingBasicConsumer(Channel);
            consumer.Received += new EventHandler<BasicDeliverEventArgs>(consumeAction);
            Channel.BasicConsume(rabbitSetting.RabbitMqQueueName, false, consumerTag: "我是日志实时分析系统", noLocal: false, exclusive: false, arguments: null, consumer);

        }

        private void ReConnect(object sender, ShutdownEventArgs e)
        {
            while (!Connection.IsOpen)
            {
                try
                {
                    Channel.Dispose();
                    Connection.Dispose();
                    LoadRbt();
                }
                catch(Exception ex) { Console.WriteLine("重新连接rabbitmq时发生错误：" + ex.Message); System.Threading.Thread.Sleep(3000); }
            }
        }

        public void BasicAck(ulong deliveryTag, bool multiple = false) => Channel.BasicAck(deliveryTag, multiple);

        public void BasicReject(ulong deliveryTag, bool requeue = true) => Channel.BasicReject(deliveryTag, requeue);

        public void Dispose()
        {
            try
            {
                Channel.Dispose();
            }
            catch { }
            try
            {
                Connection.Dispose();
            }
            catch { }
        }
    }
}
