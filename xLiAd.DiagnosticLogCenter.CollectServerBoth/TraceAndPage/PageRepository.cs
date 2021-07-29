using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using xLiAd.MongoEx.Repository;

namespace xLiAd.DiagnosticLogCenter.CollectServerBoth.TraceAndPage
{
    public class PageRepository : IPageRepository
    {
        private ConcurrentDictionary<string, MongoRepository<PageGroup>> Repositories = new ConcurrentDictionary<string, MongoRepository<PageGroup>>();
        private MongoUrl mongoUrl;
        private static object objForLock = new object();
        public PageRepository(MongoUrl mongoUrl)
        {
            this.mongoUrl = mongoUrl;
        }
        private MongoRepository<PageGroup> GetRepository(string logTable)
        {
            if (Repositories.ContainsKey(logTable))
                return Repositories[logTable];
            MongoRepository<PageGroup> repo = new MongoRepository<PageGroup>(this.mongoUrl, logTable);
            Repositories.TryAdd(logTable, repo);
            return repo;
        }

        private MongoRepository<PageGroup> GetRepository(DateTime date)
        {
            //var _client = new MongoClient(mongoUrl);
            //var db = _client.GetDatabase(mongoUrl.DatabaseName);
            //return db.GetCollection<PageGroup>(date.GetTraceIdTableName());
            return GetRepository(date.GetPageIdTableName());
        }

        public async Task<bool> AddOrUpdate(string pageId, DateTime happenTime, IEnumerable<PageItem> items)
        {
            if (pageId.NullOrEmpty())
                return false;
            var repo = GetRepository(happenTime);
            //Monitor.Enter(objForLock);
            var model = await repo.FindAsync(x => x.PageId == pageId);
            if (model == null)
            {
                var result = await repo.AddAsync(new PageGroup()
                {
                    CreatedOn = DateTime.Now,
                    Id = pageId,
                    Items = items.ToArray(),
                    ModifiedOn = DateTime.Now,
                    PageId = pageId
                });
                //Monitor.Exit(objForLock);
                return true;
            }
            else
            {
                //Monitor.Exit(objForLock);
                var filter = Builders<PageGroup>.Filter.Eq<string>(x => x.Id, pageId);
                var update = Builders<PageGroup>.Update
                    .Set(x => x.ModifiedOn, DateTime.Now)
                    .PushEach(x => x.Items, items);
                var result = await repo.UpdateAsync(filter, update);
                return result.ModifiedCount > 0;
            }
        }
    }

    public interface IPageRepository
    {
        Task<bool> AddOrUpdate(string pageId, DateTime happenTime, IEnumerable<PageItem> items);
    }
}
