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
        public void HttpRequest([Property(Name = "Request")] HttpRequestMessage request)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            var log = ToLog(request);
            log.LogType = LogTypeEnum.HttpClientRequest;
            Helper.PostHelper.ProcessLog(log);
        }

        private LogEntity ToLog(HttpRequestMessage request)
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
                HappenTime = DateTime.Now
            };
            return log;
        }

        [DiagnosticName("System.Net.Http.Response")]
        public void HttpResponse([Property(Name = "Response")] HttpResponseMessage response)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            var statuCode = response.StatusCode;
            var content = response.Content?.ReadAsStringAsync()?.Result;
            LogEntity log = new LogEntity()
            {
                StatuCode = (int)statuCode,
                StackTrace = content,
                GroupGuid = GuidHolder.Holder.Value.ToString(),
                HappenTime = DateTime.Now
            };
            Helper.PostHelper.ProcessLog(log);
        }

        [DiagnosticName("System.Net.Http.Exception")]
        public void HttpException([Property(Name = "Request")] HttpRequestMessage request,
            [Property(Name = "Exception")] Exception exception)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            var log = ToLog(request);
            log.LogType = LogTypeEnum.HttpClientException;
            log.StackTrace = exception.StackTrace;
            log.Message = exception.Message;
            Helper.PostHelper.ProcessLog(log);
        }
    }
}
