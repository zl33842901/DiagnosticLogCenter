using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.Analyzer
{
    public class AnalyzerConfig
    {
        public string RabbitMqHost { get; set; }

        public string RabbitMqUserName { get; set; }

        public string RabbitMqPassword { get; set; }
        public string RabbitMqExchangeName => "diagnosticlog.angexchange";
        public string RabbitMqQueueName => "diagnosticlog.angqueue";
        public string RabbitMqRoutingKeyName => "diagnosticlog.angroutingkey";
    }
}
