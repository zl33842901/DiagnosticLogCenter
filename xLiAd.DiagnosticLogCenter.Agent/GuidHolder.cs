using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace xLiAd.DiagnosticLogCenter.Agent
{
    public static class GuidHolder
    {
        /// <summary>
        /// 一次请求内，只有本地使用
        /// </summary>
        public static AsyncLocal<Guid> Holder = new AsyncLocal<Guid>();

        /// <summary>
        /// 子请求
        /// </summary>
        public static AsyncLocal<string> ParentHolder = new AsyncLocal<string>();

        /// <summary>
        /// 子请求HTTP
        /// </summary>
        public static AsyncLocal<string> ParentHttpHolder = new AsyncLocal<string>();

        /// <summary>
        /// 一次请求内，所有站点都一致
        /// </summary>
        public static AsyncLocal<string> TraceIdHolder = new AsyncLocal<string>();

        /// <summary>
        /// 一个页面内，所有站点都一致；如果后端生成，则和 TraceId 一致。
        /// </summary>
        public static AsyncLocal<string> PageIdHolder = new AsyncLocal<string>();
    }
}
