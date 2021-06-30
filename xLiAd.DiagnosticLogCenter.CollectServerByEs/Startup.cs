using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;
using xLiAd.DiagnosticLogCenter.CollectServerByEs.Models;
using xLiAd.DiagnosticLogCenter.CollectServerByEs.Repositories;
using xLiAd.DiagnosticLogCenter.CollectServerByEs.Services;

namespace xLiAd.DiagnosticLogCenter.CollectServerByEs
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var conf = Configuration.GetSection("Configs").Get<ConfigEntity>();
            services.AddSingleton(x => new ConnectionSettings(new Uri(conf.ConfigDbUrl)));
            services.AddScoped(x => new ElasticClient(x.GetService<ConnectionSettings>()));
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<ILogRepository, LogRepository>();
            services.AddScoped<IClientCacheService, ClientCacheService>();
            services.AddSingleton(conf);
            services.AddSingleton<ICacheService, CacheService>();
            services.AddScoped<ILogBatchService, LogBatchService>();

            services.AddGrpc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<DiaglogService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
