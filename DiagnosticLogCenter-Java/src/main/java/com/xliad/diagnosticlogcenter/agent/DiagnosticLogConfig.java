package com.xliad.diagnosticlogcenter.agent;

import java.util.List;
import java.util.function.BiConsumer;
import java.util.function.Consumer;

public class DiagnosticLogConfig {
    private static DiagnosticLogConfig config;

    private boolean enable = true;
    private boolean enableAspNetCore = true;
    private boolean enableDapperEx = true;
    private boolean enableHttpClient = true;
    private boolean enableSqlClient = true;
    private boolean enableMethod = true;
    private boolean enableSystemLog = true;
    private boolean recordSqlParameters = true;
    private boolean recordHttpClientBody = true;
    private List<String> allowPath;
    private List<String> forbiddenPath;
    private String collectServerAddress;
    private String clientName = "Sample";
    private String envName = "PRD";
    private int timeoutBySecond = 5;
    private int recordHttpClientRequestBodyMax = 262144;
    private int recordHttpClientResponseBodyMax = 262144;
    private boolean recordHttpClientFullWhenJson = true;

    // 事件回调
    private Consumer<RequestEvent> onAspNetCoreBeginRequest;
    private Consumer<RequestEvent> onAspNetCoreEndRequest;
    private BiConsumer<RequestEvent, Exception> onAspNetCoreException;

    public static DiagnosticLogConfig getConfig() {
        return config;
    }

    public static void setConfig(DiagnosticLogConfig config) {
        DiagnosticLogConfig.config = config;
    }

    // Getters and Setters
    public boolean isEnable() { return enable; }
    public void setEnable(boolean enable) { this.enable = enable; }

    public boolean isEnableAspNetCore() { return enableAspNetCore; }
    public void setEnableAspNetCore(boolean enableAspNetCore) { this.enableAspNetCore = enableAspNetCore; }

    public boolean isEnableDapperEx() { return enableDapperEx; }
    public void setEnableDapperEx(boolean enableDapperEx) { this.enableDapperEx = enableDapperEx; }

    public boolean isEnableHttpClient() { return enableHttpClient; }
    public void setEnableHttpClient(boolean enableHttpClient) { this.enableHttpClient = enableHttpClient; }

    public boolean isEnableSqlClient() { return enableSqlClient; }
    public void setEnableSqlClient(boolean enableSqlClient) { this.enableSqlClient = enableSqlClient; }

    public boolean isEnableMethod() { return enableMethod; }
    public void setEnableMethod(boolean enableMethod) { this.enableMethod = enableMethod; }

    public boolean isEnableSystemLog() { return enableSystemLog; }
    public void setEnableSystemLog(boolean enableSystemLog) { this.enableSystemLog = enableSystemLog; }

    public boolean isRecordSqlParameters() { return recordSqlParameters; }
    public void setRecordSqlParameters(boolean recordSqlParameters) { this.recordSqlParameters = recordSqlParameters; }

    public boolean isRecordHttpClientBody() { return recordHttpClientBody; }
    public void setRecordHttpClientBody(boolean recordHttpClientBody) { this.recordHttpClientBody = recordHttpClientBody; }

    public List<String> getAllowPath() { return allowPath; }
    public void setAllowPath(List<String> allowPath) { this.allowPath = allowPath; }

    public List<String> getForbiddenPath() { return forbiddenPath; }
    public void setForbiddenPath(List<String> forbiddenPath) { this.forbiddenPath = forbiddenPath; }

    public String getCollectServerAddress() { return collectServerAddress; }
    public void setCollectServerAddress(String collectServerAddress) { this.collectServerAddress = collectServerAddress; }

    public String getClientName() { return clientName; }
    public void setClientName(String clientName) { this.clientName = clientName; }

    public String getEnvName() { return envName; }
    public void setEnvName(String envName) { this.envName = envName; }

    public int getTimeoutBySecond() { return timeoutBySecond; }
    public void setTimeoutBySecond(int timeoutBySecond) { this.timeoutBySecond = timeoutBySecond; }

    public int getRecordHttpClientRequestBodyMax() { return recordHttpClientRequestBodyMax; }
    public void setRecordHttpClientRequestBodyMax(int recordHttpClientRequestBodyMax) { this.recordHttpClientRequestBodyMax = recordHttpClientRequestBodyMax; }

    public int getRecordHttpClientResponseBodyMax() { return recordHttpClientResponseBodyMax; }
    public void setRecordHttpClientResponseBodyMax(int recordHttpClientResponseBodyMax) { this.recordHttpClientResponseBodyMax = recordHttpClientResponseBodyMax; }

    public boolean isRecordHttpClientFullWhenJson() { return recordHttpClientFullWhenJson; }
    public void setRecordHttpClientFullWhenJson(boolean recordHttpClientFullWhenJson) { this.recordHttpClientFullWhenJson = recordHttpClientFullWhenJson; }

    public Consumer<RequestEvent> getOnAspNetCoreBeginRequest() { return onAspNetCoreBeginRequest; }
    public void setOnAspNetCoreBeginRequest(Consumer<RequestEvent> onAspNetCoreBeginRequest) { this.onAspNetCoreBeginRequest = onAspNetCoreBeginRequest; }

    public Consumer<RequestEvent> getOnAspNetCoreEndRequest() { return onAspNetCoreEndRequest; }
    public void setOnAspNetCoreEndRequest(Consumer<RequestEvent> onAspNetCoreEndRequest) { this.onAspNetCoreEndRequest = onAspNetCoreEndRequest; }

    public BiConsumer<RequestEvent, Exception> getOnAspNetCoreException() { return onAspNetCoreException; }
    public void setOnAspNetCoreException(BiConsumer<RequestEvent, Exception> onAspNetCoreException) { this.onAspNetCoreException = onAspNetCoreException; }

    // 事件类
    public static class RequestEvent {
        private final String guid;
        private final Object httpContext;

        public RequestEvent(String guid, Object httpContext) {
            this.guid = guid;
            this.httpContext = httpContext;
        }

        public String getGuid() { return guid; }
        public Object getHttpContext() { return httpContext; }
    }
}