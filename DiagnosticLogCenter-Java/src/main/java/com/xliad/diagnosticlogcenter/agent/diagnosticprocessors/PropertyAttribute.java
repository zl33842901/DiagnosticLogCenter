package com.xliad.diagnosticlogcenter.agent.diagnosticprocessors;

import java.lang.annotation.*;
import java.lang.reflect.Field;

@Target(ElementType.PARAMETER)
@Retention(RetentionPolicy.RUNTIME)
public @interface PropertyAttribute {
    String name() default "";
}

class PropertyResolver implements IParameterResolver {
    private final String propertyName;

    public PropertyResolver(String propertyName) {
        this.propertyName = propertyName;
    }

    @Override
    public Object resolve(Object value) {
        if (value == null || propertyName == null || propertyName.isEmpty()) {
            return null;
        }

        try {
            Field field = value.getClass().getDeclaredField(propertyName);
            field.setAccessible(true);
            return field.get(value);
        } catch (Exception e) {
            return null;
        }
    }
}