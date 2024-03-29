﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Nest;
using RabbitMQ.Client;
using xLiAd.DiagnosticLogCenter.CollectServerBoth;
using xLiAd.DiagnosticLogCenter.CollectServerBoth.TraceAndPage;

namespace xLiAd.DiagnosticLogCenter.CollectServer
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
            var conf = Configuration.GetSection("Configs").Get<CollectServerBoth.ConfigEntity>();
            services.AddSingleton(new MongoUrl(conf.MongodbUrl));
            //services.AddSingleton(x => new ConnectionSettings(new Uri(conf.EsUrl)));
            //services.AddScoped(x => new ElasticClient(x.GetService<ConnectionSettings>()));
            services.AddSingleton(conf);
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 104857600;
            });

            services.AddScoped<CollectServer.Repositories.IClientRepository, CollectServer.Repositories.ClientRepository>();
            services.AddScoped<CollectServer.Repositories.ILogRepository, CollectServer.Repositories.LogRepository>();
            services.AddScoped<CollectServer.Services.IClientCacheService, CollectServer.Services.ClientCacheService>();
            services.AddSingleton<CollectServer.Services.ICacheService, CollectServer.Services.CacheService>();
            services.AddScoped<CollectServer.Services.ILogBatchService, CollectServer.Services.LogBatchService>();


            //services.AddScoped<CollectServerByEs.Repositories.IClientRepository, CollectServerByEs.Repositories.ClientRepository>();
            //services.AddScoped<CollectServerByEs.Repositories.ILogRepository, CollectServerByEs.Repositories.LogRepository>();
            //services.AddScoped<CollectServerByEs.Services.IClientCacheService, CollectServerByEs.Services.ClientCacheService>();
            //services.AddSingleton<CollectServerByEs.Services.ICacheService, CollectServerByEs.Services.CacheService>();
            //services.AddScoped<CollectServerByEs.Services.ILogBatchService, CollectServerByEs.Services.LogBatchService>();

            services.AddScoped<ITraceRepository, TraceRepository>();
            services.AddScoped<IPageRepository, PageRepository>();
            services.AddScoped<ITraceAndGroupService, TraceAndGroupService>();
            services.AddSingleton<ICacheProvider, CacheProvider>();

            services.AddSingleton(x =>
            {
                var factory = new ConnectionFactory();
                var config = x.GetService<CollectServerBoth.ConfigEntity>();
                factory.HostName = config.RabbitMqHost;
                factory.UserName = config.RabbitMqUserName;
                factory.Password = config.RabbitMqPassword;
                factory.AutomaticRecoveryEnabled = true;
                return factory;
            });
            services.AddSingleton(x =>
            {
                var config = x.GetService<CollectServerBoth.ConfigEntity>();
                var factory = x.GetService<ConnectionFactory>();
                var conn = factory.CreateConnection();
                var channel = conn.CreateModel();
                channel.ExchangeDeclare(config.RabbitMqExchangeName, "direct", durable: true, autoDelete: false, arguments: null);
                channel.QueueDeclare(config.RabbitMqQueueName, durable: true, autoDelete: false, exclusive: false, arguments: null);
                channel.QueueBind(config.RabbitMqQueueName, config.RabbitMqExchangeName, routingKey: config.RabbitMqRoutingKeyName);
                return channel;
            });
            services.AddSingleton<IRabbitMqService, RabbitMqService>();
            services.AddSingleton<IRabbitMqBehaviorService, RabbitMqBehaviorService>();

            services.AddGrpc(x => x.MaxReceiveMessageSize = 128 * 1024 * 1024);
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
                endpoints.MapGrpcService<CollectServerBoth.DiaglogService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
