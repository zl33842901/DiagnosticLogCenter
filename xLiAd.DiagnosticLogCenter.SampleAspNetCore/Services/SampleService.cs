using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Net.Http;
using Dapper;
using Grpc.Net.Client;

namespace xLiAd.DiagnosticLogCenter.SampleAspNetCore.Services
{
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
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2Support", true);
            var result = dbConnection.Query<dynamic>("select * from Student where Id > @id", new { id = inputParam });
            //var rst = TestGrpc("哈哈").ConfigureAwait(false).GetAwaiter().GetResult();
            var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(19);
            var content = new StringContent("{\"employeeCode\":\"20720\",\"objectiveType\":\"my\",\"cycleId\":\"\"}");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var task = client.PostAsync("http://okr.cig.com.cn/AppInterface/GetList", content);
            var back = task.Result;
            var text = back.Content.ReadAsStringAsync().Result;
            var httpback = client.GetAsync("http://okr.cig.com.cn/").Result;
            var s = httpback.Content.ReadAsStringAsync().Result;
            return result.Count();
        }

        public async Task<string> TestGrpc(string name)
        {
            using var channel = GrpcChannel.ForAddress("http://localhost:5003", new GrpcChannelOptions() { MaxReceiveMessageSize = 32 * 1024 * 1024 });
            var client = new xLiAd.DiagnosticLogCenter.SampleGrpcServer.Greeter.GreeterClient(channel);
            var result = await client.SayHelloAsync(new SampleGrpcServer.HelloRequest() { Name = name });
            return result.Message;
        }

        public string RequestWeb(string url)
        {
            var client = httpClientFactory.CreateClient();
            var task = client.GetAsync(url);
            task.ConfigureAwait(false);
            task.Wait();
            var rst = task.Result;
            if (rst.StatusCode == System.Net.HttpStatusCode.OK)
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
            return "abc";
        }
    }

    public interface ISampleService
    {
        int QueryDb(int inputParam);
        string RequestWeb(string url);
        Task<string> Test();
    }
}
