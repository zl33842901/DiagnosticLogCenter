using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using xLiAd.DiagnosticLogCenter.Abstract;

namespace xLiAd.DiagnosticLogCenter.Agent.DiagnosticProcessors
{
    public class AspNetCoreDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public const string TraceIdName = "Trace-Id";
        public const string PageIdName = "Page-Id";
        public const string ParentGuidName = "Guid";
        public const string ParentHttpIdName = "Parent-Http-Id";
        public string ListenerName { get; } = "Microsoft.AspNetCore";

        [DiagnosticName("Microsoft.AspNetCore.Hosting.BeginRequest")]
        public void BeginRequest([Property] HttpContext httpContext)
        {
            bool shouldRecord = FilterRule.ShouldRecord(httpContext.Request.Path);
            if (!shouldRecord)
            {
                GuidHolder.Holder.Value = Guid.Empty;
                return;
            }
            var guid = Guid.NewGuid();
            GuidHolder.Holder.Value = guid;
            SetTraceAndPageId(httpContext);
            var log = ToLog(httpContext);
            log.LogType = LogTypeEnum.RequestBegin;
            Helper.PostHelper.ProcessLog(log);
        }

        private void SetTraceAndPageId(HttpContext httpContext)
        {
            string traceId = null;
            if (httpContext.Request.Headers.ContainsKey(TraceIdName))
                traceId = httpContext.Request.Headers[TraceIdName];
            if (traceId.NullOrEmpty())
                traceId = new TracePageIdValue(DateTime.Now, DiagnosticLogConfig.Config.ClientName, DiagnosticLogConfig.Config.EnvName).ToString();
            GuidHolder.TraceIdHolder.Value = traceId;
            string pageId = null;
            if (httpContext.Request.Headers.ContainsKey(PageIdName))
                pageId = httpContext.Request.Headers[PageIdName];
            if (pageId.NullOrEmpty())
                pageId = traceId;
            GuidHolder.PageIdHolder.Value = pageId;

            string parentGuid = null;
            if (httpContext.Request.Headers.ContainsKey(ParentGuidName))
                parentGuid = httpContext.Request.Headers[ParentGuidName];
            GuidHolder.ParentHolder.Value = parentGuid;

            string parentHttp = null;
            if (httpContext.Request.Headers.ContainsKey(ParentHttpIdName))
                parentHttp = httpContext.Request.Headers[ParentHttpIdName];
            GuidHolder.ParentHttpHolder.Value = parentHttp;
        }

        private LogEntity ToLog(HttpContext httpContext)
        {
            var path = httpContext.Request.Path;
            var ip = httpContext.Connection.RemoteIpAddress.ToString();
            var url = httpContext.Request.GetDisplayUrl();
            var method = httpContext.Request.Method;
            LogEntity log = new LogEntity()
            {
                Message = path,
                StackTrace = url,
                MethodName = method,
                GroupGuid = GuidHolder.Holder.Value.ToString(),
                Ip = ip,
                Level = LogLeveEnum.接口日志,
                HappenTime = DateTime.Now,
                PageId = GuidHolder.PageIdHolder.Value,
                TraceId = GuidHolder.TraceIdHolder.Value,
                ParentGuid = GuidHolder.ParentHolder.Value,
                ParentHttpId = GuidHolder.ParentHttpHolder.Value
            };
            return log;
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.EndRequest")]
        public void EndRequest([Property] HttpContext httpContext)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            var log = ToLog(httpContext);
            log.LogType = LogTypeEnum.RequestEndSuccess;
            Helper.PostHelper.ProcessLog(log);
        }

        [DiagnosticName("Microsoft.AspNetCore.Diagnostics.UnhandledException")]
        public void DiagnosticUnhandledException([Property] HttpContext httpContext, [Property] Exception exception)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            var log = ToLog(httpContext);
            log.LogType = LogTypeEnum.RequestEndException;
            log.Message = exception.Message;
            log.StackTrace = exception.StackTrace;
            Helper.PostHelper.ProcessLog(log);
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.UnhandledException")]
        public void HostingUnhandledException([Property] HttpContext httpContext, [Property] Exception exception)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            var log = ToLog(httpContext);
            log.LogType = LogTypeEnum.RequestEndException;
            log.Message = exception.Message;
            log.StackTrace = exception.StackTrace;
            Helper.PostHelper.ProcessLog(log);
        }

        //[DiagnosticName("Microsoft.AspNetCore.Mvc.BeforeAction")]
        public void BeforeAction([Property] ActionDescriptor actionDescriptor, [Property] HttpContext httpContext)
        {
        }

        //[DiagnosticName("Microsoft.AspNetCore.Mvc.AfterAction")]
        public void AfterAction([Property] ActionDescriptor actionDescriptor, [Property] HttpContext httpContext)
        {
        }
    }
}
