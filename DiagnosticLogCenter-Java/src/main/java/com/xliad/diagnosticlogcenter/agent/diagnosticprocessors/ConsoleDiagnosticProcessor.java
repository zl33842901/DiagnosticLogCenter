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
import java.time.LocalDateTime;
import java.util.UUID;

@Component
public class ConsoleDiagnosticProcessor {

    public void beginRequest(String methodFullName) {
        boolean shouldRecord = FilterRule.shouldRecord(methodFullName);
        if (!shouldRecord) {
            GuidHolder.setHolder(null);
            return;
        }

        String guid = UUID.randomUUID().toString();
        GuidHolder.setHolder(guid);
        setTraceAndPageId();

        LogEntity log = toLog(methodFullName);
        log.setLogType(LogTypeEnum.REQUEST_BEGIN);
        PostHelper.processLog(log);
    }

    private void setTraceAndPageId() {
        String traceId = new TracePageIdValue(
                LocalDateTime.now(),
                DiagnosticLogConfig.getConfig().getClientName(),
                DiagnosticLogConfig.getConfig().getEnvName()
        ).toString();
        GuidHolder.setTraceIdHolder(traceId);
        GuidHolder.setPageIdHolder(traceId);
        GuidHolder.setParentHolder(null);
        GuidHolder.setParentHttpHolder(null);
    }

    private LogEntity toLog(String methodFullName) {
        LogEntity log = new LogEntity();
        log.setMessage(methodFullName);
        log.setGroupGuid(GuidHolder.getHolder());
        log.setLevel(LogLevelEnum.接口日志);
        log.setHappenTime(LocalDateTime.now());
        log.setPageId(GuidHolder.getPageIdHolder());
        log.setTraceId(GuidHolder.getTraceIdHolder());
        log.setParentGuid(GuidHolder.getParentHolder());
        log.setParentHttpId(GuidHolder.getParentHttpHolder());
        return log;
    }

    public void endRequest(String methodFullName) {
        if (GuidHolder.getHolder() == null) {
            return;
        }

        LogEntity log = toLog(methodFullName);
        log.setLogType(LogTypeEnum.REQUEST_END_SUCCESS);
        PostHelper.processLog(log);
        GuidHolder.clearAll();
    }

    public void requestException(String methodFullName, Exception exception) {
        if (GuidHolder.getHolder() == null) {
            return;
        }

        LogEntity log = toLog(methodFullName);
        log.setLogType(LogTypeEnum.REQUEST_END_EXCEPTION);
        log.setMessage(exception.getMessage());
        log.setStackTrace(exception.getStackTrace().toString());
        PostHelper.processLog(log);
        GuidHolder.clearAll();
    }
}