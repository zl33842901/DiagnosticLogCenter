using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.Agent.DiagnosticProcessors
{
    public interface ITracingDiagnosticProcessor
    {
        string ListenerName { get; }
    }
}
