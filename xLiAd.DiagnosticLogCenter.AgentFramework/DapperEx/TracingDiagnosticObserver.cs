using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.AgentFramework
{
    internal class TracingDiagnosticObserver : IObserver<KeyValuePair<string, object>>
    {
        private readonly TracingDiagnosticMethodCollection _methodCollection;

        public TracingDiagnosticObserver(ITracingDiagnosticProcessor tracingDiagnosticProcessor)
        {
            _methodCollection = new TracingDiagnosticMethodCollection(tracingDiagnosticProcessor);
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            foreach (var method in _methodCollection)
            {
                try
                {
                    method.Invoke(value.Key, value.Value);
                }
                catch (Exception exception)
                {
                    //_logger.Error("Invoke diagnostic method exception.", exception);
                }
            }
        }
    }
}
