package com.xliad.diagnosticlogcenter.agent;

import com.xliad.diagnosticlogcenter.agent.diagnosticprocessors.IParameterResolver;
import com.xliad.diagnosticlogcenter.agent.diagnosticprocessors.ITracingDiagnosticProcessor;
import com.xliad.diagnosticlogcenter.agent.diagnosticprocessors.NullParameterResolver;
import com.xliad.diagnosticlogcenter.agent.diagnosticprocessors.ParameterBinder;
import com.xliad.diagnosticlogcenter.agent.diagnosticprocessors.PropertyAttribute;
import java.lang.annotation.Annotation;
import java.lang.reflect.Method;
import java.lang.reflect.Parameter;
import java.util.ArrayList;
import java.util.List;

public class TracingDiagnosticMethod {
    private final ITracingDiagnosticProcessor processor;
    private final Method method;
    private final String diagnosticName;
    private final List<IParameterResolver> parameterResolvers = new ArrayList<>();

    public TracingDiagnosticMethod(ITracingDiagnosticProcessor processor, Method method, String diagnosticName) {
        this.processor = processor;
        this.method = method;
        this.diagnosticName = diagnosticName;
        initParameterResolvers();
    }

    private void initParameterResolvers() {
        for (Parameter parameter : method.getParameters()) {
            IParameterResolver resolver = null;

            for (Annotation annotation : parameter.getAnnotations()) {
                if (annotation instanceof ParameterBinder) {
                    resolver = (IParameterResolver) annotation;
                    break;
                }
            }

            if (resolver == null) {
                resolver = new NullParameterResolver();
            }

            parameterResolvers.add(resolver);
        }
    }

    public void invoke(String eventName, Object value) {
        if (!diagnosticName.equals(eventName)) {
            return;
        }

        Object[] args = new Object[parameterResolvers.size()];
        for (int i = 0; i < parameterResolvers.size(); i++) {
            args[i] = parameterResolvers.get(i).resolve(value);
        }

        try {
            method.setAccessible(true);
            method.invoke(processor, args);
        } catch (Exception e) {
            // 忽略异常
        }
    }
}