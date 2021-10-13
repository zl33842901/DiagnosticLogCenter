using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.CollectServerBoth
{
    public class RabbitMqService : IRabbitMqService
    {
        private readonly IModel channel;
        private readonly ConfigEntity config;
        private readonly IBasicProperties props;
        public RabbitMqService(IModel channel, ConfigEntity config)
        {
            this.channel = channel;
            this.config = config;
            this.props = channel.CreateBasicProperties();
            props.Persistent = true;
        }

        public void Publish(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(config.RabbitMqExchangeName, config.RabbitMqRoutingKeyName, props, body);
        }
    }

    public interface IRabbitMqService
    {
        void Publish(string message);
    }
}
