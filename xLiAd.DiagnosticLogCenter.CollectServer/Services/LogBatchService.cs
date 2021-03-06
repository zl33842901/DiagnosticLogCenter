﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.Abstract;
using xLiAd.DiagnosticLogCenter.CollectServer.Models;
using xLiAd.DiagnosticLogCenter.CollectServer.Repositories;

namespace xLiAd.DiagnosticLogCenter.CollectServer.Services
{
    public class LogBatchService : ILogBatchService
    {
        private readonly ICacheService cacheService;
        private readonly ILogRepository logRepository;
        public LogBatchService(ICacheService cacheService, ILogRepository logRepository)
        {
            this.cacheService = cacheService;
            this.logRepository = logRepository;
        }
        public void Process(LogDto logDto)
        {
            var listNonInterface = logDto.Items.Where(x => x.GroupGuid.NullOrEmpty()).ToArray();
            var listInterface = logDto.Items.Where(x => !x.GroupGuid.NullOrEmpty()).ToArray();
            var listInterGroup = listInterface.GroupBy(x => x.GroupGuid);
            var interRst = listInterGroup.Select(x => ConvertToLog(x)).Where(x => !x.Item1.NullOrEmpty()).ToArray();
            foreach ((var key, var log) in interRst)
            {
                if (!log.Id.NullOrEmpty())
                {
                    logRepository.Update(log);
                }
                else
                {
                    log.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
                    logRepository.AddLog(log);
                }
                cacheService.Set("GUID_" + key, log, TimeSpan.FromMinutes(6));
            }
        }
        private (string, Log) ConvertToLog(IGrouping<string, LogDtoItem> item)
        {
            var start = item.FirstOrDefault(x => (LogTypeEnum)x.LogType == LogTypeEnum.RequestBegin);
            bool hasStart = start != null;
            var end = item.FirstOrDefault(x => (LogTypeEnum)x.LogType == LogTypeEnum.RequestEndException || (LogTypeEnum)x.LogType == LogTypeEnum.RequestEndSuccess);
            bool hasEnd = end != null;
            var addtions = item.Where(x => x.LogType != 21).Select(x => new LogAdditionItem()
            {
                HappenTime = System.ExtMethods.ToTime(x.HappenTime.ToString()),
                Content = x.StackTrace,
                ClassName = x.ClassName,
                CommandText = x.CommandText,
                Database = x.Database,
                DataSource = x.DataSource,
                Ip = x.Ip,
                LogType = (LogTypeEnum)x.LogType,
                Message = x.Message,
                MethodName = x.MethodName,
                Parameters = x.Parameters,
                StatuCode = x.StatuCode
            });
            if (!hasStart)
            {
                var obj = cacheService.Get("GUID_" + item.Key);
                bool existInDb = obj != null;
                if (!existInDb)
                {
                    return (null, null);
                    //throw new Exception("未找到GUID为 " + item.Key + " 的记录");
                }
                Log startLog = (Log)obj;
                startLog.PrepareLogForRead();
                //加入新的
                var nowAddtions = startLog.Addtions.ToList();
                nowAddtions.AddRange(addtions);
                startLog.Addtions = nowAddtions.OrderBy(x => x.HappenTime).ToArray();
                if (hasEnd)
                {
                    startLog.Success = end.LogType == (int)LogTypeEnum.RequestEndSuccess;
                    startLog.TotalMillionSeconds = Convert.ToInt32((System.ExtMethods.ToTime(end.HappenTime.ToString()) - startLog.HappenTime).TotalMilliseconds);
                }
                return (item.Key, startLog);
            }
            else
            {
                Log log = new Log()
                {
                    ClientName = start.ClientName,
                    CreateTime = DateTime.Now,
                    EnvironmentName = start.EnvironmentName,
                    Level = (LogLeveEnum)start.Level,
                    HappenTime = System.ExtMethods.ToTime(start.HappenTime.ToString()),
                    Success = end?.LogType == (int)LogTypeEnum.RequestEndSuccess,
                    StackTrace = start.StackTrace,
                    Message = start.Message,
                    TotalMillionSeconds = end != null ? Convert.ToInt32((System.ExtMethods.ToTime(end.HappenTime.ToString()) - System.ExtMethods.ToTime(start.HappenTime.ToString())).TotalMilliseconds) : 0,
                    Addtions = addtions.OrderBy(x => x.HappenTime).ToArray(),
                    //AddtionsString = string.Join(";", item.Select(x => x.StackTrace)),
                    Id = string.Empty
                };
                return (item.Key, log);
            }
        }
    }

    public interface ILogBatchService
    {
        void Process(LogDto logDto);
    }
}
