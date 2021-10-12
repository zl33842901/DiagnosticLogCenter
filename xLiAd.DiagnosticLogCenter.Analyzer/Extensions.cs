using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.Analyzer
{
    public static class Extensions
    {
        public static IServiceCollection AddDiagnosticeLogAnalyzer<T>(this IServiceCollection services, Action<IServiceProvider, AnalyzerConfig> options = null) where T : class,IAlertServicecs
        {
            services.AddSingleton(x =>
            {
                var config = new AnalyzerConfig();
                options?.Invoke(x, config);
                return config;
            });
            services.AddScoped<IAlertServicecs, T>();
            services.AddHostedService<AnalyzerService>();
            return services;
        }
    }
}
