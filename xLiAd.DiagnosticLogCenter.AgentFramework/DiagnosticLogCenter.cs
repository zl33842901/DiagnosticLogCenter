using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;

namespace xLiAd.DiagnosticLogCenter.AgentFramework
{
    public static class DiagnosticLogCenter
    {
        public static void AdditionLog(string content, string message = null, bool isException = false)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                AspnetRequestRecorder.LoadGuidsFromResponse(System.Web.HttpContext.Current);
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            StackTrace stackTrace = new StackTrace(false);
            var method = stackTrace.GetFrame(1).GetMethod();
            LogEntity log = new LogEntity()
            {
                Message = message,
                MethodName = method.Name,
                ClassName = method.DeclaringType.Name,
                StackTrace = content,
                LogType = isException ? LogTypeEnum.MethodException : LogTypeEnum.MethodAddition,
                HappenTime = DateTime.Now,
                GroupGuid = GuidHolder.Holder.Value.ToString(),
                PageId = GuidHolder.PageIdHolder.Value,
                TraceId = GuidHolder.TraceIdHolder.Value,
                ParentGuid = GuidHolder.ParentHolder.Value,
                ParentHttpId = GuidHolder.ParentHttpHolder.Value
            };
            PostHelper.ProcessLog(log);
        }
    }
}
