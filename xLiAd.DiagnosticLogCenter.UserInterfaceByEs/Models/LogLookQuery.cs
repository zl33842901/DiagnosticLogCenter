using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.UserInterfaceByEs.Models
{
    public class LogLookQuery : ICliEnvDate
    {
        public string ClientName { get; set; }
        public string EnvironmentName { get; set; }
        public DateTime HappenTime { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string Key { get; set; }
        /// <summary>
        /// 级别 0 到 6 或为空
        /// </summary>
        public string Level { get; set; }
        /// <summary>
        /// 是否成功  1 2 或空
        /// </summary>
        public string Success { get; set; }
        /// <summary>
        /// 执行时间  0-500 这样
        /// </summary>
        public string MSec { get; set; }

        public DateTime[] HappenTimeRegion { get; set; }

        /// <summary>
        /// 查询模式  1 wildcard    2 match
        /// </summary>
        public int QueryMode { get; set; }
    }
}
