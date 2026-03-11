package com.xliad.diagnosticlogcenter.agent.diagnosticprocessors;

import com.xliad.diagnosticlogcenter.abstracts.LogEntity;
import com.xliad.diagnosticlogcenter.abstracts.LogTypeEnum;
import com.xliad.diagnosticlogcenter.agent.DiagnosticLogConfig;
import com.xliad.diagnosticlogcenter.agent.GuidHolder;
import com.xliad.diagnosticlogcenter.agent.helper.PostHelper;
import org.aspectj.lang.ProceedingJoinPoint;
import org.aspectj.lang.annotation.Around;
import org.aspectj.lang.annotation.Aspect;
import org.springframework.boot.autoconfigure.condition.ConditionalOnProperty;
import org.springframework.stereotype.Component;

import java.lang.reflect.InvocationHandler;
import java.lang.reflect.Method;
import java.lang.reflect.Proxy;
import java.sql.*;
import java.time.LocalDateTime;
import java.util.HashMap;
import java.util.Map;
import java.util.UUID;

@Aspect
@Component
@ConditionalOnProperty(prefix = "diagnostic.log", name = "enable-sqlclient", havingValue = "true", matchIfMissing = true)
public class SqlClientDiagnosticAspect {

    @Around("execution(* javax.sql.DataSource.getConnection(..))")
    public Object aroundGetConnection(ProceedingJoinPoint joinPoint) throws Throwable {
        Connection connection = (Connection) joinPoint.proceed();

        if (GuidHolder.getHolder() == null) {
            return connection;
        }

        return wrapConnection(connection);
    }

    private Connection wrapConnection(Connection connection) {
        return (Connection) Proxy.newProxyInstance(
                connection.getClass().getClassLoader(),
                new Class[]{Connection.class},
                new ConnectionInvocationHandler(connection)
        );
    }

    private static class ConnectionInvocationHandler implements InvocationHandler {
        private final Connection target;

        public ConnectionInvocationHandler(Connection target) {
            this.target = target;
        }

        @Override
        public Object invoke(Object proxy, Method method, Object[] args) throws Throwable {
            Object result = method.invoke(target, args);

            if (result instanceof Statement) {
                return wrapStatement((Statement) result);
            }

            return result;
        }

        private Statement wrapStatement(Statement statement) {
            Class<?>[] interfaces;
            if (statement instanceof CallableStatement) {
                interfaces = new Class[]{CallableStatement.class, PreparedStatement.class, Statement.class};
            } else if (statement instanceof PreparedStatement) {
                interfaces = new Class[]{PreparedStatement.class, Statement.class};
            } else {
                interfaces = new Class[]{Statement.class};
            }

            return (Statement) Proxy.newProxyInstance(
                    statement.getClass().getClassLoader(),
                    interfaces,
                    new StatementInvocationHandler(statement)
            );
        }
    }

    private static class StatementInvocationHandler implements InvocationHandler {
        private final Statement target;
        private String lastSql;
        private String operationId;
        private Map<Integer, Object> parameters;  // 存储参数
        private Map<Integer, Integer> parameterTypes;  // 存储参数类型

        public StatementInvocationHandler(Statement target) {
            this.target = target;
            this.operationId = UUID.randomUUID().toString();
            this.parameters = new HashMap<>();
            this.parameterTypes = new HashMap<>();
        }

        @Override
        public Object invoke(Object proxy, Method method, Object[] args) throws Throwable {
            if (GuidHolder.getHolder() == null) {
                return method.invoke(target, args);
            }

            // 处理 set 方法（参数设置）
            if (method.getName().startsWith("set") && target instanceof PreparedStatement) {
                handleSetParameter(method, args);
                return method.invoke(target, args);
            }

            // 处理 addBatch（批量操作）
            if (method.getName().equals("addBatch") && target instanceof PreparedStatement) {
                // 对于批处理，可以先保存当前参数
                saveBatchParameters();
                return method.invoke(target, args);
            }

            // 处理执行方法
            if (isExecuteMethod(method.getName())) {
                // 提取 SQL（对于PreparedStatement，需要从target获取）
                if (target instanceof PreparedStatement) {
                    extractPreparedStatementSql();
                } else {
                    lastSql = extractSqlFromArgs(method, args);
                }

                // 记录开始，包含参数
                beforeExecute();

                try {
                    Object result = method.invoke(target, args);
                    afterExecute();
                    return result;
                } catch (Exception e) {
                    errorExecute(e);
                    throw e;
                }
            }

            return method.invoke(target, args);
        }

