using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using xLiAd.DiagnosticLogCenter.Agent.DiagnosticProcessors;

namespace xLiAd.DiagnosticLogCenter.Agent
{
    public static class Extension
    {
        public static IServiceCollection AddDiagnosticLog(this IServiceCollection services)
        {
            services.AddSingleton<ITracingDiagnosticProcessor, AspNetCoreDiagnosticProcessor>();
            services.AddSingleton<ITracingDiagnosticProcessor, DapperExDiagnosticProcessor>();
            services.AddSingleton<ITracingDiagnosticProcessor, HttpClientDiagnosticProcessor>();
            services.AddSingleton<ITracingDiagnosticProcessor, MethodDiagnosticProcessor>();
            services.AddSingleton<ITracingDiagnosticProcessor, SqlClientDiagnosticProcessor>();

            services.AddSingleton<TracingDiagnosticProcessorObserver>();
            services.AddSingleton<IHostedService, InstrumentationHostedService>();
            return services;
        }
    }
}
