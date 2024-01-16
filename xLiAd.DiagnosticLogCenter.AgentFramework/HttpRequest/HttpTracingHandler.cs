using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace xLiAd.DiagnosticLogCenter.AgentFramework.HttpRequest
{
    public class HttpTracingHandler : DelegatingHandler
    {
        /// <summary>
        /// If you are passing this delegating handler to <see cref="HttpClientFactory.Create"/> method, please pass a `null` in constructor as innerHandler.
        /// if you are passing it in <see cref="HttpClient"/> constructor, just use it's non-parameter constructor.
        /// as follow:
        /// <code>var httpClient = HttpClientFactory.Create(new HttpTracingHandler(null))</code>
        /// or
        /// <code>var httpClient = new HttpClient(new HttpTracingHandler())</code>
        /// </summary>
        public HttpTracingHandler()
            : this(new HttpClientHandler())
        {
        }

        /// <summary>
        /// If you are passing this delegating handler to <see cref="HttpClientFactory.Create"/> method, please pass a `null` in constructor as innerHandler.
        /// if you are passing it in <see cref="HttpClient"/> constructor, just use it's non-parameter constructor.
        /// as follow:
        /// <code>var httpClient = HttpClientFactory.Create(new HttpTracingHandler(null))</code>
        /// or
        /// <code>var httpClient = new HttpClient(new HttpTracingHandler())</code>
        /// </summary>
        public HttpTracingHandler(HttpMessageHandler innerHandler)
        {
            if (innerHandler != null)
            {
                InnerHandler = innerHandler;
            }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var guid = Guid.NewGuid();
            HttpRequestRecorder.HttpRequest(request, guid);
            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                HttpRequestRecorder.HttpResponse(response, guid);
                return response;
            }
            catch (Exception exception)
            {
                HttpRequestRecorder.HttpException(request, exception, guid);
                throw;
            }
        }
    }
}
