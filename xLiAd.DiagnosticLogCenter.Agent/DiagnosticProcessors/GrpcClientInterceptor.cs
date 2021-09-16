using Grpc.Core;
using Grpc.Core.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using xLiAd.DiagnosticLogCenter.Abstract;

namespace xLiAd.DiagnosticLogCenter.Agent.DiagnosticProcessors
{
    /// <summary>
    /// 这两个类其实用不到，因为 Grpc 本身的客户端请求也是基于 HttpClient 服务端也基于 AspNetCore，不用这两个类也能形成链路。
    /// </summary>
    public class GrpcClientInterceptor : Interceptor
    {
        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            (var metadata, var requestId) = BeginRequest(context);
            try
            {
                var options = context.Options.WithHeaders(metadata);
                context = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);
                var response = continuation(request, context);
                EndRequest(requestId.ToString());
                return response;
            }
            catch (Exception ex)
            {
                DiagnosticUnhandledException(ex, requestId.ToString(), context.Method.FullName, context.Host);
                throw ex;
            }
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            (var metadata, var requestId) = BeginRequest(context);
            try
            {
                var options = context.Options.WithHeaders(metadata);
                context = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, options);
                var response = continuation(request, context);
                var responseAsync = response.ResponseAsync.ContinueWith(r =>
                {
                    try
                    {
                        EndRequest(requestId.ToString());
                        return r.Result;
                    }
                    catch (Exception ex)
                    {
                        DiagnosticUnhandledException(ex, requestId.ToString(), context.Method.FullName, context.Host);
                        throw ex;
                    }
                });
                return new AsyncUnaryCall<TResponse>(responseAsync, response.ResponseHeadersAsync, response.GetStatus, response.GetTrailers, response.Dispose);
            }
            catch (Exception ex)
            {
                DiagnosticUnhandledException(ex, requestId.ToString(), context.Method.FullName, context.Host);
                throw ex;
            }
        }

        public (Metadata, Guid) BeginRequest<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> grpcContext)
            where TRequest : class
            where TResponse : class
        {
            var headers = grpcContext.Options.Headers;
            if (GuidHolder.Holder.Value == Guid.Empty)
                return (headers,Guid.Empty);
            var requestId = Guid.NewGuid();
            var headerValues = headers.Select(m => new KeyValuePair<string, string>(m.Key, m.Value)).ToList();
            if (!GuidHolder.PageIdHolder.Value.NullOrEmpty())
                headerValues.Add(new KeyValuePair<string, string>(AspNetCoreDiagnosticProcessor.PageIdName, GuidHolder.PageIdHolder.Value));
            if (!GuidHolder.TraceIdHolder.Value.NullOrEmpty())
                headerValues.Add(new KeyValuePair<string, string>(AspNetCoreDiagnosticProcessor.TraceIdName, GuidHolder.TraceIdHolder.Value));

            headerValues.Add(new KeyValuePair<string, string>(AspNetCoreDiagnosticProcessor.ParentGuidName, GuidHolder.Holder.Value.ToString()));
            headerValues.Add(new KeyValuePair<string, string>(AspNetCoreDiagnosticProcessor.ParentHttpIdName, requestId.ToString()));

            var log = ToLog(grpcContext, requestId);
            log.LogType = LogTypeEnum.HttpClientRequest;
            Helper.PostHelper.ProcessLog(log);

            var metadata = new Metadata();
            foreach (var item in headerValues)
            {
                metadata.Add(item.Key, item.Value);
            }
            return (metadata, requestId);
        }
        private LogEntity ToLog<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> grpcContext, Guid requestId)
            where TRequest : class
            where TResponse : class
        {
            var uri = grpcContext.Method.ServiceName;
            var host = grpcContext.Host;
            var method = grpcContext.Method.FullName;
            string content = null;
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
                HttpId = requestId.ToString(),
                ParentHttpId = GuidHolder.ParentHttpHolder.Value
            };
            return log;
        }

        public void EndRequest(string requestId)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            var statuCode = 200;
            var content = string.Empty;
            LogEntity log = new LogEntity()
            {
                StatuCode = (int)statuCode,
                StackTrace = content,
                GroupGuid = GuidHolder.Holder.Value.ToString(),
                LogType = LogTypeEnum.HttpClientResponse,
                HappenTime = DateTime.Now,
                PageId = GuidHolder.PageIdHolder.Value,
                TraceId = GuidHolder.TraceIdHolder.Value,
                ParentGuid = GuidHolder.ParentHolder.Value,
                HttpId = requestId,
                ParentHttpId = GuidHolder.ParentHttpHolder.Value
            };
            Helper.PostHelper.ProcessLog(log);
        }

        public void DiagnosticUnhandledException(Exception exception, string requestId, string method, string host)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            LogEntity log = new LogEntity()
            {
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                MethodName = method,
                Ip = host,
                GroupGuid = GuidHolder.Holder.Value.ToString(),
                HappenTime = DateTime.Now,
                PageId = GuidHolder.PageIdHolder.Value,
                TraceId = GuidHolder.TraceIdHolder.Value,
                ParentGuid = GuidHolder.ParentHolder.Value,
                ParentHttpId = GuidHolder.ParentHttpHolder.Value,
                HttpId = requestId
            };
            log.LogType = LogTypeEnum.HttpClientException;
            Helper.PostHelper.ProcessLog(log);
        }
    }
}
