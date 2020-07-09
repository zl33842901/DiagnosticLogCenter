using System;
using System.ComponentModel;

namespace xLiAd.DiagnosticLogCenter.Abstract
{
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
        DapperExSqlBefore = 41
    }
}
