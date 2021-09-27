using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.Abstract;

namespace xLiAd.DiagnosticLogCenter.CollectServerBoth
{
    public static partial class ExtMethodBoth
    {
        /// <summary>
        /// 检查一个log 是否是一个追踪的起点
        /// </summary>
        /// <param name="tracePageIdValue"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static bool IsFirst(this TracePageIdValue tracePageIdValue, CollectServer.Models.Log log)
        {
            return tracePageIdValue.ClientName == log.ClientName && tracePageIdValue.EnvName == log.EnvironmentName
                && tracePageIdValue.HappenTime.Date == log.HappenTime.Date;
        }

        public static bool IsFirst(this TracePageIdValue tracePageIdValue, TracePageIdValue other)
        {
            return tracePageIdValue.ClientName == other.ClientName && tracePageIdValue.EnvName == other.EnvName
                && tracePageIdValue.HappenTime.Date == other.HappenTime.Date;
        }
    }
}
