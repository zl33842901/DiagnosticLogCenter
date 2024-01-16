using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.AgentFramework
{
    public class DapperExDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName => "DapperExDiagnosticListener";

        [DiagnosticName("xLiAd.DapperEx.CommandBefore")]
        public void BeforeExecuteCommand([Property(Name = "SqlString")] string sqlCommand, [Property(Name = "Params")] object para, [Property(Name = "OperationId")] Guid operationId, [Property(Name = "DbConnection")] IDbConnection connection)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                AspnetRequestRecorder.LoadGuidsFromResponse(System.Web.HttpContext.Current);
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
                HttpId = operationId.ToString(),
                Database = connection.Database,
                DataSource = connection.GetType().GetProperty("DataSource", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.GetValue(connection)?.ToString()
            };
            PostHelper.ProcessLog(log);
        }

        [DiagnosticName("xLiAd.DapperEx.CommandAfter")]
        public void AfterExecuteCommand([Property(Name = "OperationId")] Guid operationId)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                AspnetRequestRecorder.LoadGuidsFromResponse(System.Web.HttpContext.Current);
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
            PostHelper.ProcessLog(log);
        }

        [DiagnosticName("xLiAd.DapperEx.CommandError")]
        public void ErrorExecuteCommand([Property(Name = "OperationId")] Guid operationId, [Property(Name = "Exception")] Exception exception)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                AspnetRequestRecorder.LoadGuidsFromResponse(System.Web.HttpContext.Current);
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
            PostHelper.ProcessLog(log);
        }
    }
}
