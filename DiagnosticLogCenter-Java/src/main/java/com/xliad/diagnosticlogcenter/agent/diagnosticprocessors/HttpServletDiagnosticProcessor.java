package com.xliad.diagnosticlogcenter.agent.diagnosticprocessors;

import com.xliad.diagnosticlogcenter.abstracts.LogEntity;
import com.xliad.diagnosticlogcenter.abstracts.LogLevelEnum;
import com.xliad.diagnosticlogcenter.abstracts.LogTypeEnum;
import com.xliad.diagnosticlogcenter.abstracts.TracePageIdValue;
import com.xliad.diagnosticlogcenter.agent.DiagnosticLogConfig;
import com.xliad.diagnosticlogcenter.agent.FilterRule;
import com.xliad.diagnosticlogcenter.agent.GuidHolder;
import com.xliad.diagnosticlogcenter.agent.helper.PostHelper;
import org.springframework.stereotype.Component;

import jakarta.servlet.*;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import java.io.IOException;
import java.time.LocalDateTime;
import java.util.UUID;

/**
 * Servlet Filter 实现 HTTP 请求的链路跟踪
 * 类似于 SkyWalking 的 Servlet 插件
 */
@Component
public class HttpServletDiagnosticProcessor implements Filter, ITracingDiagnosticProcessor {

    public static final String TRACE_ID_NAME = "x-diagnostic-trace-id";
    public static final String PAGE_ID_NAME = "x-diagnostic-page-id";
    public static final String PARENT_GUID_NAME = "x-diagnostic-parent-guid";
    public static final String PARENT_HTTP_ID_NAME = "x-diagnostic-parent-http-id";

    public HttpServletDiagnosticProcessor() {
        System.out.println("=== HttpServletDiagnosticProcessor 实例化 ===");
        System.out.println("实例化时间: " + new java.util.Date());
        System.out.println("类加载器: " + this.getClass().getClassLoader());
    }

    @Override
    public void init(FilterConfig filterConfig) throws ServletException {
        System.out.println("=== HttpServletDiagnosticProcessor.init() 被调用 ===");
        System.out.println("FilterConfig: " + filterConfig);
    }

    @Override
    public void destroy() {
        System.out.println("=== HttpServletDiagnosticProcessor.destroy() 被调用 ===");
    }

    @Override
    public String getListenerName() {
        return "http-servlet";
    }

    @Override
    public void doFilter(ServletRequest request, ServletResponse response, FilterChain chain)
            throws IOException, ServletException {

        HttpServletRequest httpRequest = (HttpServletRequest) request;
        HttpServletResponse httpResponse = (HttpServletResponse) response;

        // 检查是否需要记录
        boolean shouldRecord = FilterRule.shouldRecord(httpRequest.getRequestURI());
        if (!shouldRecord) {
            chain.doFilter(request, response);
            return;
        }

        // 开始请求
        String guid = UUID.randomUUID().toString();
        GuidHolder.setHolder(guid);
        setTraceAndPageId(httpRequest);

        try {
            // 记录请求开始
            LogEntity beginLog = createRequestLog(httpRequest, true);
            beginLog.setLogType(LogTypeEnum.REQUEST_BEGIN);
            PostHelper.processLog(beginLog);

            // 调用事件回调
            DiagnosticLogConfig config = DiagnosticLogConfig.getConfig();
            if (config != null && config.getOnAspNetCoreBeginRequest() != null) {
                config.getOnAspNetCoreBeginRequest().accept(
                        new DiagnosticLogConfig.RequestEvent(guid, httpRequest)
                );
            }

            // 继续请求处理
            chain.doFilter(request, response);

            // 记录请求成功结束
            LogEntity endLog = createRequestLog(httpRequest, false);
            endLog.setLogType(LogTypeEnum.REQUEST_END_SUCCESS);
            endLog.setStatusCode(httpResponse.getStatus());
            PostHelper.processLog(endLog);

            if (config != null && config.getOnAspNetCoreEndRequest() != null) {
                config.getOnAspNetCoreEndRequest().accept(
                        new DiagnosticLogConfig.RequestEvent(guid, httpRequest)
                );
            }

        } catch (Exception e) {
            // 记录请求异常
            LogEntity errorLog = createRequestLog(httpRequest, false);
            errorLog.setLogType(LogTypeEnum.REQUEST_END_EXCEPTION);
            errorLog.setMessage(e.getMessage());
            errorLog.setStackTrace(getStackTraceAsString(e));
            PostHelper.processLog(errorLog);

            DiagnosticLogConfig config = DiagnosticLogConfig.getConfig();
            if (config != null && config.getOnAspNetCoreException() != null) {
                config.getOnAspNetCoreException().accept(
                        new DiagnosticLogConfig.RequestEvent(guid, httpRequest), e
                );
            }

            throw e;
        } finally {
            // 清理上下文
            GuidHolder.clearAll();
        }
    }

