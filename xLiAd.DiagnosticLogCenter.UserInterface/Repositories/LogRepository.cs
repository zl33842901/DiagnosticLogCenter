using LinqKit;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.Abstract;
using xLiAd.DiagnosticLogCenter.UserInterface.Models;
using xLiAd.MongoEx.Repository;

namespace xLiAd.DiagnosticLogCenter.UserInterface.Repositories
{
    public class LogRepository : ILogRepository
    {
        private ConcurrentDictionary<string, MongoRepository<Log>> Repositories = new ConcurrentDictionary<string, MongoRepository<Log>>();
        private MongoUrl mongoUrl;
        public LogRepository(MongoUrl mongoUrl)
        {
            this.mongoUrl = mongoUrl;
        }
        private MongoRepository<Log> GetRepository(string logTable)
        {
            if (Repositories.ContainsKey(logTable))
                return Repositories[logTable];
            MongoRepository<Log> repo = new MongoRepository<Log>(this.mongoUrl, logTable);
            Repositories.TryAdd(logTable, repo);
            return repo;
        }
        private MongoRepository<Log> GetRepository(ICliEnvDate log)
        {
            return GetRepository(log.GetIndexName());
        }


        private Expression<Func<Log, bool>> GetExpression(LogLookQuery lookQuery)
        {
            Expression<Func<Log, bool>> expression = null;
            if (!string.IsNullOrEmpty(lookQuery.Key))
            {
                if (!string.IsNullOrEmpty(lookQuery.Key.Trim()) && lookQuery.Key.Trim().Contains(" "))
                {
                    var keys = lookQuery.Key.Trim().Split(' ');
                    var i = 0;
                    foreach (var k in keys)
                    {
                        if (i++ == 0)
                            expression = x => x.Message.Contains(k) || x.StackTrace.Contains(k) || x.AddtionsString.Contains(k);
                        else
                            expression = expression.And(x => x.Message.Contains(k) || x.StackTrace.Contains(k) || x.AddtionsString.Contains(k));
                    }
                }
                else
                {
                    expression = x => x.Message.Contains(lookQuery.Key) || x.StackTrace.Contains(lookQuery.Key) || x.AddtionsString.Contains(lookQuery.Key);
                }
            }
            if (!string.IsNullOrEmpty(lookQuery.Level))
            {
                LogLeveEnum en = (LogLeveEnum)Convert.ToInt32(lookQuery.Level);
                Expression<Func<Log, bool>> e1 = x => x.Level == en;
                if (expression == null)
                    expression = e1;
                else
                    expression = expression.And(e1);
            }
            if (lookQuery.Level == "10")//接口日志才对执行结果和时间进行查询
            {
                if (!string.IsNullOrEmpty(lookQuery.Success))
                {
                    bool bl = lookQuery.Success == "1";
                    Expression<Func<Log, bool>> e2 = x => x.Success == bl;
                    if (expression == null)
                        expression = e2;
                    else
                        expression = expression.And(e2);
                }
                if (!string.IsNullOrEmpty(lookQuery.MSec) && lookQuery.MSec.Contains("-"))
                {
                    var msa = lookQuery.MSec.Split('-');
                    var min = msa[0].ToInt();
                    var max = msa[1].ToInt();
                    if (min > 0)
                    {
                        Expression<Func<Log, bool>> e3 = x => x.TotalMillionSeconds >= min;
                        if (expression == null)
                            expression = e3;
                        else
                            expression = expression.And(e3);
                    }
                    if (max > 0)
                    {
                        Expression<Func<Log, bool>> e4 = x => x.TotalMillionSeconds < max;
                        if (expression == null)
                            expression = e4;
                        else
                            expression = expression.And(e4);
                    }
                }
            }
            if (lookQuery.HappenTimeRegion.Length == 2)
            {
                var d0 = lookQuery.HappenTimeRegion[0];
                var d1 = lookQuery.HappenTimeRegion[1];
                if (d0.Hour > 0 || d0.Minute > 0 || d0.Second > 0)
                {
                    d0 = new DateTime(lookQuery.HappenTime.Year, lookQuery.HappenTime.Month, lookQuery.HappenTime.Day, d0.Hour, d0.Minute, d0.Second);
                    Expression<Func<Log, bool>> e5 = x => x.HappenTime >= d0;
                    if (expression == null)
                        expression = e5;
                    else
                        expression = expression.And(e5);
                }
                if (d1.Hour < 23 || d1.Minute < 59 || d0.Second < 59)
                {
                    d1 = new DateTime(lookQuery.HappenTime.Year, lookQuery.HappenTime.Month, lookQuery.HappenTime.Day, d1.Hour, d1.Minute, d1.Second);
                    d1 = d1.AddSeconds(1);
                    Expression<Func<Log, bool>> e6 = x => x.HappenTime < d1;
                    if (expression == null)
                        expression = e6;
                    else
                        expression = expression.And(e6);
                }
            }
            return expression;
        }

        public (List<Log>, long) GetLogData(LogLookQuery query, int pageIndex, int pageSize)
        {
            var repo = GetRepository(query);
            var result = repo.Pagination(pageIndex, pageSize, x => x.Id, false, GetExpression(query));
            var list = result.ToList();
            foreach (var i in list)
            {
                i.PrepareLogForRead();
            }
            return (list, result.TotalItemCount);
        }
        public Log[] GetAllData(ICliEnvDate cli)
        {
            var repo = GetRepository(cli);
            var result = repo.All().ToArray();
            return result;
        }
        public bool Exist(ICliEnvDate cli)
        {
            var repo = GetRepository(cli);
            var result = repo.Any();
            return result;
        }
        public Log[] GetByCollectionNameAndId(string collectionName, IEnumerable<string> ids)
        {
            var repo = GetRepository(collectionName);
            ids = ids.ToArray();
            var result = repo.Where(x => ids.Contains(x.Guid)).ToArray();
            return result;
        }
        public Log[] GetByCollectionNameAndTraceId(string collectionName, string traceId)
        {
            var repo = GetRepository(collectionName);
            var result = repo.Where(x => x.TraceId == traceId).ToArray();
            return result;
        }
    }

    public interface ILogRepository
    {
        Log[] GetAllData(ICliEnvDate cli);
        bool Exist(ICliEnvDate cli);
        (List<Log>, long) GetLogData(LogLookQuery query, int pageIndex, int pageSize);
        Log[] GetByCollectionNameAndId(string collectionName, IEnumerable<string> ids);
        Log[] GetByCollectionNameAndTraceId(string collectionName, string traceId);
    }
}
