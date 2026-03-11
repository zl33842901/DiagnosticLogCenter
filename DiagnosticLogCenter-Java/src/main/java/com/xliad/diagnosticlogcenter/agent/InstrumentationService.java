package com.xliad.diagnosticlogcenter.agent;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import jakarta.annotation.PostConstruct;
import jakarta.annotation.PreDestroy;

@Service
public class InstrumentationService {

    @Autowired
    private TracingDiagnosticProcessorObserver observer;

    @PostConstruct
    public void start() {
        // 注册DiagnosticListener
        observer.start();
    }

    @PreDestroy
    public void stop() {
        observer.stop();
    }
}