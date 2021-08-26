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
        private readonly ICacheProvider cacheProvider;
        public PageRepository(MongoUrl mongoUrl, ICacheProvider cacheProvider)
        {
            this.mongoUrl = mongoUrl;
            this.cacheProvider = cacheProvider;
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
            bool pageHasBeenInDb;
            string ck = "PageId-" + pageId;
            var tcache = cacheProvider.Get<string>(ck);
            if (tcache != null)
                pageHasBeenInDb = true;
            else
            {
                var model = await repo.FindAsync(x => x.PageId == pageId);
                pageHasBeenInDb = model != null;
                if (model != null)
                    cacheProvider.Set(ck, "a", TimeSpan.FromMinutes(5));
            }
            //Monitor.Enter(objForLock);
            if (!pageHasBeenInDb)
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
        public async Task<PageGroup> FindByPageId(string pageId, DateTime happenTime)
        {
            var repo = GetRepository(happenTime);
            var result = await repo.FindAsync(pageId);
            return result;
        }
    }

    public interface IPageRepository
    {
        Task<bool> AddOrUpdate(string pageId, DateTime happenTime, IEnumerable<PageItem> items);
        Task<PageGroup> FindByPageId(string pageId, DateTime happenTime);
    }
}
