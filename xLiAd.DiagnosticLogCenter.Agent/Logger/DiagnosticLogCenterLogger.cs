using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using xLiAd.DiagnosticLogCenter.Abstract;

namespace xLiAd.DiagnosticLogCenter.Agent.Logger
{
    public class DiagnosticLogCenterLogger : ILogger
    {
        public DiagnosticLogCenterLogger(LogLevel LogLevel)
        {
            this.LogLevel = LogLevel;
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public virtual bool IsEnabled(LogLevel logLevel)
        {
            return LogLevel <= logLevel;
        }

        public LogLevel LogLevel { get; private set; } = LogLevel.Error;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            LogEntity log = new LogEntity()
            {
                LogType = LogTypeEnum.MethodAddition,
                ClassName = "系统日志",
                MethodName = string.Empty,
                StackTrace = null,
                GroupGuid = GuidHolder.Holder.Value.ToString(),
                HappenTime = DateTime.Now,
                PageId = GuidHolder.PageIdHolder.Value,
                TraceId = GuidHolder.TraceIdHolder.Value,
                ParentGuid = GuidHolder.ParentHolder.Value,
                ParentHttpId = GuidHolder.ParentHttpHolder.Value
            };
            if (exception != null)
            {
                log.MethodName = exception.Message;
                log.StackTrace = exception.StackTrace;
            }
            else if (formatter != null)
            {
                log.StackTrace = formatter(state, exception);
            }
            Helper.PostHelper.ProcessLog(log);
        }
    }
}
