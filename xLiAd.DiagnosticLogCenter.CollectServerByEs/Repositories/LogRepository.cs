using LinqKit;
using Nest;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.CollectServerByEs.Models;

namespace xLiAd.DiagnosticLogCenter.CollectServerByEs.Repositories
{
    public class LogRepository : ILogRepository
    {
        private ConcurrentDictionary<string, RepositoryBase<Log>> Repositories = new ConcurrentDictionary<string, RepositoryBase<Log>>();
        private ElasticClient client;
        public LogRepository(ElasticClient client)
        {
            this.client = client;
        }
        public string AddLog(Log log)
        {
            log.PrepareLogForWrite();
            log.DoId();
            var repo = GetRepository(log);
            var result = repo.Add(log);
            return log.Id;
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

        public bool Update(Log log)
        {
            log.PrepareLogForWrite();
            var repo = GetRepository(log);
            var logindb = repo.Get(log.Id);
            logindb.Success = log.Success;
            logindb.TotalMillionSeconds = log.TotalMillionSeconds;
            var result = repo.Update(log);
            return result;
        }


        //public Log[] GetAllData(ICliEnvDate cli)
        //{
        //    var repo = GetRepository(cli);
        //    var result = repo.Search<ModelHa>.ToArray();
        //    return result;
        //}
        public bool Exist(ICliEnvDate cli)
        {
            var repo = GetRepository(cli);
            var result = repo.Any();
            return result;
        }
    }

    public interface ILogRepository
    {
        string AddLog(Log log);
        bool Update(Log log);
        //Log[] GetAllData(ICliEnvDate cli);
        bool Exist(ICliEnvDate cli);
    }
}
