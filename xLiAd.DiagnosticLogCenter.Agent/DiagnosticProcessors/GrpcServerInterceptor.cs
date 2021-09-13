using Grpc.Core;
using Grpc.Core.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.Abstract;

namespace xLiAd.DiagnosticLogCenter.Agent.DiagnosticProcessors
{
    public class GrpcServerInterceptor : Interceptor
    {
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            BeginRequest(context);
            try
            {
                var response = await continuation(request, context);
                EndRequest(context);
                return response;
            }
            catch (Exception ex)
            {
                DiagnosticUnhandledException(context, ex);
                throw ex;
            }
        }

        public void BeginRequest(ServerCallContext grpcContext)
        {
            bool shouldRecord = FilterRule.ShouldRecord(grpcContext.Method);
            if (!shouldRecord)
            {
                GuidHolder.Holder.Value = Guid.Empty;
                return;
            }
            var guid = Guid.NewGuid();
            GuidHolder.Holder.Value = guid;
            SetTraceAndPageId(grpcContext);
            var log = ToLog(grpcContext);
            log.LogType = LogTypeEnum.RequestBegin;
            Helper.PostHelper.ProcessLog(log);
        }

        private void SetTraceAndPageId(ServerCallContext grpcContext)
        {
            string traceId = null;
            var headers = grpcContext.RequestHeaders.Select(m => new KeyValuePair<string, string>(m.Key, m.Value)).ToDictionary(x => x.Key, x => x.Value);
            if (headers.ContainsKey(AspNetCoreDiagnosticProcessor.TraceIdName))
                traceId = headers[AspNetCoreDiagnosticProcessor.TraceIdName];
            if (traceId.NullOrEmpty())
                traceId = DateTime.Now.ToString("yyyyMMdd-HHmmss-fff-") + DiagnosticLogConfig.Config.ClientName + "-" + DiagnosticLogConfig.Config.EnvName + "-" + Guid.NewGuid().ToString();
            GuidHolder.TraceIdHolder.Value = traceId;
            string pageId = null;
            if (headers.ContainsKey(AspNetCoreDiagnosticProcessor.PageIdName))
                pageId = headers[AspNetCoreDiagnosticProcessor.PageIdName];
            if (pageId.NullOrEmpty())
                pageId = traceId;
            GuidHolder.PageIdHolder.Value = pageId;

            string parentGuid = null;
            if (headers.ContainsKey(AspNetCoreDiagnosticProcessor.ParentGuidName))
                parentGuid = headers[AspNetCoreDiagnosticProcessor.ParentGuidName];
            GuidHolder.ParentHolder.Value = parentGuid;

            string parentHttp = null;
            if (headers.ContainsKey(AspNetCoreDiagnosticProcessor.ParentHttpIdName))
                parentHttp = headers[AspNetCoreDiagnosticProcessor.ParentHttpIdName];
            GuidHolder.ParentHttpHolder.Value = parentHttp;
        }

        private LogEntity ToLog(ServerCallContext grpcContext)
        {
            var path = grpcContext.Method;
            var ip = grpcContext.Peer;
            var url = grpcContext.Host;
            var method = grpcContext.Method;
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


        public void EndRequest(ServerCallContext grpcContext)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            var log = ToLog(grpcContext);
            log.LogType = LogTypeEnum.RequestEndSuccess;
            Helper.PostHelper.ProcessLog(log);
        }

        public void DiagnosticUnhandledException(ServerCallContext grpcContext, Exception exception)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            var log = ToLog(grpcContext);
            log.LogType = LogTypeEnum.RequestEndException;
            log.Message = exception.Message;
            log.StackTrace = exception.StackTrace;
            Helper.PostHelper.ProcessLog(log);
        }
    }
}
