package com.xliad.diagnosticlogcenter.agent.diagnosticprocessors;

import com.xliad.diagnosticlogcenter.abstracts.LogEntity;
import com.xliad.diagnosticlogcenter.abstracts.LogTypeEnum;
import com.xliad.diagnosticlogcenter.agent.DiagnosticLogConfig;
import com.xliad.diagnosticlogcenter.agent.GuidHolder;
import com.xliad.diagnosticlogcenter.agent.helper.PostHelper;
import org.apache.hc.core5.http.*;
import org.apache.hc.core5.http.protocol.HttpContext;
import org.apache.hc.client5.http.classic.methods.HttpUriRequestBase;
import org.apache.hc.client5.http.impl.classic.CloseableHttpResponse;
import org.springframework.stereotype.Component;

import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.nio.charset.StandardCharsets;
import java.time.LocalDateTime;
import java.util.UUID;

/**
 * Apache HttpClient 5.x 拦截器
 * 通过HttpRequestInterceptor和HttpResponseInterceptor实现追踪
 */
@Component
public class ApacheHttpClient5DiagnosticProcessor implements ITracingDiagnosticProcessor {

    private static final ThreadLocal<String> CURRENT_REQUEST_ID = new ThreadLocal<>();
    private static final ThreadLocal<Long> REQUEST_START_TIME = new ThreadLocal<>();
    private static final ThreadLocal<String> CURRENT_REQUEST_URI = new ThreadLocal<>();
    private static final ThreadLocal<String> CURRENT_REQUEST_METHOD = new ThreadLocal<>();

    @Override
    public String getListenerName() {
        return "apache-httpclient5";
    }

    /**
     * 请求拦截器 - 在请求发送前执行
     */
    public org.apache.hc.core5.http.HttpRequestInterceptor createRequestInterceptor() {
        return (HttpRequest request, EntityDetails entity, HttpContext context) -> {
            if (GuidHolder.getHolder() == null) {
                return;
            }

            String requestId = UUID.randomUUID().toString();
            CURRENT_REQUEST_ID.set(requestId);
            REQUEST_START_TIME.set(System.currentTimeMillis());

            // 记录请求URI和方法
            String uri = getFullRequestUrl(request);
            String method = request.getMethod();
            CURRENT_REQUEST_URI.set(uri);
            CURRENT_REQUEST_METHOD.set(method);

            // 添加跟踪头到出站请求
            addTracingHeaders(request);

            // 记录请求开始
            LogEntity requestLog = createRequestLog(request, entity, requestId, uri, method);
            requestLog.setLogType(LogTypeEnum.HTTP_CLIENT_REQUEST);
            PostHelper.processLog(requestLog);
        };
    }