    private void setTraceAndPageId(HttpServletRequest request) {
        // 从请求头获取或生成 TraceId
        String traceId = request.getHeader(TRACE_ID_NAME);
        if (traceId == null || traceId.isEmpty()) {
            traceId = new TracePageIdValue(
                    LocalDateTime.now(),
                    DiagnosticLogConfig.getConfig().getClientName(),
                    DiagnosticLogConfig.getConfig().getEnvName()
            ).toString();
        }
        GuidHolder.setTraceIdHolder(traceId);

        // 从请求头获取或生成 PageId
        String pageId = request.getHeader(PAGE_ID_NAME);
        if (pageId == null || pageId.isEmpty()) {
            pageId = traceId;
        }
        GuidHolder.setPageIdHolder(pageId);

        // 从请求头获取 ParentGuid
        String parentGuid = request.getHeader(PARENT_GUID_NAME);
        GuidHolder.setParentHolder(parentGuid);

        // 从请求头获取 ParentHttpId
        String parentHttp = request.getHeader(PARENT_HTTP_ID_NAME);
        GuidHolder.setParentHttpHolder(parentHttp);
    }

    private LogEntity createRequestLog(HttpServletRequest request, boolean isStart) {
        String path = request.getRequestURI();
        String ip = getClientIp(request);
        String url = request.getRequestURL().toString();
        String method = request.getMethod();

        StringBuilder stackTrace = new StringBuilder();
        if (isStart) {
            stackTrace.append("Url：").append(url).append("\n")
                    .append("IP：").append(ip).append("\n")
                    .append("Method：").append(method).append("\n")
                    .append("Headers：\n");

            java.util.Enumeration<String> headerNames = request.getHeaderNames();
            while (headerNames.hasMoreElements()) {
                String headerName = headerNames.nextElement();
                stackTrace.append("  ").append(headerName).append(": ")
                        .append(request.getHeader(headerName)).append("\n");
            }
        }

        LogEntity log = new LogEntity();
        log.setMessage(path);
        log.setStackTrace(stackTrace.toString());
        log.setMethodName(method);
        log.setGroupGuid(GuidHolder.getHolder());
        log.setIp(ip);
        log.setLevel(LogLevelEnum.接口日志);
        log.setHappenTime(LocalDateTime.now());
        log.setPageId(GuidHolder.getPageIdHolder());
        log.setTraceId(GuidHolder.getTraceIdHolder());
        log.setParentGuid(GuidHolder.getParentHolder());
        log.setParentHttpId(GuidHolder.getParentHttpHolder());

        return log;
    }

    private String getClientIp(HttpServletRequest request) {
        String ip = request.getHeader("X-Forwarded-For");
        if (ip == null || ip.isEmpty() || "unknown".equalsIgnoreCase(ip)) {
            ip = request.getHeader("Proxy-Client-IP");
        }
        if (ip == null || ip.isEmpty() || "unknown".equalsIgnoreCase(ip)) {
            ip = request.getHeader("WL-Proxy-Client-IP");
        }
        if (ip == null || ip.isEmpty() || "unknown".equalsIgnoreCase(ip)) {
            ip = request.getRemoteAddr();
        }
        return ip;
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