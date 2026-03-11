package com.xliad.diagnosticlogcenter.agent.diagnosticprocessors;

import java.lang.annotation.*;

@Target(ElementType.PARAMETER)
@Retention(RetentionPolicy.RUNTIME)
public @interface ObjectAttribute {
    Class<?> targetType() default Object.class;
}

class ObjectResolver implements IParameterResolver {
    private final Class<?> targetType;

    public ObjectResolver(Class<?> targetType) {
        this.targetType = targetType;
    }

    @Override
    public Object resolve(Object value) {
        if (targetType == null || value == null) {
            return value;
        }

        if (targetType.isAssignableFrom(value.getClass())) {
            return value;
        }

        return null;
    }
}