    /**
     * 响应拦截器 - 在收到响应后执行
     */
    public org.apache.hc.core5.http.HttpResponseInterceptor createResponseInterceptor() {
        return (HttpResponse response, EntityDetails entity, HttpContext context) -> {
            String requestId = CURRENT_REQUEST_ID.get();
            if (requestId == null) {
                return;
            }

            try {
                String responseBody = null;

                DiagnosticLogConfig config = DiagnosticLogConfig.getConfig();

                // 如果需要记录响应体
                if (config != null && config.isRecordHttpClientBody() &&
                        response instanceof ClassicHttpResponse) {

                    ClassicHttpResponse classicResponse = (ClassicHttpResponse) response;
                    HttpEntity originalEntity = classicResponse.getEntity();

                    if (originalEntity != null) {
                        try {
                            // 使用 RepeatableHttpEntity 包装，使其可重复读取
                            RepeatableHttpEntity repeatableEntity = new RepeatableHttpEntity(originalEntity);

                            // 读取内容用于日志
                            byte[] contentBytes = repeatableEntity.getContentBytes();
                            String text = new String(contentBytes, StandardCharsets.UTF_8);

                            // 判断是否为二进制
                            boolean isBinary = text.contains("\uFFFD") || hasNonPrintableCharacters(contentBytes);

                            StringBuilder contentBuilder = new StringBuilder();

                            // 添加 Content-Type 和 Content-Length 信息
                            String contentType = originalEntity.getContentType();
                            long contentLength = originalEntity.getContentLength();

                            contentBuilder.append("Content-Type: ").append(contentType != null ? contentType : "unknown").append("\n");
                            contentBuilder.append("Content-Length: ").append(contentLength > 0 ? contentLength : contentBytes.length).append("\n");

                            if (isBinary) {
                                contentBuilder.append("检测到二进制，将提取一些文本：\n");
                                contentBuilder.append(extractPrintableCharacters(contentBytes, config.getRecordHttpClientResponseBodyMax()));
                            } else {
                                // 判断是否为 JSON
                                String trimmedText = text.trim();
                                boolean isJson = (trimmedText.startsWith("{") && trimmedText.endsWith("}")) ||
                                        (trimmedText.startsWith("[") && trimmedText.endsWith("]"));

                                if (text.length() > config.getRecordHttpClientResponseBodyMax() &&
                                        (!isJson || !config.isRecordHttpClientFullWhenJson())) {
                                    contentBuilder.append(text.substring(0, config.getRecordHttpClientResponseBodyMax()));
                                } else {
                                    contentBuilder.append(text);
                                }
                            }

                            responseBody = contentBuilder.toString();

                            // 重新设置可重复读取的响应体
                            classicResponse.setEntity(repeatableEntity);

                        } catch (Exception e) {
                            responseBody = "读取响应体失败: " + e.getMessage();
                        }
                    }
                }

                // 记录响应
                LogEntity responseLog = createResponseLog(response, requestId, responseBody);
                responseLog.setLogType(LogTypeEnum.HTTP_CLIENT_RESPONSE);

//                Long startTime = REQUEST_START_TIME.get();
//                if (startTime != null) {
//                    responseLog.setElapsedMilliseconds(System.currentTimeMillis() - startTime);
//                }
                responseLog.setHappenTime(LocalDateTime.now());

                PostHelper.processLog(responseLog);

            } catch (Exception e) {
                recordException(e, requestId);
            } finally {
                // 清理ThreadLocal
                clearThreadLocal();
            }
        };
    }

    /**
     * 异常处理
     */
    public void handleException(Exception e) {
        String requestId = CURRENT_REQUEST_ID.get();
        if (requestId != null) {
            recordException(e, requestId);
            clearThreadLocal();
        }
    }

    private void clearThreadLocal() {
        CURRENT_REQUEST_ID.remove();
        REQUEST_START_TIME.remove();
        CURRENT_REQUEST_URI.remove();
        CURRENT_REQUEST_METHOD.remove();
    }

    /**
     * 获取完整的请求URL
     */
    private String getFullRequestUrl(HttpRequest request) {
        try {
            // 尝试从request中获取完整URL
            // 方法1: 如果request是HttpUriRequestBase类型，可以直接获取URI
            if (request instanceof org.apache.hc.client5.http.classic.methods.HttpUriRequestBase) {
                org.apache.hc.client5.http.classic.methods.HttpUriRequestBase uriRequest =
                        (org.apache.hc.client5.http.classic.methods.HttpUriRequestBase) request;
                java.net.URI uri = uriRequest.getUri();
                if (uri != null) {
                    return uri.toString();
                }
            }

            // 方法2: 从请求头中获取Host和协议
            String host = request.getAuthority().getHostName();
            String scheme = request.getScheme();
            String path = CURRENT_REQUEST_URI.get();
            if (path == null) {
                path = request.getRequestUri();
            }

            if (host != null && scheme != null) {
                // 构建完整URL
                return scheme + "://" + host + path;
            }

            // 如果都没有，返回相对路径
            return path;

        } catch (Exception e) {
            // 出错时返回相对路径
            return CURRENT_REQUEST_URI.get() != null ? CURRENT_REQUEST_URI.get() : request.getRequestUri();
        }
    }

    private void recordException(Exception e, String requestId) {
        try {
            LogEntity errorLog = new LogEntity();
            errorLog.setMessage(e.getMessage());
            errorLog.setStackTrace(getStackTraceAsString(e));
            errorLog.setLogType(LogTypeEnum.HTTP_CLIENT_EXCEPTION);
            errorLog.setHappenTime(LocalDateTime.now());

            // 设置链路信息
            errorLog.setGroupGuid(GuidHolder.getHolder());
            errorLog.setPageId(GuidHolder.getPageIdHolder());
            errorLog.setTraceId(GuidHolder.getTraceIdHolder());
            errorLog.setParentGuid(GuidHolder.getParentHolder());
            errorLog.setParentHttpId(GuidHolder.getParentHttpHolder());
            errorLog.setHttpId(requestId);
//            errorLog.setMessage(CURRENT_REQUEST_URI.get());
//            errorLog.setMethodName(CURRENT_REQUEST_METHOD.get());

//            Long startTime = REQUEST_START_TIME.get();
//            if (startTime != null) {
//                errorLog.setElapsedMilliseconds(System.currentTimeMillis() - startTime);
//            }
            errorLog.setHappenTime(LocalDateTime.now());

            PostHelper.processLog(errorLog);
        } catch (Exception ex) {
            // 忽略
        }
    }

