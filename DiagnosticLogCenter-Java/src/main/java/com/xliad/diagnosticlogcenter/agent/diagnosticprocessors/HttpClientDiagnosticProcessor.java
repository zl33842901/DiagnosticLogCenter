package com.xliad.diagnosticlogcenter.agent.diagnosticprocessors;

import com.xliad.diagnosticlogcenter.abstracts.LogEntity;
import com.xliad.diagnosticlogcenter.abstracts.LogTypeEnum;
import com.xliad.diagnosticlogcenter.agent.DiagnosticLogConfig;
import com.xliad.diagnosticlogcenter.agent.GuidHolder;
import com.xliad.diagnosticlogcenter.agent.helper.PostHelper;
import org.springframework.http.HttpRequest;
import org.springframework.http.client.ClientHttpRequestExecution;
import org.springframework.http.client.ClientHttpRequestInterceptor;
import org.springframework.http.client.ClientHttpResponse;
import org.springframework.stereotype.Component;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.URI;
import java.time.LocalDateTime;
import java.util.UUID;
import java.util.stream.Collectors;

/**
 * Spring RestTemplate 拦截器
 * 类似于 SkyWalking 的 HttpClient 插件
 */
@Component
public class HttpClientDiagnosticProcessor implements ClientHttpRequestInterceptor, ITracingDiagnosticProcessor {

    @Override
    public String getListenerName() {
        return "http-client";
    }

    @Override
    public ClientHttpResponse intercept(HttpRequest request, byte[] body,
                                        ClientHttpRequestExecution execution) throws IOException {

        if (GuidHolder.getHolder() == null) {
            return execution.execute(request, body);
        }

        String requestId = UUID.randomUUID().toString();

        // 添加跟踪头到出站请求
        addTracingHeaders(request);
        request.getHeaders().add(HttpServletDiagnosticProcessor.PARENT_GUID_NAME, GuidHolder.getHolder());
        request.getHeaders().add(HttpServletDiagnosticProcessor.PARENT_HTTP_ID_NAME, requestId);

        // 记录请求开始
        LogEntity requestLog = createRequestLog(request, body, requestId);
        requestLog.setLogType(LogTypeEnum.HTTP_CLIENT_REQUEST);
        PostHelper.processLog(requestLog);

        try {
            ClientHttpResponse response = execution.execute(request, body);

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

    private void addTracingHeaders(HttpRequest request) {
        if (GuidHolder.getPageIdHolder() != null) {
            request.getHeaders().add(HttpServletDiagnosticProcessor.PAGE_ID_NAME, GuidHolder.getPageIdHolder());
        }
        if (GuidHolder.getTraceIdHolder() != null) {
            request.getHeaders().add(HttpServletDiagnosticProcessor.TRACE_ID_NAME, GuidHolder.getTraceIdHolder());
        }
    }

    private LogEntity createRequestLog(HttpRequest request, byte[] body, String requestId) {
        URI uri = request.getURI();

        LogEntity log = new LogEntity();
        log.setMessage(uri.toString());
        log.setMethodName(request.getMethod().name());
        log.setIp(uri.getHost());
        log.setGroupGuid(GuidHolder.getHolder());
        log.setHappenTime(LocalDateTime.now());
        log.setPageId(GuidHolder.getPageIdHolder());
        log.setTraceId(GuidHolder.getTraceIdHolder());
        log.setParentGuid(GuidHolder.getParentHolder());
        log.setHttpId(requestId);
        log.setParentHttpId(GuidHolder.getParentHttpHolder());

        // 记录请求体
        DiagnosticLogConfig config = DiagnosticLogConfig.getConfig();
        if (config != null && config.isRecordHttpClientBody() && body != null && body.length > 0) {
            String content = new String(body);
            if (content.length() > config.getRecordHttpClientRequestBodyMax()) {
                content = content.substring(0, config.getRecordHttpClientRequestBodyMax());
            }
            log.setStackTrace(content);
        }

        return log;
    }

    private LogEntity createResponseLog(ClientHttpResponse response, String requestId) throws IOException {
        LogEntity log = new LogEntity();
        log.setStatusCode(response.getRawStatusCode());
        log.setGroupGuid(GuidHolder.getHolder());
        log.setHappenTime(LocalDateTime.now());
        log.setPageId(GuidHolder.getPageIdHolder());
        log.setTraceId(GuidHolder.getTraceIdHolder());
        log.setParentGuid(GuidHolder.getParentHolder());
        log.setHttpId(requestId);
        log.setParentHttpId(GuidHolder.getParentHttpHolder());

        // 记录响应体
        DiagnosticLogConfig config = DiagnosticLogConfig.getConfig();
        if (config != null && config.isRecordHttpClientBody()) {
            try (BufferedReader reader = new BufferedReader(new InputStreamReader(response.getBody()))) {
                String body = reader.lines().collect(Collectors.joining("\n"));
                if (body.length() > config.getRecordHttpClientResponseBodyMax()) {
                    body = body.substring(0, config.getRecordHttpClientResponseBodyMax());
                }
                log.setStackTrace(body);
            } catch (Exception e) {
                // 忽略
            }
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