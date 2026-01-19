using LinqKit;
using Microsoft.AspNetCore.Mvc;
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

        private bool ProcessSplitLogs(Log[] logs, DateTime happenTime)
        {
            var repo = GetRepository($"VLarge-{happenTime:yyyy-MM}");
            foreach(var log in logs)
            {
                if(repo.Any(x => x.Id == log.Id))
                {
                    var result = repo.Update(x => x.Id == log.Id,
                        Builders<Log>.Update
                        .Set(x => x.AddtionsString, log.AddtionsString)
                        .Set(x => x.Success, log.Success)
                        .Set(x => x.TotalMillionSeconds, log.TotalMillionSeconds)
                        .Set(x => x.Addtions, log.Addtions));
                }
                else
                {
                    repo.Add(log);
                }
            }
            return true;
        }

        public string AddLog(Log log)
        {
            var logs = log.PrepareLogForWrite();
            var nlog = logs.Where(x => x.PartIndex == 0).SingleOrDefault();
            var otherlogs = logs.Where(x => x.PartIndex > 0).ToArray();
            var repo = GetRepository(nlog);
            var result = repo.Add(nlog);
            ProcessSplitLogs(otherlogs, log.HappenTime);
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
            var logs = log.PrepareLogForWrite();
            var nlog = logs.Where(x => x.PartIndex == 0).SingleOrDefault();
            var otherlogs = logs.Where(x => x.PartIndex > 0).ToArray();
            var repo = GetRepository(nlog);
            UpdateResult result;
            try
            {
                if(!otherlogs.AnyX() || nlog.Success || nlog.TotalMillionSeconds > 0)
                {
                    result = repo.Update(x => x.Id == nlog.Id,
                    Builders<Log>.Update
                    .Set(x => x.AddtionsString, nlog.AddtionsString)
                    .Set(x => x.Success, nlog.Success)
                    .Set(x => x.IsSplitPart, nlog.IsSplitPart)
                    .Set(x => x.TotalMillionSeconds, nlog.TotalMillionSeconds)
                    .Set(x => x.Addtions, nlog.Addtions));
                }
                else if(nlog.IsSplitPart)
                {
                    result = repo.Update(x => x.Id == nlog.Id,
                    Builders<Log>.Update
                    .Set(x => x.IsSplitPart, nlog.IsSplitPart)
                    .Set(x => x.TotalMillionSeconds, nlog.TotalMillionSeconds));
                }
                return ProcessSplitLogs(otherlogs, log.HappenTime);
            }
            catch
            {
                result = repo.Update(x => x.Id == nlog.Id,
                Builders<Log>.Update
                .Set(x => x.Success, nlog.Success)
                .Set(x => x.IsSplitPart, nlog.IsSplitPart)
                .Set(x => x.TotalMillionSeconds, nlog.TotalMillionSeconds));
            }
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
