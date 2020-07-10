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
        public void BeforeExecuteCommand([Property(Name = "Command")] SqlCommand sqlCommand)
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
                GroupGuid = GuidHolder.Holder.Value.ToString()
            };
            Helper.PostHelper.ProcessLog(log);
        }

        [DiagnosticName("System.Data.SqlClient.WriteCommandAfter")]
        public void AfterExecuteCommand()
        {
            
        }

        [DiagnosticName("System.Data.SqlClient.WriteCommandError")]
        public void ErrorExecuteCommand([Property(Name = "Exception")] Exception ex)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            LogEntity log = new LogEntity()
            {
                Message = ex.Message,
                StackTrace = ex.StackTrace,
                LogType = LogTypeEnum.SqlException,
                HappenTime = DateTime.Now,
                GroupGuid = GuidHolder.Holder.Value.ToString()
            };
            Helper.PostHelper.ProcessLog(log);
        }
    }
}
