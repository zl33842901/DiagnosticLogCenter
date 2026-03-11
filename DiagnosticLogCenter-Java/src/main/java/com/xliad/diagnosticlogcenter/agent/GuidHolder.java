package com.xliad.diagnosticlogcenter.agent;

public class GuidHolder {
    private static final ThreadLocal<String> holder = new ThreadLocal<>();
    private static final ThreadLocal<String> parentHolder = new ThreadLocal<>();
    private static final ThreadLocal<String> parentHttpHolder = new ThreadLocal<>();
    private static final ThreadLocal<String> traceIdHolder = new ThreadLocal<>();
    private static final ThreadLocal<String> pageIdHolder = new ThreadLocal<>();

    public static String getHolder() {
        return holder.get();
    }

    public static void setHolder(String guid) {
        holder.set(guid);
    }

    public static void removeHolder() {
        holder.remove();
    }

    public static String getParentHolder() {
        return parentHolder.get();
    }

    public static void setParentHolder(String parentGuid) {
        parentHolder.set(parentGuid);
    }

    public static void removeParentHolder() {
        parentHolder.remove();
    }

    public static String getParentHttpHolder() {
        return parentHttpHolder.get();
    }

    public static void setParentHttpHolder(String parentHttpId) {
        parentHttpHolder.set(parentHttpId);
    }

    public static void removeParentHttpHolder() {
        parentHttpHolder.remove();
    }

    public static String getTraceIdHolder() {
        return traceIdHolder.get();
    }

    public static void setTraceIdHolder(String traceId) {
        traceIdHolder.set(traceId);
    }

    public static void removeTraceIdHolder() {
        traceIdHolder.remove();
    }

    public static String getPageIdHolder() {
        return pageIdHolder.get();
    }

    public static void setPageIdHolder(String pageId) {
        pageIdHolder.set(pageId);
    }

    public static void removePageIdHolder() {
        pageIdHolder.remove();
    }

    public static void clearAll() {
        holder.remove();
        parentHolder.remove();
        parentHttpHolder.remove();
        traceIdHolder.remove();
        pageIdHolder.remove();
    }
}