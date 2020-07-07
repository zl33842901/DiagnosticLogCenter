using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.Abstract
{
    public class LogEntity
    {
        public string EnvironmentName { get; set; }
        public string ClientName { get; set; }
        /// <summary>
        /// 远程时间
        /// </summary>
        public DateTime HappenTime { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public LogLeveEnum Level { get; set; }
        public string GroupGuid { get; set; }

        //下面是新的

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
    }
}
