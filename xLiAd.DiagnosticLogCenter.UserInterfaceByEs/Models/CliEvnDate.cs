using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.UserInterfaceByEs.Models
{
    public class CliEvnDate : ICliEnvDate
    {
        public string EnvironmentName { get; set; }
        public string ClientName { get; set; }
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime HappenTime { get; set; }
        public bool Exist { get; set; }

        public string LookUrl => this.GetUrl();
    }
}
