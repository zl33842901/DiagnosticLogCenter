package com.xliad.diagnosticlogcenter.agent;

import java.util.List;
import java.util.regex.Pattern;

public class FilterRule {
    private static List<String> allowPath;
    private static List<String> forbiddenPath;

    public static void setAllowPath(List<String> paths) {
        allowPath = paths;
    }

    public static void setForbiddenPath(List<String> paths) {
        forbiddenPath = paths;
    }

    public static boolean shouldRecord(String path) {
        System.out.println("FilterRule.shouldRecord: " + path);
        if (path == null) {
            path = "";
        }

        if (allowPath != null && !allowPath.isEmpty()) {
            for (String pattern : allowPath) {
                if (Pattern.matches(pattern, path)) {
                    return true;
                }
            }
            return false;
        } else if (forbiddenPath != null && !forbiddenPath.isEmpty()) {
            for (String pattern : forbiddenPath) {
                if (Pattern.matches(pattern, path)) {
                    return false;
                }
            }
            return true;
        } else {
            return true;
        }
    }
}