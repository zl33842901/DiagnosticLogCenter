using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.Abstract
{
    public class LogEntity
    {
        public string EnvironmentName { get; set; }
        public string ClientName { get; set; }
        public LogTypeEnum LogType { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        /// <summary>
        /// 远程时间
        /// </summary>
        public DateTime HappenTime { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public LogLeveEnum Level { get; set; }
        /// <summary>
        /// 接口是否成功
        /// </summary>
        public bool Success { get; set; }
        public string GroupGuid { get; set; }

        #region SQL专用
        public string DataSource { get; set; }
        public string Database { get; set; }
        public string CommandText { get; set; }
        public string Parameters { get; set; }
        #endregion
    }
}
