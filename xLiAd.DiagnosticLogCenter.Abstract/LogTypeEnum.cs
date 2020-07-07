using System;

namespace xLiAd.DiagnosticLogCenter.Abstract
{
    public enum LogTypeEnum : byte
    {
        MethodEntry = 1,
        MethodAddition = 2,
        MethodLeave = 3,
        MethodException = 4,

        SqlBefore = 11,
        SqlAfter = 12,
        SqlException = 13,

        RequestBegin = 21,
        RequestEndSuccess = 22,
        RequestEndException = 23,

        HttpClientRequest = 31,
        HttpClientResponse = 32,
        HttpClientException = 33,

        DapperExSqlBefore = 41
    }
}
