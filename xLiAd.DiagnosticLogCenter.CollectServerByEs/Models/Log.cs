using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.Abstract;

namespace xLiAd.DiagnosticLogCenter.CollectServerByEs.Models
{
    public class Log : EntityBase, ICliEnvDate
    {
        public string EnvironmentName { get; set; }
        public string ClientName { get; set; }
        /// <summary>
        /// 远程时间
        /// </summary>
        public DateTime HappenTime { get; set; }
        public string HappenTimeString => HappenTime.ToString("yyyy-MM-dd HH:mm:ss");
        public string Message { get; set; }
        public string StackTrace { get; set; }
        /// <summary>
        /// 本地时间
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public string CreateTimeString => CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
        public LogLeveEnum Level { get; set; }
        public string LevelString => Level.ToString();

        #region 为接口型日志加的字段
        /// <summary>
        /// 为多触发日志保存内容的字段
        /// </summary>
        public LogAdditionItem[] Addtions { get; set; } = new LogAdditionItem[0];
        /// <summary>
        /// 为上边字段的Content建立的索引，方便前台查询
        /// </summary>
        //[JsonIgnore]
        public string AddtionsString { get; set; }
        /// <summary>
        /// 最后一次写入Addtions 时，他的HappenTime 与本实体的CreateTime 的差。单位毫秒
        /// </summary>
        public int TotalMillionSeconds { get; set; } = -1;
        /// <summary>
        /// 接口是否成功
        /// </summary>
        public bool Success { get; set; }
        #endregion
    }
    /// <summary>
    /// 追加日志
    /// </summary>
    public class LogAdditionItem
    {
        public string Content { get; set; }
        public DateTime HappenTime { get; set; }
        #region 新字段
        public string Message { get; set; }

        public LogTypeEnum LogType { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }

        public string Ip { get; set; }

        public int StatuCode { get; set; }

        #region SQL专用
        public string DataSource { get; set; }
        public string Database { get; set; }
        public string CommandText { get; set; }
        public string Parameters { get; set; }
        #endregion
        #endregion
    }
    /// <summary>
    /// 代表日志结束
    /// </summary>
    public class LogFinishAdditionItem : LogAdditionItem
    {
        /// <summary>
        /// 日志是否以成功结束
        /// </summary>
        public bool Success { get; set; }
    }
}
