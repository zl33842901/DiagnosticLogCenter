using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.Analyzer
{
    public class StatisticsResultModel
    {
        public string ClientName { get; set; }
        public string EnvironmentName { get; set; }
        /// <summary>
        /// 统计开始时间
        /// </summary>
        public DateTime Start { get; set; }
        /// <summary>
        /// 统计结束时间
        /// </summary>
        public DateTime End { get; set; }
        /// <summary>
        /// 接口  统计全部结果使用 *
        /// </summary>
        public string Interface { get; set; }
        /// <summary>
        /// 总访问数量
        /// </summary>
        public int Total { get; set; }
        /// <summary>
        /// 访问失败数量
        /// </summary>
        public int FailCount { get; set; }
        /// <summary>
        /// 最长耗时
        /// </summary>
        public int MaxTotalMillionSeconds { get; set; }
        /// <summary>
        /// 最短耗时
        /// </summary>
        public int MinTotalMillionSeconds { get; set; }
        /// <summary>
        /// 平均耗时
        /// </summary>
        public double AvgTotalMillionSeconds { get; set; }
    }
}
