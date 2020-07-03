using System;
using System.Collections.Concurrent;
using xLiAd.DiagnosticLogCenter.Abstract;

namespace xLiAd.DiagnosticLogCenter.Agent
{
    public class TracingContext
    {
        public ConcurrentBag<LogEntity> Logs { get; } = new ConcurrentBag<LogEntity>();
    }
}
