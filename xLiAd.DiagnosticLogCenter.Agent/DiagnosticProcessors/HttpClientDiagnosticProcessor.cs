using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.Agent.DiagnosticProcessors
{
    public class HttpClientDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName { get; } = "HttpHandlerDiagnosticListener";
        [DiagnosticName("System.Net.Http.Request")]
        public void HttpRequest([Property(Name = "Request")] HttpRequestMessage request)
        {
            var uri = request.RequestUri.ToString();
            var host = request.RequestUri.Host;
            var method = request.Method.ToString();
            var content = request?.Content?.ReadAsStringAsync()?.Result;
        }

        [DiagnosticName("System.Net.Http.Response")]
        public void HttpResponse([Property(Name = "Response")] HttpResponseMessage response)
        {
            var statuCode = response.StatusCode;
            var content = response.Content?.ReadAsStringAsync()?.Result;
        }

        [DiagnosticName("System.Net.Http.Exception")]
        public void HttpException([Property(Name = "Request")] HttpRequestMessage request,
            [Property(Name = "Exception")] Exception exception)
        {
            
        }
    }
}
