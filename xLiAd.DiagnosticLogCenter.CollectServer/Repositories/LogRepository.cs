using LinqKit;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.CollectServer.Models;
using xLiAd.MongoEx.Repository;

namespace xLiAd.DiagnosticLogCenter.CollectServer.Repositories
{
    public class LogRepository : ILogRepository
    {
        private ConcurrentDictionary<string, MongoRepository<Log>> Repositories = new ConcurrentDictionary<string, MongoRepository<Log>>();
        private MongoUrl mongoUrl;
        public LogRepository(MongoUrl mongoUrl)
        {
            this.mongoUrl = mongoUrl;
        }
        public string AddLog(Log log)
        {
            log.PrepareLogForWrite();
            var repo = GetRepository(log);
            var result = repo.Add(log);
            return result.Id;
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

        public bool Update(Log log)
        {
            log.PrepareLogForWrite();
            var repo = GetRepository(log);
            var result = repo.Update(x => x.Id == log.Id,
                Builders<Log>.Update
                .Set(x => x.AddtionsString, log.AddtionsString)
                .Set(x => x.Success, log.Success)
                .Set(x => x.TotalMillionSeconds, log.TotalMillionSeconds)
                .Set(x => x.Addtions, log.Addtions));
            return result.ModifiedCount > 0;
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
            var result = repo.Count();
            return result > 0;
        }
    }

    public interface ILogRepository
    {
        string AddLog(Log log);
        bool Update(Log log);
        Log[] GetAllData(ICliEnvDate cli);
        bool Exist(ICliEnvDate cli);
    }
}
