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

        }
    }
}
