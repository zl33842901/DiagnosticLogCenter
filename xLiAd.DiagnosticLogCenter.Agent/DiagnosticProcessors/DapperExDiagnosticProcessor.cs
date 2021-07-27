using System;
using System.Collections.Generic;
using System.Text;
using xLiAd.DiagnosticLogCenter.Abstract;
using xLiAd.DiagnosticLogCenter.Agent.Helper;

namespace xLiAd.DiagnosticLogCenter.Agent.DiagnosticProcessors
{
    public class DapperExDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName => "DapperExDiagnosticListener";

        [DiagnosticName("xLiAd.DapperEx.CommandBefore")]
        public void BeforeExecuteCommand([Property(Name = "SqlString")] string sqlCommand, [Property(Name = "Params")] object para, [Property(Name = "OperationId")]Guid operationId)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            LogEntity log = new LogEntity()
            {
                CommandText = sqlCommand,
                StackTrace = sqlCommand,
                Parameters = (DiagnosticLogConfig.Config?.RecordSqlParameters ?? false) ? para.FormatDynamicString() : string.Empty,
                LogType = LogTypeEnum.DapperExSqlBefore,
                HappenTime = DateTime.Now,
                GroupGuid = GuidHolder.Holder.Value.ToString(),
                PageId = GuidHolder.PageIdHolder.Value,
                TraceId = GuidHolder.TraceIdHolder.Value,
                ParentGuid = GuidHolder.ParentHolder.Value,
                ParentHttpId = GuidHolder.ParentHttpHolder.Value,
                HttpId = operationId.ToString()
            };
            Helper.PostHelper.ProcessLog(log);
        }

        [DiagnosticName("xLiAd.DapperEx.CommandAfter")]
        public void AfterExecuteCommand([Property(Name = "OperationId")] Guid operationId)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            LogEntity log = new LogEntity()
            {
                LogType = LogTypeEnum.DapperExSqlAfter,
                HappenTime = DateTime.Now,
                GroupGuid = GuidHolder.Holder.Value.ToString(),
                PageId = GuidHolder.PageIdHolder.Value,
                TraceId = GuidHolder.TraceIdHolder.Value,
                ParentGuid = GuidHolder.ParentHolder.Value,
                ParentHttpId = GuidHolder.ParentHttpHolder.Value,
                HttpId = operationId.ToString()
            };
            Helper.PostHelper.ProcessLog(log);
        }

        [DiagnosticName("xLiAd.DapperEx.CommandError")]
        public void ErrorExecuteCommand([Property(Name = "OperationId")] Guid operationId, [Property(Name = "Exception")] Exception exception)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            LogEntity log = new LogEntity()
            {
                LogType = LogTypeEnum.DapperExSqlException,
                HappenTime = DateTime.Now,
                GroupGuid = GuidHolder.Holder.Value.ToString(),
                PageId = GuidHolder.PageIdHolder.Value,
                TraceId = GuidHolder.TraceIdHolder.Value,
                ParentGuid = GuidHolder.ParentHolder.Value,
                ParentHttpId = GuidHolder.ParentHttpHolder.Value,
                HttpId = operationId.ToString(),
                Message = exception.Message,
                StackTrace = exception.StackTrace
            };
            Helper.PostHelper.ProcessLog(log);
        }
    }
}
