using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using xLiAd.DiagnosticLogCenter.Agent;
using xLiAd.DiagnosticLogCenter.SampleAspNetCore.Services;

namespace xLiAd.DiagnosticLogCenter.SampleAspNetCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDiagnosticLog(x =>
            {
                x.CollectServerAddress = "172.16.101.28:8814";
                x.ClientName = "abs";
                x.EnvName = "DEV";
                x.RecordHttpClientBody = false;
                x.RecordSqlParameters = true;
            });
            services.AddScoped<IDbConnection>(x => new SqlConnection("server=127.0.0.1;user id=sa;password=zhanglei;database=zhanglei;"));
            services.AddScoped<ISampleService, SampleService>();
            services.AddHttpClient();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
