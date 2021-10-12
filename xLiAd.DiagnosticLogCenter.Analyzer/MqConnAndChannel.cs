using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.Analyzer
{
    public class MqConnAndChannel : IDisposable
    {
        private readonly ConnectionFactory Factory;
        IConnection Connection { get; set; }
        public IModel Channel { get; private set; }
        private IBasicProperties props;
        public MqConnAndChannel(AnalyzerConfig rabbitSetting)
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
        }

        private void ReConnect(object sender, ShutdownEventArgs e)
        {
            if (!Channel.IsOpen)
            {
                if (!Connection.IsOpen)
                {
                    try
                    {
                        Connection.Dispose();
                    }
                    catch { }
                    Connection = Factory.CreateConnection();
                }
                try
                {
                    Channel.Dispose();
                }
                catch { }
                Channel = Connection.CreateModel();
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
