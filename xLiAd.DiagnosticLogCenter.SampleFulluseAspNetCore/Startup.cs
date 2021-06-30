using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Extensions.Autofac;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using xLiAd.DiagnosticLogCenter.Agent;

namespace xLiAd.DiagnosticLogCenter.SampleFulluseAspNetCore
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
            services.AddDiagnosticLog(x => x.CollectServerAddress = "localhost:5000");
            services.AddScoped<IDbConnection>(x => new SqlConnection("server=127.0.0.1;user id=sa;password=zhanglei;database=zhanglei;"));
            services.AddHttpClient();
            services.AddControllers();
        }
        public void ConfigureContainer(ContainerBuilder builder)
        {
            List<System.Reflection.Assembly> assemblies = new List<System.Reflection.Assembly>();
            assemblies.Add(typeof(Services.SampleService).Assembly);
            builder.RegisterAssemblyTypes(assemblies.ToArray()).
                Where(x => x.Name.EndsWith("service", StringComparison.OrdinalIgnoreCase) || x.Name.EndsWith("repository", StringComparison.OrdinalIgnoreCase)).AsImplementedInterfaces();
            builder.RegisterDynamicProxy();
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
