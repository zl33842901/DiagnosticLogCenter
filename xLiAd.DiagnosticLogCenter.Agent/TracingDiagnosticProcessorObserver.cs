using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using xLiAd.DiagnosticLogCenter.Agent.DiagnosticProcessors;

namespace xLiAd.DiagnosticLogCenter.Agent
{
    public class TracingDiagnosticProcessorObserver : IObserver<DiagnosticListener>
    {
        private readonly IEnumerable<ITracingDiagnosticProcessor> _tracingDiagnosticProcessors;
        public TracingDiagnosticProcessorObserver(IEnumerable<ITracingDiagnosticProcessor> tracingDiagnosticProcessors)
        {
            this._tracingDiagnosticProcessors = tracingDiagnosticProcessors;
        }
        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(DiagnosticListener listener)
        {
            foreach (var diagnosticProcessor in _tracingDiagnosticProcessors)
            {
                if (listener.Name == diagnosticProcessor.ListenerName)
                {
                    listener.Subscribe(new TracingDiagnosticObserver(diagnosticProcessor));
                }
            }
        }
    }
}
