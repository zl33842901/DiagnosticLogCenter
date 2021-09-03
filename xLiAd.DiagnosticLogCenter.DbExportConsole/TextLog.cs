using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using xLiAd.DiagnosticLogCenter.DbExportCore;

namespace xLiAd.DiagnosticLogCenter.DbExportConsole
{
    public class TextLog : ILog, IExportLog
    {
        private readonly ILog Provider;
        public TextLog()
        {
            string config = "<log4net>";
            config += $"<appender name=\"RollingLogFileAppender\" type=\"log4net.Appender.RollingFileAppender\"><file value=\"logs/\" />";
            config += "<param name=\"Encoding\" value=\"utf-8\" /><appendToFile value=\"true\" /><rollingStyle value=\"Composite\" /><staticLogFileName value=\"false\" />";
            config += "<datePattern value=\"yyyyMMdd'.log'\" /><maxSizeRollBackups value=\"10\" /><maximumFileSize value=\"1024MB\" />";
            config += "<layout type=\"log4net.Layout.PatternLayout\"><conversionPattern value=\"%date [%thread](%file:%line) %-5level %logger [%property{NDC}] - %message%newline\"/></layout></appender>";
            config += " <root><level value=\"ALL\" /><appender-ref ref=\"RollingLogFileAppender\" /></root>";
            config += "</log4net>";
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(config);
            ILoggerRepository repository = LogManager.CreateRepository("CIG.ELog");
            // 默认简单配置，输出至控制台
            //BasicConfigurator.Configure(repository);
            XmlConfigurator.Configure(repository, doc.DocumentElement);
            Provider = LogManager.GetLogger("CIG.ELog", "CIG.ELogger");
        }

        public bool IsDebugEnabled => Provider.IsDebugEnabled;

        public bool IsInfoEnabled => Provider.IsInfoEnabled;

        public bool IsWarnEnabled => Provider.IsWarnEnabled;

        public bool IsErrorEnabled => Provider.IsErrorEnabled;

        public bool IsFatalEnabled => Provider.IsFatalEnabled;

        public ILogger Logger => Provider.Logger;

        public void Debug(object message)
        {
            Provider.Debug(message);
        }

        public void Debug(object message, Exception exception)
        {
            Provider.Debug(message, exception);
        }

        public void DebugFormat(string format, params object[] args)
        {
            Provider.DebugFormat(format, args);
        }

        public void DebugFormat(string format, object arg0)
        {
            Provider.DebugFormat(format, arg0);
        }

        public void DebugFormat(string format, object arg0, object arg1)
        {
            Provider.DebugFormat(format, arg0, arg1);
        }

        public void DebugFormat(string format, object arg0, object arg1, object arg2)
        {
            Provider.DebugFormat(format, arg0, arg1, arg2);
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            Provider.DebugFormat(provider, format, args);
        }

        public void Error(object message)
        {
            Provider.Error(message);
        }

        public void Error(object message, Exception exception)
        {
            Provider.Error(message, exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            Provider.ErrorFormat(format, args);
        }

        public void ErrorFormat(string format, object arg0)
        {
            Provider.ErrorFormat(format, arg0);
        }

        public void ErrorFormat(string format, object arg0, object arg1)
        {
            Provider.ErrorFormat(format, arg0, arg1);
        }

        public void ErrorFormat(string format, object arg0, object arg1, object arg2)
        {
            Provider.ErrorFormat(format, arg0, arg1, arg2);
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            Provider.ErrorFormat(provider, format, args);
        }

        public void Fatal(object message)
        {
            Provider.Fatal(message);
        }

        public void Fatal(object message, Exception exception)
        {
            Provider.Fatal(message, exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            Provider.FatalFormat(format, args);
        }

        public void FatalFormat(string format, object arg0)
        {
            Provider.FatalFormat(format, arg0);
        }

        public void FatalFormat(string format, object arg0, object arg1)
        {
            Provider.FatalFormat(format, arg0, arg1);
        }

        public void FatalFormat(string format, object arg0, object arg1, object arg2)
        {
            Provider.FatalFormat(format, arg0, arg1, arg2);
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            Provider.FatalFormat(provider, format, args);
        }

        public void Info(object message)
        {
            Provider.Info(message);
        }

        public void Info(object message, Exception exception)
        {
            Provider.Info(message, exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            Provider.InfoFormat(format, args);
        }

        public void InfoFormat(string format, object arg0)
        {
            Provider.InfoFormat(format, arg0);
        }

        public void InfoFormat(string format, object arg0, object arg1)
        {
            Provider.InfoFormat(format, arg0, arg1);
        }

        public void InfoFormat(string format, object arg0, object arg1, object arg2)
        {
            Provider.InfoFormat(format, arg0, arg1, arg2);
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            Provider.InfoFormat(provider, format, args);
        }

        public void Warn(object message)
        {
            Provider.Warn(message);
        }

        public void Warn(object message, Exception exception)
        {
            Provider.Warn(message, exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            Provider.WarnFormat(format, args);
        }

        public void WarnFormat(string format, object arg0)
        {
            Provider.WarnFormat(format, arg0);
        }

        public void WarnFormat(string format, object arg0, object arg1)
        {
            Provider.WarnFormat(format, arg0, arg1);
        }

        public void WarnFormat(string format, object arg0, object arg1, object arg2)
        {
            Provider.WarnFormat(format, arg0, arg1, arg2);
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
            Provider.WarnFormat(provider, format, args);
        }
    }
}
