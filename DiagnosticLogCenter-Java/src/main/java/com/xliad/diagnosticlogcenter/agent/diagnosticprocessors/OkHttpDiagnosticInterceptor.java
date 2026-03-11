package com.xliad.diagnosticlogcenter.agent.diagnosticprocessors;

import com.xliad.diagnosticlogcenter.abstracts.LogEntity;
import com.xliad.diagnosticlogcenter.abstracts.LogTypeEnum;
import com.xliad.diagnosticlogcenter.agent.DiagnosticLogConfig;
import com.xliad.diagnosticlogcenter.agent.GuidHolder;
import com.xliad.diagnosticlogcenter.agent.helper.PostHelper;
import okhttp3.*;
import java.io.IOException;
import java.time.LocalDateTime;
import java.util.UUID;

/**
 * OkHttp 拦截器
 * 注意：这个类只有在引入 okhttp 依赖时才可用
 */
public class OkHttpDiagnosticInterceptor implements Interceptor {

    @Override
    public Response intercept(Chain chain) throws IOException {
        if (GuidHolder.getHolder() == null) {
            return chain.proceed(chain.request());
        }

        Request request = chain.request();
        String requestId = UUID.randomUUID().toString();

        // 添加跟踪头
        Request.Builder builder = request.newBuilder();
        if (GuidHolder.getPageIdHolder() != null) {
            builder.addHeader(HttpServletDiagnosticProcessor.PAGE_ID_NAME, GuidHolder.getPageIdHolder());
        }
        if (GuidHolder.getTraceIdHolder() != null) {
            builder.addHeader(HttpServletDiagnosticProcessor.TRACE_ID_NAME, GuidHolder.getTraceIdHolder());
        }
        builder.addHeader(HttpServletDiagnosticProcessor.PARENT_GUID_NAME, GuidHolder.getHolder());
        builder.addHeader(HttpServletDiagnosticProcessor.PARENT_HTTP_ID_NAME, requestId);

        Request newRequest = builder.build();

        // 记录请求
        LogEntity requestLog = createRequestLog(newRequest, requestId);
        requestLog.setLogType(LogTypeEnum.HTTP_CLIENT_REQUEST);
        PostHelper.processLog(requestLog);

        try {
            Response response = chain.proceed(newRequest);

            // 记录响应
            LogEntity responseLog = createResponseLog(response, requestId);
            responseLog.setLogType(LogTypeEnum.HTTP_CLIENT_RESPONSE);
            PostHelper.processLog(responseLog);

            return response;

        } catch (Exception e) {
            // 记录异常
            LogEntity errorLog = new LogEntity();
            errorLog.setMessage(e.getMessage());
            errorLog.setStackTrace(getStackTraceAsString(e));
            errorLog.setLogType(LogTypeEnum.HTTP_CLIENT_EXCEPTION);
            errorLog.setHappenTime(LocalDateTime.now());
            errorLog.setGroupGuid(GuidHolder.getHolder());
            errorLog.setPageId(GuidHolder.getPageIdHolder());
            errorLog.setTraceId(GuidHolder.getTraceIdHolder());
            errorLog.setParentGuid(GuidHolder.getParentHolder());
            errorLog.setParentHttpId(GuidHolder.getParentHttpHolder());
            errorLog.setHttpId(requestId);

            PostHelper.processLog(errorLog);
            throw e;
        }
    }

    private LogEntity createRequestLog(Request request, String requestId) {
        HttpUrl url = request.url();

        LogEntity log = new LogEntity();
        log.setMessage(url.toString());
        log.setMethodName(request.method());
        log.setIp(url.host());
        log.setGroupGuid(GuidHolder.getHolder());
        log.setHappenTime(LocalDateTime.now());
        log.setPageId(GuidHolder.getPageIdHolder());
        log.setTraceId(GuidHolder.getTraceIdHolder());
        log.setParentGuid(GuidHolder.getParentHolder());
        log.setHttpId(requestId);
        log.setParentHttpId(GuidHolder.getParentHttpHolder());

        // 记录请求体
        RequestBody body = request.body();
        if (body != null) {
            try {
                // OkHttp 的 body 只能读取一次，这里只记录内容类型
                MediaType contentType = body.contentType();
                log.setParameters("Content-Type: " + (contentType != null ? contentType.toString() : "unknown"));
            } catch (Exception e) {
                // 忽略
            }
        }

        return log;
    }

    private LogEntity createResponseLog(Response response, String requestId) {
        LogEntity log = new LogEntity();
        log.setStatusCode(response.code());
        log.setGroupGuid(GuidHolder.getHolder());
        log.setHappenTime(LocalDateTime.now());
        log.setPageId(GuidHolder.getPageIdHolder());
        log.setTraceId(GuidHolder.getTraceIdHolder());
        log.setParentGuid(GuidHolder.getParentHolder());
        log.setHttpId(requestId);
        log.setParentHttpId(GuidHolder.getParentHttpHolder());

        // 记录响应头信息
        ResponseBody body = response.body();
        if (body != null) {
            MediaType contentType = body.contentType();
            log.setParameters("Content-Type: " + (contentType != null ? contentType.toString() : "unknown") +
                    ", Content-Length: " + body.contentLength());
        }

        return log;
    }

    private String getStackTraceAsString(Exception e) {
        StringBuilder sb = new StringBuilder();
        sb.append(e.toString()).append("\n");
        for (StackTraceElement element : e.getStackTrace()) {
            sb.append("  at ").append(element.toString()).append("\n");
        }
        return sb.toString();
    }
}