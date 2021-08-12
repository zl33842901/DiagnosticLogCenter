using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.Agent.Logger
{
    public class DiagnosticLogCenterLoggerProvider : ILoggerProvider
    {
        private readonly LogLevel LogLevel;
        public DiagnosticLogCenterLoggerProvider(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            var logConfig = configuration?.GetValue<string>("Logging:LogLevel:Default");
            if (!string.IsNullOrEmpty(logConfig))
            {
                bool suc = Enum.TryParse(logConfig, out LogLevel);
                if (!suc)
                    LogLevel = LogLevel.Warning;
            }
            else
                LogLevel = LogLevel.Warning;
        }
        public ILogger CreateLogger(string categoryName)
        {
            return new DiagnosticLogCenterLogger(LogLevel);
        }

        public void Dispose()
        {

        }
    }
}
