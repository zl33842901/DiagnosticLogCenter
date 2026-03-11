package com.xliad.diagnosticlogcenter.agent;

import com.xliad.diagnosticlogcenter.agent.diagnosticprocessors.ITracingDiagnosticProcessor;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;
import java.util.List;
import java.util.concurrent.CopyOnWriteArrayList;

@Component
public class TracingDiagnosticProcessorObserver {
    private final List<ITracingDiagnosticProcessor> processors;
    private final List<TracingDiagnosticObserver> observers = new CopyOnWriteArrayList<>();

    @Autowired
    public TracingDiagnosticProcessorObserver(List<ITracingDiagnosticProcessor> processors) {
        this.processors = processors;
    }

    public void start() {
        for (ITracingDiagnosticProcessor processor : processors) {
            TracingDiagnosticObserver observer = new TracingDiagnosticObserver(processor);
            observers.add(observer);
            // 在实际的Java实现中，这里需要注册到对应的框架监听器
            // 例如Spring的ApplicationListener或Servlet的Filter
            registerListener(processor.getListenerName(), observer);
        }
    }

    private void registerListener(String listenerName, TracingDiagnosticObserver observer) {
        // 根据不同的listenerName注册到不同的框架
        switch (listenerName) {
            case "Microsoft.AspNetCore":
                // 注册到Spring MVC/Servlet Filter
                break;
            case "HttpHandlerDiagnosticListener":
                // 注册到HttpClient拦截器
                break;
            case "SqlClientDiagnosticListener":
                // 注册到JDBC拦截器
                break;
            case "DiagnosticLogCenterListener":
                // 自定义方法监听
                break;
        }
    }

    public void stop() {
        observers.clear();
    }
}