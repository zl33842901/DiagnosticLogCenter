using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace xLiAd.DiagnosticLogCenter.AgentFramework
{
    public static class DatabaseOperationRecorder
    {
        public static void BeforeExecuteCommand(string sqlCommand, object para, Guid operationId, IDbConnection connection)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                AspnetRequestRecorder.LoadGuidsFromResponse(System.Web.HttpContext.Current);
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            BeforeExecuteCommandDo(connection.GetType().GetProperty("DataSource", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.GetValue(connection)?.ToString(),
                connection.Database, sqlCommand, para, operationId);
        }
        public static void BeforeExecuteCommand(SqlCommand sqlCommand, Guid operationId)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                AspnetRequestRecorder.LoadGuidsFromResponse(System.Web.HttpContext.Current);
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            var datasource = sqlCommand.Connection.DataSource;
            var database = sqlCommand.Connection.Database;
            var commandText = sqlCommand.CommandText;
            var para = sqlCommand.Parameters;
            BeforeExecuteCommandDo(datasource, database, commandText, para, operationId);
        }
        private static string ProcessParameters(object para)
        {
            if (para == null)
                return string.Empty;
            if(!(DiagnosticLogConfig.Config?.RecordSqlParameters ?? false))
                return string.Empty;
            if (para is SqlParameterCollection p)
                return p.ConvertToString();
            else
                return para.FormatDynamicString();
        }
        private static void BeforeExecuteCommandDo(string datasource, string database, string commandText, object para, Guid operationId)
        {
            LogEntity log = new LogEntity()
            {
                DataSource = datasource,
                Database = database,
                CommandText = commandText,
                StackTrace = commandText,
                Parameters = ProcessParameters(para),
                LogType = LogTypeEnum.SqlBefore,
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

        public static void AfterExecuteCommand(Guid operationId)
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
            PostHelper.ProcessLog(log);
        }

        public static void ErrorExecuteCommand(Guid operationId, Exception exception)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            LogEntity log = new LogEntity()
            {
                LogType = LogTypeEnum.SqlException,
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
