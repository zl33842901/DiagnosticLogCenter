package com.xliad.diagnosticlogcenter.agent.diagnosticprocessors;

import java.lang.annotation.*;

@Target(ElementType.METHOD)
@Retention(RetentionPolicy.RUNTIME)
public @interface DiagnosticName {
    String value();
}