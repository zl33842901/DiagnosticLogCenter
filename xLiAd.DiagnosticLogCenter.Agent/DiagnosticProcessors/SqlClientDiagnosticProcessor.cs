using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using xLiAd.DiagnosticLogCenter.Abstract;
using xLiAd.DiagnosticLogCenter.Agent.Helper;

namespace xLiAd.DiagnosticLogCenter.Agent.DiagnosticProcessors
{
    public class SqlClientDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName { get; } = "SqlClientDiagnosticListener";

        [DiagnosticName("System.Data.SqlClient.WriteCommandBefore")]
        public void BeforeExecuteCommand([Property(Name = "Command")] SqlCommand sqlCommand, [Property(Name = "OperationId")] Guid operationId)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            var datasource = sqlCommand.Connection.DataSource;
            var database = sqlCommand.Connection.Database;
            var commandText = sqlCommand.CommandText;
            var para = sqlCommand.Parameters;
            LogEntity log = new LogEntity()
            {
                DataSource = datasource,
                Database = database,
                CommandText = commandText,
                StackTrace = commandText,
                Parameters = (DiagnosticLogConfig.Config?.RecordSqlParameters ?? false) ? para.ConvertToString() : string.Empty,
                LogType = LogTypeEnum.SqlBefore,
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

        [DiagnosticName("System.Data.SqlClient.WriteCommandAfter")]
        public void AfterExecuteCommand([Property(Name = "OperationId")] Guid operationId)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            LogEntity log = new LogEntity()
            {
                LogType = LogTypeEnum.SqlAfter,
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

        [DiagnosticName("System.Data.SqlClient.WriteCommandError")]
        public void ErrorExecuteCommand([Property(Name = "Exception")] Exception ex, [Property(Name = "OperationId")] Guid operationId)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            LogEntity log = new LogEntity()
            {
                Message = ex.Message,
                StackTrace = ex.StackTrace,
                LogType = LogTypeEnum.SqlException,
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
    }
}
