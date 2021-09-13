using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using xLiAd.DapperEx.Repository;
using xLiAd.DiagnosticLogCenter.Agent;
using xLiAd.DiagnosticLogCenter.Agent.DiagnosticProcessors;

namespace xLiAd.DiagnosticLogCenter.SampleConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceCollection services = new ServiceCollection();
            services.AddDiagnosticLog(x =>
            {
                x.CollectServerAddress = "172.16.101.28:8814";
                x.ClientName = "abs";
                x.EnvName = "DEV";
                x.RecordHttpClientBody = false;
                x.RecordSqlParameters = true;
            });
            services.AddScoped<IDbConnection>(x => new SqlConnection("server=127.0.0.1;user id=sa;password=zhanglei;database=zhanglei;"));
            var sp = services.BuildServiceProvider();
            var ss = sp.GetService<IEnumerable<IHostedService>>();
            foreach (var s in ss)
                s.StartAsync(new System.Threading.CancellationToken()).ConfigureAwait(false).GetAwaiter().GetResult();

            for(var i = 0; i < 2; i++)
            {
                var scope = sp.CreateScope();
                var serviceProvider = scope.ServiceProvider;
                var dlg = serviceProvider.GetService<ConsoleDiagnosticProcessor>();
                dlg.BeginRequest();
                var db = serviceProvider.GetService<IDbConnection>();
                var repo = new Repository<Student>(db);
                var rst = repo.Where(x => x.Id == 2);
                if (i == 0)
                    dlg.EndRequest();
                else
                    dlg.RequestException(new Exception("错误来了"));
            }
        }
    }

    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
