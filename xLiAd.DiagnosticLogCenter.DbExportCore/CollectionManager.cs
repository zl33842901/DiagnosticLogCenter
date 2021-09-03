using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.DbExportCore
{
    public class CollectionManager : ICollectionManager
    {
        private readonly MongoClient mongoClient;
        public MongoUrl MongoUrl { get; }
        private readonly IMongoDatabase mongoDatabaseBase;
        public CollectionManager(MongoUrl mongoUrl)
        {
            this.MongoUrl = mongoUrl;
            mongoClient = new MongoClient(mongoUrl);
            mongoDatabaseBase = mongoClient.GetDatabase(mongoUrl.DatabaseName);
        }
        public CollectionManager(string conn) : this(new MongoUrl(conn)) { }

        public async Task<List<string>> GetCollections()
        {
            var collections = await mongoDatabaseBase.ListCollectionNamesAsync();
            List<string> result = new List<string>();
            while (await collections.MoveNextAsync())
            {
                result.AddRange(collections.Current);
            }
            return result;
        }

        public Task DropCollection(string name)
        {
            //这么处理是因为有一次停在这一步，报了超时72分钟
            var retryTimes = 0;
            Task t = mongoDatabaseBase.DropCollectionAsync(name);
            while (!t.Wait(TimeSpan.FromSeconds(25)) && retryTimes++ < 5)
            {
                t = mongoDatabaseBase.DropCollectionAsync(name);
            }
            return Task.CompletedTask;
        }
    }

    public interface ICollectionManager
    {
        MongoUrl MongoUrl { get; }
        Task<List<string>> GetCollections();
        Task DropCollection(string name);
    }
}
