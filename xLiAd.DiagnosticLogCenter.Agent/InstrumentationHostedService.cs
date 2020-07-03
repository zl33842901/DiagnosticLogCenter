using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.Agent
{
    public class InstrumentationHostedService : IHostedService
    {
        private readonly TracingDiagnosticProcessorObserver observer;
        public InstrumentationHostedService(TracingDiagnosticProcessorObserver observer)
        {
            this.observer = observer;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            System.Diagnostics.DiagnosticListener.AllListeners.Subscribe(observer);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