        /**
         * 处理 set 参数方法
         */
        private void handleSetParameter(Method method, Object[] args) {
            try {
                if (args != null && args.length >= 2) {
                    Integer index = null;
                    Object value = null;

                    // 不同的 set 方法参数位置不同
                    if (args[0] instanceof Integer) {
                        index = (Integer) args[0];  // 第一个参数是索引
                        value = args[1];             // 第二个参数是值
                    } else if (args[0] instanceof String) {
                        // 如果是 setString(parameterName, value) 形式
                        // 对于命名参数，我们可能需要特殊处理
                        index = -1;  // 标记为命名参数
                    }

                    if (index != null && index > 0) {
                        parameters.put(index, value);
                        // 记录参数类型
                        parameterTypes.put(index, getParameterType(method.getName()));
                    }
                }
            } catch (Exception e) {
                // 忽略参数记录异常
            }
        }

        /**
         * 从方法名获取参数类型
         */
        private int getParameterType(String methodName) {
            switch (methodName) {
                case "setString": return Types.VARCHAR;
                case "setInt": return Types.INTEGER;
                case "setLong": return Types.BIGINT;
                case "setDouble": return Types.DOUBLE;
                case "setFloat": return Types.FLOAT;
                case "setBoolean": return Types.BOOLEAN;
                case "setDate": return Types.DATE;
                case "setTime": return Types.TIME;
                case "setTimestamp": return Types.TIMESTAMP;
                case "setBigDecimal": return Types.DECIMAL;
                case "setBytes": return Types.VARBINARY;
                case "setNull": return Types.NULL;
                default: return Types.OTHER;
            }
        }

        /**
         * 保存批处理参数
         */
        private void saveBatchParameters() {
            // 对于批处理，可以在这里保存当前参数快照
            // 然后清空 parameters 准备下一个批次
        }

        /**
         * 提取 PreparedStatement 的 SQL
         */
        private void extractPreparedStatementSql() {
            try {
                // 尝试多种方式获取 SQL
                Method getSqlMethod = target.getClass().getMethod("getSql");
                lastSql = (String) getSqlMethod.invoke(target);
            } catch (Exception e) {
                // 如果无法获取，使用 toString
                lastSql = target.toString();
                // 清理 toString 中的多余信息
                if (lastSql != null && lastSql.contains(":")) {
                    lastSql = lastSql.substring(lastSql.indexOf(":") + 1).trim();
                }
            }
        }

        /**
         * 从参数中提取 SQL
         */
        private String extractSqlFromArgs(Method method, Object[] args) {
            if (args != null && args.length > 0 && args[0] instanceof String) {
                return (String) args[0];
            }
            return "Unknown SQL";
        }

        private boolean isExecuteMethod(String methodName) {
            return methodName.equals("execute") ||
                    methodName.equals("executeQuery") ||
                    methodName.equals("executeUpdate") ||
                    methodName.equals("executeBatch") ||
                    methodName.equals("executeLargeBatch") ||
                    methodName.equals("executeLargeUpdate");
        }

        /**
         * 构建带参数的 SQL（用于日志）
         */
        private String buildSqlWithParameters() {
            if (lastSql == null || parameters.isEmpty()) {
                return lastSql;
            }

            StringBuilder sqlWithParams = new StringBuilder();
            sqlWithParams.append("SQL: ").append(lastSql).append("\n");
            sqlWithParams.append("Parameters: [");

            boolean first = true;
            for (Map.Entry<Integer, Object> entry : parameters.entrySet()) {
                if (!first) {
                    sqlWithParams.append(", ");
                }
                sqlWithParams.append(entry.getKey()).append("=");

                Object value = entry.getValue();
                if (value == null) {
                    sqlWithParams.append("null");
                } else if (value instanceof String) {
                    sqlWithParams.append("'").append(value).append("'");
                } else if (value instanceof Date || value instanceof Time || value instanceof Timestamp) {
                    sqlWithParams.append("'").append(value).append("'");
                } else {
                    sqlWithParams.append(value);
                }

                first = false;
            }
            sqlWithParams.append("]");

            return sqlWithParams.toString();
        }

