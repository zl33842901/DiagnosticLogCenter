using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.CollectServerBoth
{
    public class ConfigEntity
    {
        public string MongodbUrl { get; set; }
        public string EsUrl { get; set; }
        public string RabbitMqHost { get; set; }
        public string RabbitMqUserName { get; set; }
        public string RabbitMqPassword { get; set; }
        public string RabbitMqExchangeName => "diagnosticlog.angexchange";
        public string RabbitMqQueueName => "diagnosticlog.angqueue";
        public string RabbitMqRoutingKeyName => "diagnosticlog.angroutingkey";

        /// <summary>
        /// 是否启用行为队列的发送
        /// </summary>
        public bool EnableBehaviorMq { get; set; }
        public string BehaviorRabbitMqHost { get; set; }
        public string BehaviorRabbitMqUserName { get; set; }
        public string BehaviorRabbitMqPassword { get; set; }
        public string BehaviorRabbitMqExchangeName => "diagnosticlog.behaviorexchange";
        public string BehaviorRabbitMqQueueName => "diagnosticlog.behaviorqueue";
        public string BehaviorRabbitMqRoutingKeyName => "diagnosticlog.behaviorroutingkey";
    }
}
