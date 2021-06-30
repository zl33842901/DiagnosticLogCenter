using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nest;
using xLiAd.DiagnosticLogCenter.UserInterfaceByEs.Models;
using xLiAd.DiagnosticLogCenter.UserInterfaceByEs.Repositories;
using xLiAd.DiagnosticLogCenter.UserInterfaceByEs.Services;

namespace xLiAd.DiagnosticLogCenter.UserInterfaceByEs
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
            var conf = Configuration.GetSection("Configs").Get<ConfigEntity>();
            services.AddSingleton(x => new ConnectionSettings(new Uri(conf.ConfigDbUrl)));
            services.AddScoped(x => new ElasticClient(x.GetService<ConnectionSettings>()));
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<ILogRepository, LogRepository>();
            services.AddSingleton(conf);
            services.AddScoped<IConfigService, ConfigService>();
            services.AddScoped<ILogReadService, LogReadService>();
            services.AddHttpContextAccessor();
            services.AddHttpClient();
            services.AddControllersWithViews().AddJsonOptions(x => {
                x.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All);
                x.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
