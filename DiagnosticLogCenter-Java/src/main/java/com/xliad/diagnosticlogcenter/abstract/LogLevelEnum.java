package com.xliad.diagnosticlogcenter.abstracts;

public enum LogLevelEnum {
    TRACE(0),
    DEBUG(1),
    INFORMATION(2),
    WARNING(3),
    ERROR(4),
    CRITICAL(5),
    接口日志(10);

    private final int value;

    LogLevelEnum(int value) {
        this.value = value;
    }

    public int getValue() {
        return value;
    }

    public static LogLevelEnum fromValue(int value) {
        for (LogLevelEnum level : LogLevelEnum.values()) {
            if (level.value == value) {
                return level;
            }
        }
        return INFORMATION;
    }
}