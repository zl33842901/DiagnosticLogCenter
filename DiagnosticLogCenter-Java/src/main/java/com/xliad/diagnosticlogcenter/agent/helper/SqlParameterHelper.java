package com.xliad.diagnosticlogcenter.agent.helper;

import java.sql.PreparedStatement;
import java.sql.ParameterMetaData;
import java.sql.SQLException;
import java.util.HashMap;
import java.util.Map;

public class SqlParameterHelper {

    public static String convertToString(PreparedStatement statement) {
        StringBuilder sb = new StringBuilder();
        sb.append("{\r\n");

        try {
            ParameterMetaData metaData = statement.getParameterMetaData();
            int parameterCount = metaData.getParameterCount();

            for (int i = 1; i <= parameterCount; i++) {
                sb.append("  ").append("@p").append(i).append(" : ");

                // 这里无法直接获取参数值，需要额外实现
                sb.append("\"<?>\",\r\n");
            }
        } catch (SQLException e) {
            // 忽略
        }

        sb.append("}");
        return sb.toString();
    }

    public static String formatDynamicString(Object parameters) {
        if (parameters == null) {
            return null;
        }

        try {
            Map<String, Object> map = toMap(parameters);
            if (map == null) {
                if (parameters.getClass().isPrimitive() || parameters instanceof Number ||
                        parameters instanceof String || parameters instanceof Boolean) {
                    return parameters.toString();
                } else {
                    try {
                        return new com.fasterxml.jackson.databind.ObjectMapper()
                                .writeValueAsString(parameters);
                    } catch (Exception e) {
                        return null;
                    }
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.append("{ \r\n");
            java.util.List<String> list = new java.util.ArrayList<>();

            for (Map.Entry<String, Object> entry : map.entrySet()) {
                if (entry.getValue() instanceof Iterable && !(entry.getValue() instanceof String)) {
                    StringBuilder sb2 = new StringBuilder();
                    for (Object item : (Iterable<?>) entry.getValue()) {
                        if (sb2.length() > 0) sb2.append(", ");
                        sb2.append('"').append(item).append('"');
                    }
                    list.add("  \"" + entry.getKey() + "\" : [ " + sb2 + " ]");
                } else {
                    list.add("  \"" + entry.getKey() + "\" : \"" + entry.getValue() + "\"");
                }
            }

            sb.append(String.join(",\r\n", list));
            sb.append("\r\n}");
            return sb.toString();
        } catch (Exception e) {
            return "";
        }
    }

    private static Map<String, Object> toMap(Object parameters) {
        // 简化的实现，实际需要根据参数类型进行转换
        if (parameters instanceof Map) {
            return (Map<String, Object>) parameters;
        }
        return null;
    }
}