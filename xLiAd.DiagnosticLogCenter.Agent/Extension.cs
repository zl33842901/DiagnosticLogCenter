using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using xLiAd.DiagnosticLogCenter.Agent.DiagnosticProcessors;
using xLiAd.DiagnosticLogCenter.Agent.Helper;

namespace xLiAd.DiagnosticLogCenter.Agent
{
    public static class Extension
    {
        public static IServiceCollection AddDiagnosticLog(this IServiceCollection services, Action<DiagnosticLogConfig> action = null)
        {
            DiagnosticLogConfig config = new DiagnosticLogConfig();
            action?.Invoke(config);
            if (!config.Enable)
                return services;
            DiagnosticLogConfig.Config = config;
            services.AddSingleton<ITracingDiagnosticProcessor, AspNetCoreDiagnosticProcessor>();
            if (config.EnableDapperEx)
                services.AddSingleton<ITracingDiagnosticProcessor, DapperExDiagnosticProcessor>();
            if (config.EnableHttpClient)
                services.AddSingleton<ITracingDiagnosticProcessor, HttpClientDiagnosticProcessor>();
            if (config.EnableMethod)
                services.AddSingleton<ITracingDiagnosticProcessor, MethodDiagnosticProcessor>();
            if (config.EnableSqlClient)
                services.AddSingleton<ITracingDiagnosticProcessor, SqlClientDiagnosticProcessor>();

            services.AddSingleton<TracingDiagnosticProcessorObserver>();
            services.AddSingleton<IHostedService, InstrumentationHostedService>();
            FilterRule.AllowPath = config.AllowPath;
            FilterRule.ForbbidenPath = config.ForbbidenPath;

            PostHelper.Init(config.CollectServerAddress, config.ClientName, config.EnvName, config.TimeoutBySecond);
            return services;
        }
    }
}
