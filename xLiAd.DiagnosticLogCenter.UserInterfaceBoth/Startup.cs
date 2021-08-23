using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.UserInterface.Models;
using xLiAd.DiagnosticLogCenter.UserInterface.Repositories;
using xLiAd.DiagnosticLogCenter.UserInterface.Services;

namespace xLiAd.DiagnosticLogCenter.UserInterfaceBoth
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public virtual void ConfigureServices(IServiceCollection services)
        {

            MongoDB.Bson.Serialization.BsonSerializer.RegisterSerializer(typeof(DateTime), MongoDB.Bson.Serialization.Serializers.DateTimeSerializer.LocalInstance);
            var conf = Configuration.GetSection("Configs").Get<ConfigEntity>();
            services.AddSingleton(new MongoUrl(conf.ConfigDbUrl));
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<ILogRepository, LogRepository>();
            services.AddSingleton(conf);
            services.AddScoped<IConfigService, ConfigService>();
            services.AddScoped<ILogReadService, LogReadService>();
            services.AddScoped<CollectServerBoth.TraceAndPage.IPageRepository, CollectServerBoth.TraceAndPage.PageRepository>();
            services.AddScoped<CollectServerBoth.TraceAndPage.ITraceRepository, CollectServerBoth.TraceAndPage.TraceRepository>();
            services.AddScoped<ITraceAndPageService, TraceAndPageService>();
            services.AddHttpContextAccessor();
            services.AddHttpClient();
            services.AddControllersWithViews().AddJsonOptions(x => {
                x.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All);
                x.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
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
