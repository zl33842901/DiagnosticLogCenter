package com.xliad.diagnosticlogcenter.agent.diagnosticprocessors;

import com.xliad.diagnosticlogcenter.abstracts.LogEntity;
import com.xliad.diagnosticlogcenter.abstracts.LogTypeEnum;
import com.xliad.diagnosticlogcenter.agent.DiagnosticLogConfig;
import com.xliad.diagnosticlogcenter.agent.GuidHolder;
import com.xliad.diagnosticlogcenter.agent.helper.PostHelper;
import org.apache.hc.core5.http.*;
import org.apache.hc.core5.http.protocol.HttpContext;
import java.io.IOException;
import java.time.LocalDateTime;
import java.util.UUID;

/**
 * Apache HttpClient5 拦截器
 * 兼容 Spring Boot 3.x
 */
public class ApacheHttpClient5DiagnosticProcessor implements HttpRequestInterceptor, HttpResponseInterceptor {

    private static final ThreadLocal<String> REQUEST_ID_HOLDER = new ThreadLocal<>();

    @Override
    public void process(HttpRequest request, EntityDetails entity, HttpContext context) throws HttpException, IOException {
        if (GuidHolder.getHolder() == null) {
            return;
        }

        String requestId = UUID.randomUUID().toString();
        REQUEST_ID_HOLDER.set(requestId);

        // 添加跟踪头
        if (GuidHolder.getPageIdHolder() != null) {
            request.setHeader(HttpServletDiagnosticProcessor.PAGE_ID_NAME, GuidHolder.getPageIdHolder());
        }
        if (GuidHolder.getTraceIdHolder() != null) {
            request.setHeader(HttpServletDiagnosticProcessor.TRACE_ID_NAME, GuidHolder.getTraceIdHolder());
        }
        request.setHeader(HttpServletDiagnosticProcessor.PARENT_GUID_NAME, GuidHolder.getHolder());
        request.setHeader(HttpServletDiagnosticProcessor.PARENT_HTTP_ID_NAME, requestId);

        // 记录请求
        LogEntity requestLog = createRequestLog(request, requestId);
        requestLog.setLogType(LogTypeEnum.HTTP_CLIENT_REQUEST);
        PostHelper.processLog(requestLog);
    }

    @Override
    public void process(HttpResponse response, EntityDetails entity, HttpContext context) throws HttpException, IOException {
        String requestId = REQUEST_ID_HOLDER.get();
        if (GuidHolder.getHolder() == null || requestId == null) {
            return;
        }

        // 记录响应
        LogEntity responseLog = createResponseLog(response, requestId);
        responseLog.setLogType(LogTypeEnum.HTTP_CLIENT_RESPONSE);
        PostHelper.processLog(responseLog);

        REQUEST_ID_HOLDER.remove();
    }

    private LogEntity createRequestLog(HttpRequest request, String requestId) {
        LogEntity log = new LogEntity();
        log.setMessage(request.getRequestUri());
        log.setMethodName(request.getMethod());
        log.setGroupGuid(GuidHolder.getHolder());
        log.setHappenTime(LocalDateTime.now());
        log.setPageId(GuidHolder.getPageIdHolder());
        log.setTraceId(GuidHolder.getTraceIdHolder());
        log.setParentGuid(GuidHolder.getParentHolder());
        log.setHttpId(requestId);
        log.setParentHttpId(GuidHolder.getParentHttpHolder());

        return log;
    }

    private LogEntity createResponseLog(HttpResponse response, String requestId) {
        LogEntity log = new LogEntity();
        log.setStatusCode(response.getCode());
        log.setGroupGuid(GuidHolder.getHolder());
        log.setHappenTime(LocalDateTime.now());
        log.setPageId(GuidHolder.getPageIdHolder());
        log.setTraceId(GuidHolder.getTraceIdHolder());
        log.setParentGuid(GuidHolder.getParentHolder());
        log.setHttpId(requestId);
        log.setParentHttpId(GuidHolder.getParentHttpHolder());

        return log;
    }
}