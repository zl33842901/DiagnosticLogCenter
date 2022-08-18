using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
            var log = ToLog(httpContext, true);
            log.LogType = LogTypeEnum.RequestBegin;
            Helper.PostHelper.ProcessLog(log);
        }

        private void SetTraceAndPageId(HttpContext httpContext)
        {
            string traceId = null;
            if (httpContext.Request.Headers.ContainsKey(TraceIdName))
                traceId = SubstringToDot(httpContext.Request.Headers[TraceIdName]);
            if (traceId.NullOrEmpty())
                traceId = new TracePageIdValue(DateTime.Now, DiagnosticLogConfig.Config.ClientName, DiagnosticLogConfig.Config.EnvName).ToString();
            GuidHolder.TraceIdHolder.Value = traceId;
            string pageId = null;
            if (httpContext.Request.Headers.ContainsKey(PageIdName))
                pageId = SubstringToDot(httpContext.Request.Headers[PageIdName]);
            if (pageId.NullOrEmpty())
                pageId = traceId;
            GuidHolder.PageIdHolder.Value = pageId;

            string parentGuid = null;
            if (httpContext.Request.Headers.ContainsKey(ParentGuidName))
                parentGuid = SubstringToDot(httpContext.Request.Headers[ParentGuidName]);
            GuidHolder.ParentHolder.Value = parentGuid;

            string parentHttp = null;
            if (httpContext.Request.Headers.ContainsKey(ParentHttpIdName))
                parentHttp = SubstringToDot(httpContext.Request.Headers[ParentHttpIdName]);
            GuidHolder.ParentHttpHolder.Value = parentHttp;
        }

        private string SubstringToDot(IEnumerable<string> header)
        {
            var result = header.FirstOrDefault();
            if (result == null)
                return null;
            if (result.Contains(','))
                return result.Substring(0, result.IndexOf(','));
            else
                return result;
        }

        private LogEntity ToLog(HttpContext httpContext, bool isStart = false)
        {
            var path = httpContext.Request.Path;
            var ip = httpContext.Connection.RemoteIpAddress.ToString();
            var url = httpContext.Request.GetDisplayUrl();
            var method = httpContext.Request.Method;
            string stackTrace;
            if (isStart)
                stackTrace = $"Url：{url}\r\nIP：{httpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()}:{httpContext.Connection.RemotePort}\r\nLocalIP：{httpContext.Connection.LocalIpAddress.MapToIPv4().ToString()}:{httpContext.Connection.LocalPort}\r\nHeaders：\r\n{string.Join("\r\n", httpContext.Request.Headers.Select(x => $"{x.Key}:{x.Value.ToString()}"))}";
            else
            {
                var u = GetUser(httpContext);
                stackTrace = u + (u.NullOrEmpty() ? "" : "\r\n") + GetBackInfo(httpContext);
            }
            LogEntity log = new LogEntity()
            {
                Message = path,
                StackTrace = stackTrace,
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

        private string GetBackInfo(HttpContext httpContext)
        {
            var headers = string.Join("\r\n", httpContext.Response.Headers.Select(x => $"{x.Key}:{x.Value.ToString()}"));
            return $"StatusCode:{httpContext.Response.StatusCode}\r\nHeaders:\r\n{headers}";
        }

        private string GetUser(HttpContext httpContext)
        {
            string id = null, cname = null, domainAccount = null;
            var u = httpContext.User;
            if (u != null && u.Identity.IsAuthenticated)
            {
                IEnumerable<Claim> claimList = u.Claims;
                var claimUserId = claimList.FirstOrDefault(y => y.Type == "Id");
                if (claimUserId != null && !string.IsNullOrEmpty(claimUserId.Value))
                    id = claimUserId.Value;

                var claimUserCode = claimList.FirstOrDefault(y => y.Type == "CName");
                if (claimUserCode != null && !string.IsNullOrEmpty(claimUserCode.Value))
                    cname = claimUserCode.Value;

                var claimUserName = claimList.FirstOrDefault(y => y.Type == "DomainAccount");
                if (claimUserName != null && !string.IsNullOrEmpty(claimUserName.Value))
                    domainAccount = claimUserName.Value;
            }
            if (id == null && cname == null && domainAccount == null)
                return string.Empty;
            else
                return $"用户Id：{id}\r\n用户姓名：{cname}\r\n用户域帐号：{domainAccount}\r\n";
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
