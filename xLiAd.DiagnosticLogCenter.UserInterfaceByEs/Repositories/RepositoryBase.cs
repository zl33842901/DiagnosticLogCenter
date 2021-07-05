using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.UserInterfaceByEs.Models;

namespace xLiAd.DiagnosticLogCenter.UserInterfaceByEs.Repositories
{
    public class RepositoryBase<T> : IRepository<T> where T : EntityBase
    {
        protected readonly ElasticClient client;
        protected readonly string indexName;
        public RepositoryBase(ElasticClient elasticClient, string indexName = null)
        {
            this.client = elasticClient;
            this.indexName = string.IsNullOrWhiteSpace(indexName) ? typeof(T).Name.ToLower() : indexName;
        }

        public async Task<bool> AddAsync(T t)
        {
            var response = await client.IndexAsync(t, x => x.Index(indexName));
            return response.Result == Result.Created || response.Result == Result.Updated;
        }

        public bool Add(T t)
        {
            var response = client.Index(t, s => s.Index(indexName));
            return response.Result == Result.Created || response.Result == Result.Updated;
        }

        public async Task<IReadOnlyCollection<T>> AllAsync()
        {
            var response = await client.SearchAsync<T>(x => x.Index(indexName).From(0).Size(50));
            return response.Documents;
        }

        public IReadOnlyCollection<T> All()
        {
            var response = client.Search<T>(x => x.Index(indexName).From(0).Size(50));
            return response.Documents;
        }

        public async Task<bool> UpdateAsync(T t)
        {
            var response = await client.UpdateAsync<T>(t.Id, x => x.Index(indexName).Doc(t));
            return response.Result == Result.Updated;
        }

        public bool Update(T t)
        {
            var response = client.Update<T>(t.Id, x => x.Index(indexName).Doc(t));
            return response.Result == Result.Updated;
        }

        public async Task<T> GetAsync(string id)
        {
            var result = await client.GetAsync<T>(id, x => x.Index(indexName));
            return result.Source;
        }

        public T Get(string id)
        {
            var result = client.Get<T>(id, x => x.Index(indexName));
            return result.Source;
        }

        public async Task<bool> AnyAsync()
        {
            return (await client.CountAsync<T>(x => x.Index(indexName))).Count > 0;
        }

        public bool Any()
        {
            return client.Count<T>(x => x.Index(indexName)).Count > 0;
        }

        //var rst = c.Search<TestModel5>(x => x.From(0) //跳过的数据个数
        //        .Size(50) //返回数据个数.Query(y=>x.)
        //        .Query(y => y.Term(z => z.Dic, "kwgk"))
        //        .Sort(a => a.Ascending(asc => asc.Id)).TypedKeys(null));

        //c.Update

        //var rst = c.Search<ModelHa>(x => x.From(0).Size(50));

        public async Task<(IReadOnlyCollection<T>, long)> PageListAsync<TKey>(Func<QueryContainerDescriptor<T>, QueryContainer> expression, Expression<Func<T, TKey>> order,int pageIndex, int pageSize, bool desc = false)
        {
            Func<SortDescriptor<T>, IPromise<IList<ISort>>> sort;
            if (desc)
                sort = x => x.Descending(order);
            else
                sort = x => x.Ascending(order);
            var result = await client.SearchAsync<T>(x => x.Index(indexName).Query(expression).Sort(sort)
            .From((pageIndex - 1) * pageSize)
            .Size(pageSize).TrackTotalHits());
            return (result.Documents, result.Total);
        }

        public (IReadOnlyCollection<T>, long) PageList<TKey>(Func<QueryContainerDescriptor<T>, QueryContainer> expression, Expression<Func<T, TKey>> order, int pageIndex, int pageSize, bool desc = false)
        {
            Func<SortDescriptor<T>, IPromise<IList<ISort>>> sort;
            if (desc)
                sort = x => x.Descending(order);
            else
                sort = x => x.Ascending(order);
            var result = client.Search<T>(x => x.Index(indexName).Query(expression).Sort(sort)
                .From((pageIndex - 1) * pageSize)
                .Size(pageSize)
            );
            return (result.Documents, result.Total);
        }
    }

    public interface IRepository<T> where T : EntityBase
    {
        Task<bool> AddAsync(T t);
        bool Add(T t);
        Task<IReadOnlyCollection<T>> AllAsync();
        IReadOnlyCollection<T> All();
        Task<bool> UpdateAsync(T t);
        bool Update(T t);
        Task<T> GetAsync(string id);
        T Get(string id);
        Task<bool> AnyAsync();
        bool Any();
        Task<(IReadOnlyCollection<T>, long)> PageListAsync<TKey>(Func<QueryContainerDescriptor<T>, QueryContainer> expression, Expression<Func<T, TKey>> order, int pageIndex, int pageSize, bool desc = false);
        (IReadOnlyCollection<T>, long) PageList<TKey>(Func<QueryContainerDescriptor<T>, QueryContainer> expression, Expression<Func<T, TKey>> order, int pageIndex, int pageSize, bool desc = false);
    }
}
