using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.CollectServerBoth.TraceAndPage;

namespace xLiAd.DiagnosticLogCenter.CollectServerBoth
{
    public static partial class ExtMethodBoth
    {
        private static string ThirdPart(DateTime happenTime)
        {
            string result;
            if (happenTime > new DateTime(2021, 8, 26, 13, 59, 00))
                result = "-" + (happenTime.Day < 11 ? "0" : (happenTime.Day < 21 ? "1" : "2"));
            else
                result = string.Empty;
            return result;
        }
        public static string GetTraceIdTableName(this DateTime happenTime)
        {
            return $"TraceId-{happenTime.Year}-{happenTime.Month.ToString().PadLeft(2, '0')}{ThirdPart(happenTime)}";
        }
        public static string GetPageIdTableName(this DateTime happenTime)
        {
            return $"PageId-{happenTime.Year}-{happenTime.Month.ToString().PadLeft(2, '0')}{ThirdPart(happenTime)}";
        }
    }
}
