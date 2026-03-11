package com.xliad.diagnosticlogcenter.agent;

import com.xliad.diagnosticlogcenter.agent.diagnosticprocessors.DiagnosticName;
import com.xliad.diagnosticlogcenter.agent.diagnosticprocessors.ITracingDiagnosticProcessor;
import java.lang.reflect.Method;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

public class TracingDiagnosticMethodCollection implements Iterable<TracingDiagnosticMethod> {
    private final List<TracingDiagnosticMethod> methods = new ArrayList<>();

    public TracingDiagnosticMethodCollection(ITracingDiagnosticProcessor processor) {
        for (Method method : processor.getClass().getDeclaredMethods()) {
            DiagnosticName diagnosticName = method.getAnnotation(DiagnosticName.class);
            if (diagnosticName != null) {
                methods.add(new TracingDiagnosticMethod(processor, method, diagnosticName.value()));
            }
        }
    }

    @Override
    public Iterator<TracingDiagnosticMethod> iterator() {
        return methods.iterator();
    }
}