using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.Agent
{
    public delegate void AspNetCoreRequest(Guid guid, HttpContext httpContext);
    public delegate void AspNetCoreRequestException(Guid guid, HttpContext httpContext, Exception exception);
    public class DiagnosticLogConfig
    {
        internal static DiagnosticLogConfig Config { get; set; }
        /// <summary>
        /// 总开关
        /// </summary>
        public bool Enable { get; set; } = true;
        public bool EnableAspNetCore { get; set; } = true;
        public bool EnableDapperEx { get; set; } = true;
        public bool EnableHttpClient { get; set; } = true;
        public bool EnableSqlClient { get; set; } = true;
        public bool EnableMethod { get; set; } = true;
        public bool EnableSystemLog { get; set; } = true;
        /// <summary>
        /// 是否记录SQL的参数
        /// </summary>
        public bool RecordSqlParameters { get; set; } = true;
        /// <summary>
        /// 是否记录 HttpClient 的返回结果
        /// </summary>
        public bool RecordHttpClientBody { get; set; } = true;
        /// <summary>
        /// 如果这个属性设置了，那么只允许这个属性允许的请求
        /// </summary>
        public IEnumerable<string> AllowPath { get; set; } = null;
        /// <summary>
        /// 如果 AllowPath 没有设置，那么阻止这个属性设置的请求
        /// </summary>
        public IEnumerable<string> ForbbidenPath { get; set; } = null;

        public string CollectServerAddress { get; set; }
        public string ClientName { get; set; } = "Sample";
        public string EnvName { get; set; } = "PRD";
        public int TimeoutBySecond { get; set; } = 5;

        public event AspNetCoreRequest OnAspNetCoreBeginRequest;
        public event AspNetCoreRequest OnAspNetCoreEndRequest;
        public event AspNetCoreRequestException OnAspNetCoreException;

        internal void CallAspNetCoreBeginRequest(Guid guid, HttpContext httpContext) => OnAspNetCoreBeginRequest?.Invoke(guid, httpContext);
        internal void CallAspNetCoreEndRequest(Guid guid, HttpContext httpContext) => OnAspNetCoreEndRequest?.Invoke(guid, httpContext);
        internal void CallAspNetCoreException(Guid guid, HttpContext httpContext, Exception exception) => OnAspNetCoreException?.Invoke(guid, httpContext, exception);
    }
}
