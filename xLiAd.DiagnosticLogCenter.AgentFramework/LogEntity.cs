using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.AgentFramework
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

        public string TraceId { get; set; }
        public string PageId { get; set; }
        public string ParentGuid { get; set; }
        /// <summary>
        /// 最先用在 httpClient里，所以就叫这个名了。其实应该叫 SegmentId
        /// </summary>
        public string HttpId { get; set; }
        public string ParentHttpId { get; set; }
    }

    public enum LogLeveEnum
    {
        Trace = 0,
        Debug = 1,
        Information = 2,
        Warning = 3,
        Error = 4,
        Critical = 5,
        接口日志 = 10
    }
    public enum LogTypeEnum : byte
    {
        [Description("进入方法")]
        MethodEntry = 1,
        [Description("手动日志")]
        MethodAddition = 2,
        [Description("方法完毕")]
        MethodLeave = 3,
        [Description("方法报错")]
        MethodException = 4,

        [Description("Sql开始")]
        SqlBefore = 11,
        [Description("Sql完毕")]
        SqlAfter = 12,
        [Description("Sql报错")]
        SqlException = 13,

        [Description("请求开始")]
        RequestBegin = 21,
        [Description("请求完毕")]
        RequestEndSuccess = 22,
        [Description("请求报错")]
        RequestEndException = 23,

        [Description("请求外部")]
        HttpClientRequest = 31,
        [Description("外部成功返回")]
        HttpClientResponse = 32,
        [Description("外部报错")]
        HttpClientException = 33,

        [Description("执行Sql")]
        DapperExSqlBefore = 41,
        [Description("执行Sql完成")]
        DapperExSqlAfter = 42,
        [Description("执行Sql报错")]
        DapperExSqlException = 43
    }
}
