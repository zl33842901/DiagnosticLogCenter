using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace xLiAd.DiagnosticLogCenter.AgentFramework
{
    public static class AspnetRequestRecorder
    {
        public const string TraceIdName = "Trace-Id";
        public const string PageIdName = "Page-Id";
        public const string ParentGuidName = "Guid";
        public const string ParentHttpIdName = "Parent-Http-Id";
        public static void OnBeginRequest(object sender, EventArgs e)
        {
            var httpApplication = sender as HttpApplication;
            var httpContext = httpApplication.Context;
            bool shouldRecord = FilterRule.ShouldRecord(httpContext.Request.Path);
            if (!shouldRecord)
            {
                GuidHolder.Holder.Value = Guid.Empty;
                return;
            }
            var guid = Guid.NewGuid();
            GuidHolder.Holder.Value = guid;
            SetTraceAndPageId(httpContext);
            httpContext.Response.Headers.Add("DLC-Guids", $"{guid}|{GuidHolder.ParentHolder.Value}|{GuidHolder.ParentHttpHolder.Value}|{GuidHolder.TraceIdHolder.Value}|{GuidHolder.PageIdHolder.Value}");
            var log = ToLog(httpContext, true);
            log.LogType = LogTypeEnum.RequestBegin;
            PostHelper.ProcessLog(log);
            DiagnosticLogConfig.Config.CallAspNetCoreBeginRequest(GuidHolder.Holder.Value, httpContext);
        }
        private static void SetTraceAndPageId(HttpContext httpContext)
        {
            string traceId = null;
            if (httpContext.Request.Headers.AllKeys.Contains(TraceIdName))
                traceId = httpContext.Request.Headers[TraceIdName];
            if (traceId.NullOrEmpty())
                traceId = new TracePageIdValue(DateTime.Now, DiagnosticLogConfig.Config.ClientName, DiagnosticLogConfig.Config.EnvName).ToString();
            GuidHolder.TraceIdHolder.Value = traceId;
            string pageId = null;
            if (httpContext.Request.Headers.AllKeys.Contains(PageIdName))
                pageId = httpContext.Request.Headers[PageIdName];
            if (pageId.NullOrEmpty())
                pageId = traceId;
            GuidHolder.PageIdHolder.Value = pageId;

            string parentGuid = null;
            if (httpContext.Request.Headers.AllKeys.Contains(ParentGuidName))
                parentGuid = httpContext.Request.Headers[ParentGuidName];
            GuidHolder.ParentHolder.Value = parentGuid;

            string parentHttp = null;
            if (httpContext.Request.Headers.AllKeys.Contains(ParentHttpIdName))
                parentHttp = httpContext.Request.Headers[ParentHttpIdName];
            GuidHolder.ParentHttpHolder.Value = parentHttp;
        }

        private static LogEntity ToLog(HttpContext httpContext, bool isStart = false)
        {
            var path = httpContext.Request.Path;
            var ip = httpContext.Request.UserHostAddress.ToString();
            var url = httpContext.Request.Url.ToString();//.GetDisplayUrl();
            var method = httpContext.Request.RequestType;//.Method;
            string stackTrace;
            if (isStart)
                stackTrace = $"Url：{url}\r\nIP：{httpContext.Request.UserHostAddress.ToString()}\r\nLocalIP：{httpContext.Request.ServerVariables.ToString()}\r\nHeaders：\r\n{string.Join("\r\n", ConvertToString(httpContext.Request.Headers))}";
            else
            {
                //var u = GetUser(httpContext);
                stackTrace = GetBackInfo(httpContext);
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
        private static string GetBackInfo(HttpContext httpContext)
        {
            var headers = string.Join("\r\n", ConvertToString(httpContext.Response.Headers));
            return $"StatusCode:{httpContext.Response.StatusCode}\r\nHeaders:\r\n{headers}";
        }

        public static void LoadGuidsFromResponse(HttpContext httpContext)
        {
            if (httpContext != null && httpContext.Response != null && !httpContext.Response.Headers["DLC-Guids"].NullOrEmpty())
            {
                var ss = httpContext.Response.Headers["DLC-Guids"].Split('|');
                if(ss.Length == 5)
                {
                    GuidHolder.Holder.Value = Guid.Parse(ss[0]);
                    GuidHolder.ParentHolder.Value = ss[1];
                    GuidHolder.ParentHttpHolder.Value = ss[2];
                    GuidHolder.TraceIdHolder.Value = ss[3];
                    GuidHolder.PageIdHolder.Value = ss[4];
                }
            }
        }

        public static void OnEndRequest(object sender, EventArgs e)
        {
            var httpApplication = sender as HttpApplication;
            var httpContext = httpApplication.Context;
            if (GuidHolder.Holder.Value == Guid.Empty)
                LoadGuidsFromResponse(httpContext);
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            var log = ToLog(httpContext);
            log.LogType = LogTypeEnum.RequestEndSuccess;
            PostHelper.ProcessLog(log);
            DiagnosticLogConfig.Config.CallAspNetCoreEndRequest(GuidHolder.Holder.Value, httpContext);
        }

        public static IEnumerable<string> ConvertToString(NameValueCollection nameValueCollection)
        {
            //.Select(x => $"{x.Key}:{x.Value.ToString()}")
            List<string> strings = new List<string>();
            foreach(var item in nameValueCollection.AllKeys)
            {
                strings.Add($"{item}:{nameValueCollection[item]}");
            }
            return strings;
        }
    }
}
