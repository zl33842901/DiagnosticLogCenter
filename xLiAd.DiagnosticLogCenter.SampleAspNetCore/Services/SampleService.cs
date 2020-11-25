using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Net.Http;
using Dapper;

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
            var result = dbConnection.Query<dynamic>("select * from Student where Id > @id", new { id = 2 });
            return result.Count();
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
