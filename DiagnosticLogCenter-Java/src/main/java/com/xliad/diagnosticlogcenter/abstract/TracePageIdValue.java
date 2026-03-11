package com.xliad.diagnosticlogcenter.abstracts;

import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.Arrays;
import java.util.UUID;

public class TracePageIdValue {
    private final LocalDateTime happenTime;
    private final String clientName;
    private final String envName;
    private final UUID guid;

    public TracePageIdValue(LocalDateTime happenTime, String clientName, String envName, UUID guid) {
        this.happenTime = happenTime;
        this.clientName = clientName;
        this.envName = envName;
        this.guid = guid;
    }

    public TracePageIdValue(LocalDateTime happenTime, String clientName, String envName) {
        this(happenTime, clientName, envName, UUID.randomUUID());
    }

    public LocalDateTime getHappenTime() {
        return happenTime;
    }

    public String getClientName() {
        return clientName;
    }

    public String getEnvName() {
        return envName;
    }

    public UUID getGuid() {
        return guid;
    }

    @Override
    public String toString() {
        DateTimeFormatter formatter = DateTimeFormatter.ofPattern("yyyyMMdd-HHmmss-SSS");
        return happenTime.format(formatter) + "-" + clientName + "-" + envName + "-" + guid.toString();
    }

    public static TracePageIdValue fromString(String s) {
        if (s == null || s.isEmpty()) {
            throw new IllegalArgumentException("试图转化 null/空串 到一个 TraceId/PageId");
        }
        String[] sa = s.split("-");
        if (sa.length < 6) {
            throw new IllegalArgumentException("转化一个 TraceId/PageId 时提供了一个不合规的字符串: " + s);
        }

        // 解析日期时间
        String datePart = sa[0] + sa[1] + sa[2];
        DateTimeFormatter formatter = DateTimeFormatter.ofPattern("yyyyMMddHHmmssSSS");
        LocalDateTime dt = LocalDateTime.parse(datePart, formatter);

        // 组合剩余的GUID部分
        String guidStr = String.join("-", Arrays.copyOfRange(sa, 5, sa.length));

        return new TracePageIdValue(dt, sa[3], sa[4], UUID.fromString(guidStr));
    }
}