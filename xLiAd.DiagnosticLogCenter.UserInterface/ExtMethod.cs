using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.UserInterface.Models;

namespace xLiAd.DiagnosticLogCenter.UserInterface
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
        public static void PrepareLogForWrite(this Log log)
        {
            log.AddtionsString = string.Join(SplitString, log.Addtions.Select(x => x.Content));
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
                for (var i = 0; i < log.Addtions.Length; i++)
                {
                    if (contents.Length > i)
                        log.Addtions[i].Content = contents[i];
                }
            }
        }
    }
}
