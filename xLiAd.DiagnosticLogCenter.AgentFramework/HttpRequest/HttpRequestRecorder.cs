using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace xLiAd.DiagnosticLogCenter.AgentFramework
{
    public static class HttpRequestRecorder
    {
        public static void HttpRequest(HttpRequestMessage request,  Guid requestId)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            if (!GuidHolder.PageIdHolder.Value.NullOrEmpty() && !request.Headers.Contains(AspnetRequestRecorder.PageIdName))
                request.Headers.TryAddWithoutValidation(AspnetRequestRecorder.PageIdName, GuidHolder.PageIdHolder.Value);
            if (!GuidHolder.TraceIdHolder.Value.NullOrEmpty() && !request.Headers.Contains(AspnetRequestRecorder.TraceIdName))
                request.Headers.TryAddWithoutValidation(AspnetRequestRecorder.TraceIdName, GuidHolder.TraceIdHolder.Value);
            if (!request.Headers.Contains(AspnetRequestRecorder.ParentGuidName))
                request.Headers.TryAddWithoutValidation(AspnetRequestRecorder.ParentGuidName, GuidHolder.Holder.Value.ToString());
            if (!request.Headers.Contains(AspnetRequestRecorder.ParentHttpIdName))
                request.Headers.TryAddWithoutValidation(AspnetRequestRecorder.ParentHttpIdName, requestId.ToString());

            var log = ToLog(request, requestId);
            log.LogType = LogTypeEnum.HttpClientRequest;
            PostHelper.ProcessLog(log);
        }

        private static LogEntity ToLog(HttpRequestMessage request, Guid? requestId)
        {
            var uri = request.RequestUri.ToString();
            var host = request.RequestUri.Host;
            var method = request.Method.ToString();
            var content = request?.Content?.ReadAsStringAsync()?.Result;
            LogEntity log = new LogEntity()
            {
                Message = uri,
                StackTrace = content,
                MethodName = method,
                Ip = host,
                GroupGuid = GuidHolder.Holder.Value.ToString(),
                HappenTime = DateTime.Now,
                PageId = GuidHolder.PageIdHolder.Value,
                TraceId = GuidHolder.TraceIdHolder.Value,
                ParentGuid = GuidHolder.ParentHolder.Value,
                HttpId = requestId?.ToString(),
                ParentHttpId = GuidHolder.ParentHttpHolder.Value
            };
            return log;
        }

        public static void HttpResponse(HttpResponseMessage response, Guid requestId)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            int statuCode = (int?)response?.StatusCode ?? 0;
            var content = (DiagnosticLogConfig.Config?.RecordHttpClientBody ?? false) ? response?.Content?.ReadAsStringAsync()?.Result : string.Empty;
            LogEntity log = new LogEntity()
            {
                StatuCode = statuCode,
                StackTrace = content,
                GroupGuid = GuidHolder.Holder.Value.ToString(),
                LogType = LogTypeEnum.HttpClientResponse,
                HappenTime = DateTime.Now,
                PageId = GuidHolder.PageIdHolder.Value,
                TraceId = GuidHolder.TraceIdHolder.Value,
                ParentGuid = GuidHolder.ParentHolder.Value,
                HttpId = requestId.ToString(),
                ParentHttpId = GuidHolder.ParentHttpHolder.Value
            };
            PostHelper.ProcessLog(log);
        }

        public static void HttpException(HttpRequestMessage request, Exception exception, Guid? requestId)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            var log = ToLog(request, requestId);
            log.LogType = LogTypeEnum.HttpClientException;
            log.StackTrace = exception.StackTrace;
            log.Message = exception.Message;
            log.PageId = GuidHolder.PageIdHolder.Value;
            log.TraceId = GuidHolder.TraceIdHolder.Value;
            log.ParentGuid = GuidHolder.ParentHolder.Value;
            log.ParentHttpId = GuidHolder.ParentHttpHolder.Value;
            PostHelper.ProcessLog(log);
        }
    }
}
