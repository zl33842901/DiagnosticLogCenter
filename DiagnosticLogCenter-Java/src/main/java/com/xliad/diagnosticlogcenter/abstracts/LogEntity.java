package com.xliad.diagnosticlogcenter.abstracts;

import java.time.LocalDateTime;

public class LogEntity {
    private String environmentName;
    private String clientName;
    private LocalDateTime happenTime;
    private String message;
    private String stackTrace;
    private LogLevelEnum level;
    private String groupGuid;
    private LogTypeEnum logType;
    private String className;
    private String methodName;
    private String ip;
    private int statusCode;

    // SQL专用
    private String dataSource;
    private String database;
    private String commandText;
    private String parameters;

    private String traceId;
    private String pageId;
    private String parentGuid;
    private String httpId;
    private String parentHttpId;

    // Getters and Setters
    public String getEnvironmentName() { return environmentName; }
    public void setEnvironmentName(String environmentName) { this.environmentName = environmentName; }

    public String getClientName() { return clientName; }
    public void setClientName(String clientName) { this.clientName = clientName; }

    public LocalDateTime getHappenTime() { return happenTime; }
    public void setHappenTime(LocalDateTime happenTime) { this.happenTime = happenTime; }

    public String getMessage() { return message; }
    public void setMessage(String message) { this.message = message; }

    public String getStackTrace() { return stackTrace; }
    public void setStackTrace(String stackTrace) { this.stackTrace = stackTrace; }

    public LogLevelEnum getLevel() { return level; }
    public void setLevel(LogLevelEnum level) { this.level = level; }

    public String getGroupGuid() { return groupGuid; }
    public void setGroupGuid(String groupGuid) { this.groupGuid = groupGuid; }

    public LogTypeEnum getLogType() { return logType; }
    public void setLogType(LogTypeEnum logType) { this.logType = logType; }

    public String getClassName() { return className; }
    public void setClassName(String className) { this.className = className; }

    public String getMethodName() { return methodName; }
    public void setMethodName(String methodName) { this.methodName = methodName; }

    public String getIp() { return ip; }
    public void setIp(String ip) { this.ip = ip; }

    public int getStatusCode() { return statusCode; }
    public void setStatusCode(int statusCode) { this.statusCode = statusCode; }

    public String getDataSource() { return dataSource; }
    public void setDataSource(String dataSource) { this.dataSource = dataSource; }

    public String getDatabase() { return database; }
    public void setDatabase(String database) { this.database = database; }

    public String getCommandText() { return commandText; }
    public void setCommandText(String commandText) { this.commandText = commandText; }

    public String getParameters() { return parameters; }
    public void setParameters(String parameters) { this.parameters = parameters; }

    public String getTraceId() { return traceId; }
    public void setTraceId(String traceId) { this.traceId = traceId; }

    public String getPageId() { return pageId; }
    public void setPageId(String pageId) { this.pageId = pageId; }

    public String getParentGuid() { return parentGuid; }
    public void setParentGuid(String parentGuid) { this.parentGuid = parentGuid; }

    public String getHttpId() { return httpId; }
    public void setHttpId(String httpId) { this.httpId = httpId; }

    public String getParentHttpId() { return parentHttpId; }
    public void setParentHttpId(String parentHttpId) { this.parentHttpId = parentHttpId; }
}