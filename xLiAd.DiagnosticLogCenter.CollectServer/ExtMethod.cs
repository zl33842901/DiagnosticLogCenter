using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.CollectServer.Models;

namespace xLiAd.DiagnosticLogCenter.CollectServer
{
    public static class ExtMethod
    {
        private const string SplitString = ";-^45#24Suehwi7&58$%;";
        public static string GetIndexName(this ICliEnvDate cliEnvDate)
        {
            var dbp = "LogCenter-" + cliEnvDate.ClientName.ToLower() + "-" + cliEnvDate.EnvironmentName.ToLower() + "-" + cliEnvDate.HappenTime.ToString("yyyy.MM.dd");
            return dbp;
        }

        public static string GetUrl(this ICliEnvDate cliEnvDate)
        {
            var url = "/Log/Look/" + cliEnvDate.ClientName + "/" + cliEnvDate.EnvironmentName + "/" + cliEnvDate.HappenTime.ToString("yyyy-MM-dd");
            return url;
        }


        public static List<Log> PrepareLogForWrite(this Log log)
        {
            if(!log.Addtions.Any())
                return new List<Log> { log };
            const int maxSizeBytes = 10 * 1024 * 1024; // 10MB

            var result = new List<Log>();
            var currentAdditionsContent = new List<string>();
            var currentAdditionsItems = new List<LogAdditionItem>();
            long currentSize = 0;
            int partIndex = 0;

            // 遍历所有附加项
            foreach (var addition in log.Addtions)
            {
                var content = addition.Content ?? string.Empty;
                var contentSize = System.Text.Encoding.UTF8.GetByteCount(content) + (addition.CommandText?.Length ?? 0) + 300;//加上预估 addition 本身序列化的大小

                // 如果添加当前项会超过限制，并且当前批次已经有内容，则先保存当前批次
                if (currentSize + contentSize > maxSizeBytes && currentAdditionsContent.Count > 0)
                {
                    // 创建当前部分的日志
                    var partLog = CreatePartLog(log, partIndex, currentAdditionsContent, currentAdditionsItems);
                    result.Add(partLog);

                    // 重置批次
                    currentAdditionsContent.Clear();
                    currentAdditionsItems.Clear();
                    currentSize = 0;
                    partIndex++;
                }

                // 添加当前项到批次
                currentAdditionsContent.Add(content);
                currentAdditionsItems.Add(CreateLogAdditionItem(addition));
                currentSize += contentSize;
            }

            // 处理最后一批（如果有）
            if (currentAdditionsContent.Count > 0)
            {
                var partLog = CreatePartLog(log, partIndex, currentAdditionsContent, currentAdditionsItems);
                result.Add(partLog);
            }
            if (result.Count > 1)
                result[0].IsSplitPart = true;
            return result;
        }

        private static LogAdditionItem CreateLogAdditionItem(LogAdditionItem source)
        {
            // 保持你原有的逻辑
            return new LogAdditionItem()
            {
                Content = null,
                HappenTime = source.HappenTime,
                StatuCode = source.StatuCode,
                ClassName = source.ClassName,
                Parameters = source.Parameters,
                MethodName = source.MethodName,
                Message = source.Message,
                LogType = source.LogType,
                Ip = source.Ip,
                DataSource = source.DataSource,
                CommandText = source.CommandText,
                Database = source.Database,
                ParentHttpId = source.ParentHttpId,
                HttpId = source.HttpId,
                PageId = source.PageId,
                ParentGuid = source.ParentGuid,
                TraceId = source.TraceId
            };
        }

        private static Log CreatePartLog(Log originalLog, int partIndex,
            List<string> additionsContent, List<LogAdditionItem> additionsItems)
        {
            // 克隆原始日志，但只修改必要的字段
            var partLog = new Log
            {
                // 这里需要根据你的Log类的实际构造函数来复制所有必要的属性
                // 示例代码，请根据实际情况调整：
                Id = originalLog.Id + (partIndex > 0 ? $"_part{partIndex}" : ""),
                Level = originalLog.Level,
                Success = originalLog.Success,
                ClientName = originalLog.ClientName,
                CreatedOn = originalLog.CreatedOn,
                CreateTime = originalLog.CreateTime,
                EnvironmentName = originalLog.EnvironmentName,
                Guid = originalLog.Guid,
                HappenTime = originalLog.HappenTime,
                Message = originalLog.Message,
                ModifiedOn = originalLog.ModifiedOn,
                PageId = originalLog.PageId,
                ParentGuid = originalLog.ParentGuid,
                ParentHttpId=originalLog.ParentHttpId,
                StackTrace = originalLog.StackTrace,
                TotalMillionSeconds = originalLog.TotalMillionSeconds,
                TraceId=originalLog.TraceId,

                // 原始方法中的核心逻辑
                AddtionsString = string.Join(SplitString, additionsContent),
                Addtions = additionsItems.ToArray(),

                // 添加拆分标识（如果需要的话）
                IsSplitPart = partIndex > 0,
                PartIndex = partIndex
            };

            return partLog;
        }



        //public static void PrepareLogForWrite(this Log log)
        //{
        //    log.AddtionsString = string.Join(SplitString, log.Addtions.Select(x => x.Content));
        //    log.Addtions = log.Addtions.Select(x => new LogAdditionItem() 
        //    { 
        //        Content = null, 
        //        HappenTime = x.HappenTime,
        //        StatuCode = x.StatuCode,
        //        ClassName = x.ClassName,
        //        Parameters = x.Parameters,
        //        MethodName = x.MethodName,
        //        Message = x.Message,
        //        LogType = x.LogType,
        //        Ip = x.Ip,
        //        DataSource = x.DataSource,
        //        CommandText = x.CommandText,
        //        Database = x.Database,
        //        ParentHttpId = x.ParentHttpId,
        //        HttpId = x.HttpId,
        //        PageId = x.PageId,
        //        ParentGuid = x.ParentGuid,
        //        TraceId = x.TraceId
        //    }).ToArray();
        //}
        public static void PrepareLogForRead(this Log log)
        {
            if (string.IsNullOrEmpty(log.AddtionsString))
                return;
            bool allContentNull = log.Addtions.Length > 0 && !log.Addtions.Any(x => x.Content != null);
            if (log.AddtionsString.Contains(SplitString) || allContentNull)
            {
                var contents = log.AddtionsString.Split(new string[] { SplitString }, StringSplitOptions.None);
                for (var i = 0; i < log.Addtions.Length; i++)
                {
                    if (contents.Length > i)
                        log.Addtions[i].Content = contents[i];
                }
            }
        }
    }
}
