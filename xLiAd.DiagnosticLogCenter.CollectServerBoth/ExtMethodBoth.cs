using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.CollectServerBoth.TraceAndPage;

namespace xLiAd.DiagnosticLogCenter.CollectServerBoth
{
    public static partial class ExtMethodBoth
    {
        public static string GetIndexName(this TraceGroup traceGroup)
        {
            return $"TraceId-{traceGroup.Year}-{traceGroup.Month.ToString().PadLeft(2, '0')}";
        }

        public static string GetTraceIdTableName(this DateTime happenTime)
        {
            return $"TraceId-{happenTime.Year}-{happenTime.Month.ToString().PadLeft(2, '0')}";
        }
        public static string GetPageIdTableName(this DateTime happenTime)
        {
            return $"PageId-{happenTime.Year}-{happenTime.Month.ToString().PadLeft(2, '0')}";
        }
    }
}
