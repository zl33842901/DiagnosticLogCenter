using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.CollectServerByEs.Models;

namespace xLiAd.DiagnosticLogCenter.CollectServerByEs
{
    public static class ExtMethod
    {
        private const string SplitString = ";-^45#24Si7&5$%;";
        public static string GetIndexName(this ICliEnvDate cliEnvDate)
        {
            var dbp = "logcenter-" + cliEnvDate.ClientName.ToLower() + "-" + cliEnvDate.EnvironmentName.ToLower() + "-" + cliEnvDate.HappenTime.ToString("yyyy.MM.dd");
            return dbp;
        }

        public static string GetUrl(this ICliEnvDate cliEnvDate)
        {
            var url = "/Log/Look/" + cliEnvDate.ClientName + "/" + cliEnvDate.EnvironmentName + "/" + cliEnvDate.HappenTime.ToString("yyyy-MM-dd");
            return url;
        }
        public static void PrepareLogForWrite(this Log log)
        {
            //AddtionsString 字段的规则是 message | stacktrace | [Addtions.Content] | otherForSearch
            List<string> ads = new List<string>() { log.Message, log.StackTrace };
            ads.AddRange(log.Addtions.Select(x => x.Content));
            ads.AddRange(log.Addtions.Select(x => $"{x.ClassName}|||{x.CommandText}|||{x.Database}|||{x.DataSource}|||{x.Ip}|||{x.Message}|||{x.MethodName}|||{x.Parameters}"));
            log.AddtionsString = string.Join(SplitString, ads);
            log.Message = null;
            log.StackTrace = null;
            log.Addtions = log.Addtions.Select(x => new LogAdditionItem()
            {
                Content = null,
                HappenTime = x.HappenTime,
                StatuCode = x.StatuCode,
                ClassName = x.ClassName,
                Parameters = x.Parameters,
                MethodName = x.MethodName,
                Message = x.Message,
                LogType = x.LogType,
                Ip = x.Ip,
                DataSource = x.DataSource,
                CommandText = x.CommandText,
                Database = x.Database
            }).ToArray();
        }
        public static void PrepareLogForRead(this Log log)
        {
            if (string.IsNullOrEmpty(log.AddtionsString))
                return;
            bool allContentNull = log.Addtions.Length > 0 && !log.Addtions.Any(x => x.Content != null);
            if (log.AddtionsString.Contains(SplitString) || allContentNull)
            {
                var contents = log.AddtionsString.Split(new string[] { SplitString }, StringSplitOptions.None);
                log.Message = contents[0];
                log.StackTrace = contents.Length > 1 ? contents[1] : null;
                for (var i = 0; i < log.Addtions.Length; i++)
                {
                    var j = i + 2;
                    if (contents.Length > j)
                        log.Addtions[i].Content = contents[j];
                }
            }
        }
    }
}
