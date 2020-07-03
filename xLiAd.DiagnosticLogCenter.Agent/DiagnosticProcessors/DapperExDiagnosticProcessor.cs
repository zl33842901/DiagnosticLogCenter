using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.Agent.DiagnosticProcessors
{
    public class DapperExDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName => "DapperExDiagnosticListener";

        [DiagnosticName("xLiAd.DapperEx.CommandBefore")]
        public void BeforeExecuteCommand([Property(Name = "SqlString")] string sqlCommand, [Property(Name = "Params")] object para)
        {
            
        }
    }
}
