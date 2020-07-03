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
        SqlException = 13
    }
}
