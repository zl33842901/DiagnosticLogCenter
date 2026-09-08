package com.xliad.diagnosticlogcenter.abstracts;

public enum LogTypeEnum {
    METHOD_ENTRY(1, "进入方法"),
    METHOD_ADDITION(2, "手动日志"),
    METHOD_LEAVE(3, "方法完毕"),
    METHOD_EXCEPTION(4, "方法报错"),

    SQL_BEFORE(11, "Sql开始"),
    SQL_AFTER(12, "Sql完毕"),
    SQL_EXCEPTION(13, "Sql报错"),

    REQUEST_BEGIN(21, "请求开始"),
    REQUEST_END_SUCCESS(22, "请求完毕"),
    REQUEST_END_EXCEPTION(23, "请求报错"),

    HTTP_CLIENT_REQUEST(31, "请求外部"),
    HTTP_CLIENT_RESPONSE(32, "外部成功返回"),
    HTTP_CLIENT_EXCEPTION(33, "外部报错"),

    DAPPER_EX_SQL_BEFORE(41, "执行Sql"),
    DAPPER_EX_SQL_AFTER(42, "执行Sql完成"),
    DAPPER_EX_SQL_EXCEPTION(43, "执行Sql报错");

    private final int value;
    private final String description;

    LogTypeEnum(int value, String description) {
        this.value = value;
        this.description = description;
    }

    public int getValue() {
        return value;
    }

    public String getDescription() {
        return description;
    }

    public static LogTypeEnum fromValue(int value) {
        for (LogTypeEnum type : LogTypeEnum.values()) {
            if (type.value == value) {
                return type;
            }
        }
        return METHOD_ADDITION;
    }
}