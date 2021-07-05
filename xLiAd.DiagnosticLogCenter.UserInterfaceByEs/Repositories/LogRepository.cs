using Nest;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.Abstract;
using xLiAd.DiagnosticLogCenter.UserInterfaceByEs.Models;

namespace xLiAd.DiagnosticLogCenter.UserInterfaceByEs.Repositories
{
    public class LogRepository : ILogRepository
    {
        private ConcurrentDictionary<string, RepositoryBase<Log>> Repositories = new ConcurrentDictionary<string, RepositoryBase<Log>>();
        private ElasticClient client;
        public LogRepository(ElasticClient client)
        {
            this.client = client;
        }
        private RepositoryBase<Log> GetRepository(string logTable)
        {
            if (Repositories.ContainsKey(logTable))
                return Repositories[logTable];
            RepositoryBase<Log> repo = new RepositoryBase<Log>(client, logTable);
            Repositories.TryAdd(logTable, repo);
            return repo;
        }
        private RepositoryBase<Log> GetRepository(ICliEnvDate log)
        {
            return GetRepository(log.GetIndexName());
        }

        private Func<QueryContainerDescriptor<Log>, QueryContainer> GetQueryContainer(LogLookQuery lookQuery)
        {
            Expression<Func<QueryContainerDescriptor<Log>, QueryContainer>> func = null;
            if (!lookQuery.Key.NullOrEmpty() && !lookQuery.Key.Trim().NullOrEmpty())
            {
                var realKey = lookQuery.Key.Trim();
                if(lookQuery.QueryMode == 1)
                {
                    if (realKey.Contains(' '))
                    {
                        var keys = lookQuery.Key.Trim().Split(' ');
                        var i = 0;
                        foreach (var k in keys)
                        {
                            var rk = $"{k}";
                            if (i++ == 0)
                                func = x => x.Wildcard(y => y.AddtionsString, rk, null, null, null);
                            else
                                func = func.And(x => x.Wildcard(y => y.AddtionsString, rk, null, null, null));
                        }
                    }
                    else
                    {
                        func = x => x.Wildcard(y => y.AddtionsString, $"{realKey}", null, null, null);
                    }
                }
                else
                {
                    //if (realKey.Contains(' '))
                    //{
                        var keys = lookQuery.Key.Trim().Split(' ');
                        var fs = keys.Select(x =>
                        {
                            Func<QueryContainerDescriptor<Log>, QueryContainer> f = y => y.Match(z => z.Field(w => w.AddtionsString).Query(x));
                            return f;
                        });
                        func = x => x.Bool(y => y.Must(fs));
                    //}
                    //else
                    //{
                    //    func = x => x.Match(y => y.Field(z => z.AddtionsString).Query(realKey));
                    //}
                }
            }
            if(lookQuery.HappenTimeRegion.Length == 2)
            {
                var d0 = lookQuery.HappenTimeRegion[0];
                var d1 = lookQuery.HappenTimeRegion[1];
                if (d0.Hour > 0 || d0.Minute > 0 || d0.Second > 0 || d1.Hour < 23 || d1.Minute < 59 || d1.Second < 59)
                {
                    d0 = new DateTime(lookQuery.HappenTime.Year, lookQuery.HappenTime.Month, lookQuery.HappenTime.Day, d0.Hour, d0.Minute, d0.Second);
                    d1 = new DateTime(lookQuery.HappenTime.Year, lookQuery.HappenTime.Month, lookQuery.HappenTime.Day, d1.Hour, d1.Minute, d1.Second);
                    func = func.And(x => x.DateRange(y => y.Field(z => z.HappenTime).LessThan(d1.ToUniversalTime()).GreaterThan(d0.ToUniversalTime())));
                }
            }
            return func?.Compile();
        }

        //private Expression<Func<Log, bool>> GetExpression(LogLookQuery lookQuery)
        //{
        //    Expression<Func<Log, bool>> expression = null;
        //    if (!string.IsNullOrEmpty(lookQuery.Key))
        //    {
        //        if (!string.IsNullOrEmpty(lookQuery.Key.Trim()) && lookQuery.Key.Trim().Contains(" "))
        //        {
        //            var keys = lookQuery.Key.Trim().Split(' ');
        //            var i = 0;
        //            foreach (var k in keys)
        //            {
        //                if (i++ == 0)
        //                    expression = x => x.Message.Contains(k) || x.StackTrace.Contains(k) || x.AddtionsString.Contains(k);
        //                else
        //                    expression = expression.And(x => x.Message.Contains(k) || x.StackTrace.Contains(k) || x.AddtionsString.Contains(k));
        //            }
        //        }
        //        else
        //        {
        //            expression = x => x.Message.Contains(lookQuery.Key) || x.StackTrace.Contains(lookQuery.Key) || x.AddtionsString.Contains(lookQuery.Key);
        //        }
        //    }
        //    if (lookQuery.HappenTimeRegion.Length == 2)
        //    {
        //        var d0 = lookQuery.HappenTimeRegion[0];
        //        var d1 = lookQuery.HappenTimeRegion[1];
        //        if (d0.Hour > 0 || d0.Minute > 0 || d0.Second > 0)
        //        {
        //            d0 = new DateTime(lookQuery.HappenTime.Year, lookQuery.HappenTime.Month, lookQuery.HappenTime.Day, d0.Hour, d0.Minute, d0.Second);
        //            Expression<Func<Log, bool>> e5 = x => x.HappenTime >= d0;
        //            if (expression == null)
        //                expression = e5;
        //            else
        //                expression = expression.And(e5);
        //        }
        //        if (d1.Hour < 23 || d1.Minute < 59 || d0.Second < 59)
        //        {
        //            d1 = new DateTime(lookQuery.HappenTime.Year, lookQuery.HappenTime.Month, lookQuery.HappenTime.Day, d1.Hour, d1.Minute, d1.Second);
        //            d1 = d1.AddSeconds(1);
        //            Expression<Func<Log, bool>> e6 = x => x.HappenTime < d1;
        //            if (expression == null)
        //                expression = e6;
        //            else
        //                expression = expression.And(e6);
        //        }
        //    }
        //    return expression;
        //}

        public async Task<(List<Log>, long)> GetLogData(LogLookQuery query, int pageIndex, int pageSize)
        {
            var repo = GetRepository(query);
            (var result, var total) = await repo.PageListAsync(GetQueryContainer(query), x => x.CreateTime, pageIndex, pageSize, true);
            var list = result.ToList();
            foreach (var i in list)
            {
                i.PrepareLogForRead();
            }
            return (list, total);
        }
        public async Task<Log[]> GetAllData(ICliEnvDate cli)
        {
            var repo = GetRepository(cli);
            var result = (await repo.AllAsync()).ToArray();
            return result;
        }
        public async Task<bool> Exist(ICliEnvDate cli)
        {
            var repo = GetRepository(cli);
            var result = await repo.AnyAsync();
            return result;
        }
    }

    public interface ILogRepository
    {
        Task<Log[]> GetAllData(ICliEnvDate cli);
        Task<bool> Exist(ICliEnvDate cli);
        Task<(List<Log>, long)> GetLogData(LogLookQuery query, int pageIndex, int pageSize);
    }
}
