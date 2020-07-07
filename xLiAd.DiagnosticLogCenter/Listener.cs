using System;
using System.Diagnostics;
using xLiAd.DiagnosticLogCenter.Abstract;

namespace xLiAd.DiagnosticLogCenter
{
    internal static class Listener
    {
        internal static DiagnosticSource SqlDiagnosticSource = new DiagnosticListener("DiagnosticLogCenterListener");
        internal static readonly string SqlDiagnosticSourceName = "xLiAd.DiagnosticLogCenter.Log";
        internal static bool SqlDiagnosticSourceEnabled;
        static Listener() { SqlDiagnosticSourceEnabled = SqlDiagnosticSource.IsEnabled(SqlDiagnosticSourceName); }
        internal static void Write(LogTypeEnum logType, string className, string methodName, string logContent)
        {
            if (SqlDiagnosticSourceEnabled)
                SqlDiagnosticSource.Write(SqlDiagnosticSourceName, new { LogType = logType, ClassName = className, MethodName = methodName, LogContent = logContent });
        }
    }
}