    private void addTracingHeaders(HttpRequest request) {
        if (GuidHolder.getPageIdHolder() != null) {
            request.addHeader(HttpServletDiagnosticProcessor.PAGE_ID_NAME, GuidHolder.getPageIdHolder());
        }
        if (GuidHolder.getTraceIdHolder() != null) {
            request.addHeader(HttpServletDiagnosticProcessor.TRACE_ID_NAME, GuidHolder.getTraceIdHolder());
        }
        if (GuidHolder.getHolder() != null) {
            request.addHeader(HttpServletDiagnosticProcessor.PARENT_GUID_NAME, GuidHolder.getHolder());
        }
        if (CURRENT_REQUEST_ID.get() != null) {
            request.addHeader(HttpServletDiagnosticProcessor.PARENT_HTTP_ID_NAME, CURRENT_REQUEST_ID.get());
        }
    }

    private LogEntity createRequestLog(HttpRequest request, EntityDetails entity, String requestId, String uri, String method) {
        LogEntity log = new LogEntity();
        log.setMessage(uri);
        System.out.println("=== ApacheHttpRequestStart ===");
        System.out.println(log.getMessage());
        log.setMethodName(method);

        // 解析host
        try {
            if (uri != null && uri.startsWith("http")) {
                java.net.URI netUri = java.net.URI.create(uri);
                log.setIp(netUri.getHost());
            }
        } catch (Exception e) {
            // ignore
        }

        log.setGroupGuid(GuidHolder.getHolder());
        log.setHappenTime(LocalDateTime.now());
        log.setPageId(GuidHolder.getPageIdHolder());
        log.setTraceId(GuidHolder.getTraceIdHolder());
        log.setParentGuid(GuidHolder.getParentHolder());
        log.setHttpId(requestId);
        log.setParentHttpId(GuidHolder.getParentHttpHolder());

        // 记录请求体（HttpClient 5.x中需要特殊处理）
        DiagnosticLogConfig config = DiagnosticLogConfig.getConfig();
        if (config != null && config.isRecordHttpClientBody() && entity != null) {
            try {
                // 注意：这里不能直接读取entity内容，因为会消耗流
                // 实际使用时需要通过包装HttpRequest来实现可重复读
                log.setStackTrace("[Request body logging requires repeatable entity]");
            } catch (Exception e) {
                // 忽略
            }
        }

        return log;
    }

    private LogEntity createResponseLog(HttpResponse response, String requestId, String responseBody) {
        LogEntity log = new LogEntity();
        if (response.getCode() > 0) {
            log.setStatusCode(response.getCode());
        }
        log.setGroupGuid(GuidHolder.getHolder());
        log.setHappenTime(LocalDateTime.now());
        log.setPageId(GuidHolder.getPageIdHolder());
        log.setTraceId(GuidHolder.getTraceIdHolder());
        log.setParentGuid(GuidHolder.getParentHolder());
        log.setHttpId(requestId);
        log.setParentHttpId(GuidHolder.getParentHttpHolder());

        // 记录响应体
        if (responseBody != null) {
            log.setStackTrace(responseBody);
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

    /**
     * 检查是否包含不可打印字符
     */
    private boolean hasNonPrintableCharacters(byte[] bytes) {
        for (byte b : bytes) {
            if (b < 9 || (b > 13 && b < 32)) {
                return true;
            }
        }
        return false;
    }

    /**
     * 从二进制中提取可打印字符
     */
    private String extractPrintableCharacters(byte[] bytes, int maxLength) {
        StringBuilder sb = new StringBuilder();
        for (byte b : bytes) {
            if (sb.length() >= maxLength) break;

            // ASCII 可打印字符范围
            if (b >= 32 && b <= 126) {
                sb.append((char) b);
            }
        }
        return sb.toString();
    }
}