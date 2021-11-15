using RabbitMQ.Client;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.CollectServerBoth
{
    public class RabbitMqBehaviorService : IRabbitMqBehaviorService
    {
        private readonly IModel channel;
        private readonly ConfigEntity config;
        private readonly IBasicProperties props;
        public RabbitMqBehaviorService(ConfigEntity config)
        {
            this.config = config;
            var factory = new ConnectionFactory();
            factory.HostName = config.BehaviorRabbitMqHost;
            factory.UserName = config.BehaviorRabbitMqUserName;
            factory.Password = config.BehaviorRabbitMqPassword;
            factory.AutomaticRecoveryEnabled = true;
            var conn = factory.CreateConnection();
            var channel = conn.CreateModel();
            channel.ExchangeDeclare(config.BehaviorRabbitMqExchangeName, "direct", durable: true, autoDelete: false, arguments: null);
            channel.QueueDeclare(config.BehaviorRabbitMqQueueName, durable: true, autoDelete: false, exclusive: false, arguments: null);
            channel.QueueBind(config.BehaviorRabbitMqQueueName, config.BehaviorRabbitMqExchangeName, routingKey: config.BehaviorRabbitMqRoutingKeyName);
            this.channel = channel;
            this.props = channel.CreateBasicProperties();
            props.Persistent = true;
        }
        public void Publish(string message)
        {
            if (!config.EnableBehaviorMq)
                return;
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(config.BehaviorRabbitMqExchangeName, config.BehaviorRabbitMqRoutingKeyName, props, body);
        }
    }

    public interface IRabbitMqBehaviorService
    {
        void Publish(string message);
    }
}
