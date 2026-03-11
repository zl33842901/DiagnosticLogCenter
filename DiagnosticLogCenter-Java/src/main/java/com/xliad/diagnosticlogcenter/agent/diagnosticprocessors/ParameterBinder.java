package com.xliad.diagnosticlogcenter.agent.diagnosticprocessors;

import java.lang.annotation.*;

@Target(ElementType.PARAMETER)
@Retention(RetentionPolicy.RUNTIME)
public @interface ParameterBinder {
}