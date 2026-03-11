package com.xliad.diagnosticlogcenter.agent.diagnosticprocessors;

public class NullParameterResolver implements IParameterResolver {
    @Override
    public Object resolve(Object value) {
        return null;
    }
}