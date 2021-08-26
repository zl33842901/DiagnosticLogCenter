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
    public class TraceRepository : ITraceRepository
    {
        private ConcurrentDictionary<string, MongoRepository<TraceGroup>> Repositories = new ConcurrentDictionary<string, MongoRepository<TraceGroup>>();
        private MongoUrl mongoUrl;
        private readonly ICacheProvider cacheProvider;
        public TraceRepository(MongoUrl mongoUrl, ICacheProvider cacheProvider)
        {
            this.mongoUrl = mongoUrl;
            this.cacheProvider = cacheProvider;
        }
        private MongoRepository<TraceGroup> GetRepository(string logTable)
        {
            if (Repositories.ContainsKey(logTable))
                return Repositories[logTable];
            MongoRepository<TraceGroup> repo = new MongoRepository<TraceGroup>(this.mongoUrl, logTable);
            Repositories.TryAdd(logTable, repo);
            return repo;
        }

        private MongoRepository<TraceGroup> GetRepository(DateTime date)
        {
            //var _client = new MongoClient(mongoUrl);
            //var db = _client.GetDatabase(mongoUrl.DatabaseName);
            //return db.GetCollection<TraceGroup>(date.GetTraceIdTableName());
            return GetRepository(date.GetTraceIdTableName());
        }

        public async Task<bool> AddOrUpdate(string traceId, DateTime happenTime, IEnumerable<TraceItem> items)
        {
            if (traceId.NullOrEmpty())
                return false;
            var repo = GetRepository(happenTime);
            bool traceHasBeenInDb;
            string ck = "TraceId-" + traceId;
            var tcache = cacheProvider.Get<string>(ck);
            if (tcache != null)
                traceHasBeenInDb = true;
            else
            {
                var model = await repo.FindAsync(x => x.TraceId == traceId);
                traceHasBeenInDb = model != null;
                if(model != null)
                    cacheProvider.Set(ck, "a", TimeSpan.FromMinutes(5));
            }
            //Monitor.Enter(objForLock);
            if(!traceHasBeenInDb)
            {
                var result = await repo.AddAsync(new TraceGroup()
                {
                    CreatedOn = DateTime.Now,
                    Id = traceId,
                    Items = items.ToArray(),
                    ModifiedOn = DateTime.Now,
                    Month = happenTime.Month,
                    Year = happenTime.Year,
                    TraceId = traceId
                });
                //Monitor.Exit(objForLock);
                cacheProvider.Set(ck, "a", TimeSpan.FromMinutes(5));
                return true;
            }
            else
            {
                //Monitor.Exit(objForLock);
                var filter = Builders<TraceGroup>.Filter.Eq<string>(x => x.Id, traceId);
                var update = Builders<TraceGroup>.Update
                    .Set(x => x.ModifiedOn, DateTime.Now)
                    .PushEach(x => x.Items, items);
                var result = await repo.UpdateAsync(filter, update);
                return result.ModifiedCount > 0;
            }
        }

        //public async Task<bool> AddOrUpdate(string traceId, DateTime happenTime, string collectionName, string guid)
        //{
        //    var repo = GetRepository(happenTime);
        //    Monitor.Enter(objForLock);
        //    var model = await repo.FindAsync(x => x.TraceId == traceId);
        //    var item = new TraceItem() { CollectionName = collectionName, Guid = guid };
        //    if (model == null)
        //    {
        //        var result = await repo.AddAsync(new TraceGroup()
        //        {
        //            CreatedOn = DateTime.Now,
        //            Id = traceId,
        //            Items = new TraceItem[] { item },
        //            ModifiedOn = DateTime.Now,
        //            Month = happenTime.Month,
        //            Year = happenTime.Year,
        //            TraceId = traceId
        //        });
        //        Monitor.Exit(objForLock);
        //        return true;
        //    }
        //    else
        //    {
        //        Monitor.Exit(objForLock);
        //        var filter = Builders<TraceGroup>.Filter.Eq<string>(x => x.Id, traceId);
        //        var update = Builders<TraceGroup>.Update
        //            .Set(x => x.ModifiedOn, DateTime.Now)
        //            .Push(x => x.Items, item);
        //        var result = await repo.UpdateAsync(filter, update);
        //        return result.ModifiedCount > 0;
        //    }
        //}

        public async Task<TraceGroup> FindByTraceId(string traceId, DateTime happenTime)
        {
            var repo = GetRepository(happenTime);
            var result = await repo.FindAsync(traceId);
            return result;
        }
    }

    public interface ITraceRepository
    {
        //Task<bool> AddOrUpdate(string traceId, DateTime happenTime, string collectionName, string guid);
        Task<bool> AddOrUpdate(string traceId, DateTime happenTime, IEnumerable<TraceItem> items);
        Task<TraceGroup> FindByTraceId(string traceId, DateTime happenTime);
    }
}
