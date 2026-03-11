package com.xliad.diagnosticlogcenter.agent;

import com.xliad.diagnosticlogcenter.agent.diagnosticprocessors.ITracingDiagnosticProcessor;

public class TracingDiagnosticObserver {
    private final TracingDiagnosticMethodCollection methodCollection;

    public TracingDiagnosticObserver(ITracingDiagnosticProcessor processor) {
        this.methodCollection = new TracingDiagnosticMethodCollection(processor);
    }

    public void onNext(String eventName, Object value) {
        for (TracingDiagnosticMethod method : methodCollection) {
            try {
                method.invoke(eventName, value);
            } catch (Exception e) {
                // 忽略异常
            }
        }
    }
}