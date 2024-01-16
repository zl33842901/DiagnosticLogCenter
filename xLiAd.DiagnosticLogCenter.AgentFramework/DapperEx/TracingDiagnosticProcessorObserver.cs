using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.AgentFramework
{
    public class TracingDiagnosticProcessorObserver : IObserver<DiagnosticListener>
    {
        private readonly IEnumerable<ITracingDiagnosticProcessor> _tracingDiagnosticProcessors;

        public TracingDiagnosticProcessorObserver(IEnumerable<ITracingDiagnosticProcessor> tracingDiagnosticProcessors)
        {
            _tracingDiagnosticProcessors = tracingDiagnosticProcessors ??
                                           throw new ArgumentNullException(nameof(tracingDiagnosticProcessors));
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
                    Subscribe(listener, diagnosticProcessor);
                    Console.WriteLine($"Loaded diagnostic listener [{diagnosticProcessor.ListenerName}].");
                }
            }
        }

        protected virtual void Subscribe(DiagnosticListener listener, ITracingDiagnosticProcessor tracingDiagnosticProcessor)
        {
            listener.Subscribe(new TracingDiagnosticObserver(tracingDiagnosticProcessor));
        }
    }
}
