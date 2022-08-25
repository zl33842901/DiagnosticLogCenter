using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.SampleFulluseAspNetCore.Services
{
    [AspectLog]
    public class SampleService : ISampleService
    {
        private readonly IDbConnection dbConnection;
        private readonly IHttpClientFactory httpClientFactory;
        public SampleService(IDbConnection dbConnection, IHttpClientFactory httpClientFactory)
        {
            this.dbConnection = dbConnection;
            this.httpClientFactory = httpClientFactory;
        }

        public int QueryDb(int inputParam)
        {
            var result = dbConnection.Query<dynamic>("select * from Student where Id > @id", new { id = 2 });
            return result.Count();
        }

        public string RequestWeb(string url)
        {
            var client = httpClientFactory.CreateClient();
            DiagnosticLogCenter.AdditionLog("手写日志。");
            var task = client.GetAsync(url);
            task.ConfigureAwait(false);
            task.Wait();
            var rst = task.Result;
            if (rst.StatusCode== System.Net.HttpStatusCode.OK)
            {
                return rst.Content.ReadAsStringAsync().Result;
            }
            else
            {
                return "error";
            }
        }

        public async Task<string> Test()
        {
            await Task.Delay(3000);
            throw new NotImplementedException("这是一个异常");
            return "abc";
        }

        public Task<string> Test1()
        {
            Task.Delay(3000).ConfigureAwait(false).GetAwaiter().GetResult();
            return Task.FromResult("abc");
        }

        public void Test2()
        {
            System.Threading.Thread.Sleep(5000);
        }

        public async Task Test3()
        {
            await Task.Delay(3000);
        }
    }

    public interface ISampleService
    {
        int QueryDb(int inputParam);
        string RequestWeb(string url);
        Task<string> Test();
        Task<string> Test1();
        void Test2();
        Task Test3();
    }
}
