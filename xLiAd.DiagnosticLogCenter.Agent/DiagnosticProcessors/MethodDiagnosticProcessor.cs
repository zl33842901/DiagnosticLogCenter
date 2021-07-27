using System;
using System.Collections.Generic;
using System.Text;
using xLiAd.DiagnosticLogCenter.Abstract;

namespace xLiAd.DiagnosticLogCenter.Agent.DiagnosticProcessors
{
    public class MethodDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName => "DiagnosticLogCenterListener";

        [DiagnosticName("xLiAd.DiagnosticLogCenter.Log")]
        public void Log([Property(Name = "LogType")] LogTypeEnum logType, [Property(Name = "ClassName")] string className, [Property(Name = "MethodName")] string methodName, [Property(Name = "LogContent")] string logContent)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            LogEntity log = new LogEntity()
            {
                LogType = logType,
                ClassName = className,
                MethodName = methodName,
                StackTrace = logContent,
                GroupGuid = GuidHolder.Holder.Value.ToString(),
                HappenTime = DateTime.Now,
                PageId = GuidHolder.PageIdHolder.Value,
                TraceId = GuidHolder.TraceIdHolder.Value,
                ParentGuid = GuidHolder.ParentHolder.Value,
                ParentHttpId = GuidHolder.ParentHttpHolder.Value
            };
            Helper.PostHelper.ProcessLog(log);
        }
    }
}
