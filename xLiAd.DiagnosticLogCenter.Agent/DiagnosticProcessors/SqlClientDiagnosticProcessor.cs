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
            var datasource = sqlCommand.Connection.DataSource;
            var database = sqlCommand.Connection.Database;
            var commandText = sqlCommand.CommandText;
            var para = sqlCommand.Parameters;
            LogEntity log = new LogEntity()
            {
                DataSource = datasource,
                Database = database,
                CommandText = commandText,
                Parameters = para.ConvertToString()
            };

        }

        [DiagnosticName("System.Data.SqlClient.WriteCommandAfter")]
        public void AfterExecuteCommand()
        {
            
        }

        [DiagnosticName("System.Data.SqlClient.WriteCommandError")]
        public void ErrorExecuteCommand([Property(Name = "Exception")] Exception ex)
        {
            
        }
    }
}
