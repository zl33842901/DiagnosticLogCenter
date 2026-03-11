package com.xliad.diagnosticlogcenter.agent.diagnosticprocessors;

import com.xliad.diagnosticlogcenter.abstracts.LogEntity;
import com.xliad.diagnosticlogcenter.abstracts.LogTypeEnum;
import com.xliad.diagnosticlogcenter.agent.GuidHolder;
import com.xliad.diagnosticlogcenter.agent.helper.PostHelper;
import org.springframework.stereotype.Component;
import java.time.LocalDateTime;

@Component
public class MethodDiagnosticProcessor implements ITracingDiagnosticProcessor {

    @Override
    public String getListenerName() {
        return "DiagnosticLogCenterListener";
    }

    @DiagnosticName("xLiAd.DiagnosticLogCenter.Log")
    public void log(@PropertyAttribute(name = "logType") LogTypeEnum logType,
                    @PropertyAttribute(name = "className") String className,
                    @PropertyAttribute(name = "methodName") String methodName,
                    @PropertyAttribute(name = "logContent") String logContent) {
        if (GuidHolder.getHolder() == null) {
            return;
        }

        LogEntity log = new LogEntity();
        log.setLogType(logType);
        log.setClassName(className);
        log.setMethodName(methodName);
        log.setStackTrace(logContent);
        log.setGroupGuid(GuidHolder.getHolder());
        log.setHappenTime(LocalDateTime.now());
        log.setPageId(GuidHolder.getPageIdHolder());
        log.setTraceId(GuidHolder.getTraceIdHolder());
        log.setParentGuid(GuidHolder.getParentHolder());
        log.setParentHttpId(GuidHolder.getParentHttpHolder());

        PostHelper.processLog(log);
    }
}