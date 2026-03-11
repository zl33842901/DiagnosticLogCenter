package com.xliad.diagnosticlogcenter.agent.logger;

import org.slf4j.ILoggerFactory;
import org.slf4j.Logger;
import org.slf4j.event.Level;
import org.springframework.core.env.Environment;

public class DiagnosticLogCenterLoggerProvider implements ILoggerFactory {

    private final Level logLevel;

    public DiagnosticLogCenterLoggerProvider(Environment environment) {
        String logConfig = environment.getProperty("logging.level.root");
        Level level = Level.WARN;

        if (logConfig != null && !logConfig.isEmpty()) {
            try {
                level = Level.valueOf(logConfig.toUpperCase());
            } catch (IllegalArgumentException e) {
                level = Level.WARN;
            }
        }

        this.logLevel = level;
    }

    @Override
    public Logger getLogger(String name) {
        return new DiagnosticLogCenterLogger(logLevel);
    }
}