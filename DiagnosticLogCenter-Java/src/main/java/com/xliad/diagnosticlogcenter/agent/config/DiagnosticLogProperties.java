package com.xliad.diagnosticlogcenter.agent.config;

import org.springframework.boot.context.properties.ConfigurationProperties;

import java.util.List;

@ConfigurationProperties(prefix = "diagnostic.log")
public class DiagnosticLogProperties {

    private boolean enable = true;
    private boolean enableAspNetCore = true;
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

    // Getters and Setters
    public boolean isEnable() { return enable; }
    public void setEnable(boolean enable) { this.enable = enable; }

    public boolean isEnableAspNetCore() { return enableAspNetCore; }
    public void setEnableAspNetCore(boolean enableAspNetCore) { this.enableAspNetCore = enableAspNetCore; }

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
}