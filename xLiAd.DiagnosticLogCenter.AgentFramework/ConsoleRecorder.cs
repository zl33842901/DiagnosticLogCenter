using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.AgentFramework
{
    public static class ConsoleRecorder
    {
        public static void BeginRequest()
        {
            StackTrace stackTrace = new StackTrace(false);
            var method = stackTrace.GetFrame(1).GetMethod();

            bool shouldRecord = FilterRule.ShouldRecord(method.DeclaringType.FullName + "." + method.Name);
            if (!shouldRecord)
            {
                GuidHolder.Holder.Value = Guid.Empty;
                return;
            }
            var guid = Guid.NewGuid();
            GuidHolder.Holder.Value = guid;
            SetTraceAndPageId();
            var log = ToLog(method);
            log.LogType = LogTypeEnum.RequestBegin;
            PostHelper.ProcessLog(log);
        }

        private static void SetTraceAndPageId()
        {
            string traceId = new TracePageIdValue(DateTime.Now, DiagnosticLogConfig.Config.ClientName, DiagnosticLogConfig.Config.EnvName).ToString();
            GuidHolder.TraceIdHolder.Value = traceId;
            string pageId = traceId;
            GuidHolder.PageIdHolder.Value = pageId;

            string parentGuid = null;
            GuidHolder.ParentHolder.Value = parentGuid;

            string parentHttp = null;
            GuidHolder.ParentHttpHolder.Value = parentHttp;
        }

        private static LogEntity ToLog(MethodBase method)
        {
            var path = method.DeclaringType.FullName + "." + method.Name;
            string ip = null;
            string url = null;
            LogEntity log = new LogEntity()
            {
                Message = path,
                StackTrace = url,
                MethodName = null,
                GroupGuid = GuidHolder.Holder.Value.ToString(),
                Ip = ip,
                Level = LogLeveEnum.接口日志,
                HappenTime = DateTime.Now,
                PageId = GuidHolder.PageIdHolder.Value,
                TraceId = GuidHolder.TraceIdHolder.Value,
                ParentGuid = GuidHolder.ParentHolder.Value,
                ParentHttpId = GuidHolder.ParentHttpHolder.Value
            };
            return log;
        }

        public static void EndRequest()
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            StackTrace stackTrace = new StackTrace(false);
            var method = stackTrace.GetFrame(1).GetMethod();
            var log = ToLog(method);
            log.LogType = LogTypeEnum.RequestEndSuccess;
            PostHelper.ProcessLog(log);
        }

        public static void RequestException(Exception exception)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            StackTrace stackTrace = new StackTrace(false);
            var method = stackTrace.GetFrame(1).GetMethod();
            var log = ToLog(method);
            log.LogType = LogTypeEnum.RequestEndException;
            log.Message = exception.Message;
            log.StackTrace = exception.StackTrace;
            PostHelper.ProcessLog(log);
        }
    }
}
