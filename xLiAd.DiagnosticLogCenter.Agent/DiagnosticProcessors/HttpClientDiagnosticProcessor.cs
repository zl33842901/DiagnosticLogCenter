using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using xLiAd.DiagnosticLogCenter.Abstract;

namespace xLiAd.DiagnosticLogCenter.Agent.DiagnosticProcessors
{
    public class HttpClientDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName { get; } = "HttpHandlerDiagnosticListener";
        [DiagnosticName("System.Net.Http.Request")]
        public void HttpRequest([Property(Name = "Request")] HttpRequestMessage request, [Property(Name = "LoggingRequestId")] Guid requestId)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            if(!GuidHolder.PageIdHolder.Value.NullOrEmpty() && !request.Headers.Contains(AspNetCoreDiagnosticProcessor.PageIdName))
                request.Headers.TryAddWithoutValidation(AspNetCoreDiagnosticProcessor.PageIdName, GuidHolder.PageIdHolder.Value);
            if (!GuidHolder.TraceIdHolder.Value.NullOrEmpty() && !request.Headers.Contains(AspNetCoreDiagnosticProcessor.TraceIdName))
                request.Headers.TryAddWithoutValidation(AspNetCoreDiagnosticProcessor.TraceIdName, GuidHolder.TraceIdHolder.Value);
            if (!request.Headers.Contains(AspNetCoreDiagnosticProcessor.ParentGuidName))
                request.Headers.TryAddWithoutValidation(AspNetCoreDiagnosticProcessor.ParentGuidName, GuidHolder.Holder.Value.ToString());
            if (!request.Headers.Contains(AspNetCoreDiagnosticProcessor.ParentHttpIdName))
                request.Headers.TryAddWithoutValidation(AspNetCoreDiagnosticProcessor.ParentHttpIdName, requestId.ToString());

            var log = ToLog(request, requestId);
            log.LogType = LogTypeEnum.HttpClientRequest;
            Helper.PostHelper.ProcessLog(log);
        }

        private LogEntity ToLog(HttpRequestMessage request, Guid? requestId)
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

        [DiagnosticName("System.Net.Http.Response")]
        public void HttpResponse([Property(Name = "Response")] HttpResponseMessage response, [Property(Name = "LoggingRequestId")] Guid requestId)
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
            Helper.PostHelper.ProcessLog(log);
        }

        [DiagnosticName("System.Net.Http.Exception")]
        public void HttpException([Property(Name = "Request")] HttpRequestMessage request,
            [Property(Name = "Exception")] Exception exception, [Property(Name = "LoggingRequestId")] Guid? requestId)
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
            Helper.PostHelper.ProcessLog(log);
        }
    }
}