        /**
         * 构建参数 JSON（用于 LogEntity）
         */
        private String buildParametersJson() {
            if (parameters.isEmpty()) {
                return null;
            }

            StringBuilder json = new StringBuilder("{");
            boolean first = true;

            for (Map.Entry<Integer, Object> entry : parameters.entrySet()) {
                if (!first) {
                    json.append(",");
                }

                json.append("\"").append(entry.getKey()).append("\":");

                Object value = entry.getValue();
                if (value == null) {
                    json.append("null");
                } else if (value instanceof String || value instanceof Date ||
                        value instanceof Time || value instanceof Timestamp) {
                    json.append("\"").append(value).append("\"");
                } else {
                    json.append(value);
                }

                first = false;
            }
            json.append("}");

            return json.toString();
        }

        private void beforeExecute() {
            try {
                String database = null;
                if (target.getConnection() != null) {
                    database = target.getConnection().getCatalog();
                }

                DiagnosticLogConfig config = DiagnosticLogConfig.getConfig();

                LogEntity log = new LogEntity();
                log.setDataSource(null);
                log.setDatabase(database);

                // 如果需要记录带参数的 SQL
                if (config != null && config.isRecordSqlParameters()) {
                    //log.setCommandText(buildSqlWithParameters());
                    log.setCommandText(lastSql);
                    log.setParameters(buildParametersJson());
                } else {
                    log.setCommandText(lastSql);
                }

                log.setLogType(LogTypeEnum.SQL_BEFORE);
                log.setHappenTime(LocalDateTime.now());
                log.setGroupGuid(GuidHolder.getHolder());
                log.setPageId(GuidHolder.getPageIdHolder());
                log.setTraceId(GuidHolder.getTraceIdHolder());
                log.setParentGuid(GuidHolder.getParentHolder());
                log.setParentHttpId(GuidHolder.getParentHttpHolder());
                log.setHttpId(operationId);

                PostHelper.processLog(log);
            } catch (Exception e) {
                // 忽略
            }
        }

        private void afterExecute() {
            try {
                LogEntity log = new LogEntity();
                log.setLogType(LogTypeEnum.SQL_AFTER);
                log.setHappenTime(LocalDateTime.now());
                log.setGroupGuid(GuidHolder.getHolder());
                log.setPageId(GuidHolder.getPageIdHolder());
                log.setTraceId(GuidHolder.getTraceIdHolder());
                log.setParentGuid(GuidHolder.getParentHolder());
                log.setParentHttpId(GuidHolder.getParentHttpHolder());
                log.setHttpId(operationId);

                PostHelper.processLog(log);
            } catch (Exception e) {
                // 忽略
            }
        }

        private void errorExecute(Exception e) {
            try {
                LogEntity log = new LogEntity();
                log.setMessage(e.getMessage());
                log.setStackTrace(getStackTraceAsString(e));
                log.setLogType(LogTypeEnum.SQL_EXCEPTION);
                log.setHappenTime(LocalDateTime.now());
                log.setGroupGuid(GuidHolder.getHolder());
                log.setPageId(GuidHolder.getPageIdHolder());
                log.setTraceId(GuidHolder.getTraceIdHolder());
                log.setParentGuid(GuidHolder.getParentHolder());
                log.setParentHttpId(GuidHolder.getParentHttpHolder());
                log.setHttpId(operationId);

                PostHelper.processLog(log);
            } catch (Exception ex) {
                // 忽略
            }
        }

        private String getStackTraceAsString(Exception e) {
            StringBuilder sb = new StringBuilder();
            sb.append(e.toString()).append("\n");
            for (StackTraceElement element : e.getStackTrace()) {
                sb.append("  at ").append(element.toString()).append("\n");
            }
            return sb.toString();
        }
    }
}