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
        public void BeforeExecuteCommand([Property(Name = "SqlString")] string sqlCommand, [Property(Name = "Params")] object para)
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
                GroupGuid = GuidHolder.Holder.Value.ToString()
            };
            Helper.PostHelper.ProcessLog(log);
        }
    }
}
