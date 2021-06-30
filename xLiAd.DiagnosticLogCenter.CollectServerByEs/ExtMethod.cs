using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.CollectServerByEs.Models;

namespace xLiAd.DiagnosticLogCenter.CollectServerByEs
{
    public static class ExtMethod
    {
        private const string SplitString = ";-^45#24Suehwi7&58$%;";
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

        }
        public static void PrepareLogForRead(this Log log)
        {

        }
    }
}
