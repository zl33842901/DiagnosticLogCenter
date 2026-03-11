package com.xliad.diagnosticlogcenter.agent.diagnosticprocessors;

import com.xliad.diagnosticlogcenter.abstracts.LogEntity;
import com.xliad.diagnosticlogcenter.abstracts.LogTypeEnum;
import com.xliad.diagnosticlogcenter.agent.GuidHolder;
import com.xliad.diagnosticlogcenter.agent.helper.PostHelper;
import org.aspectj.lang.ProceedingJoinPoint;
import org.aspectj.lang.annotation.Around;
import org.aspectj.lang.annotation.Aspect;
import org.aspectj.lang.annotation.Pointcut;
import org.springframework.stereotype.Component;

import java.time.LocalDateTime;
import java.util.UUID;

/**
 * Spring AOP 方法拦截
 * 类似于 SkyWalking 的 Spring 插件
 */
@Aspect
@Component
public class SpringMethodDiagnosticProcessor implements ITracingDiagnosticProcessor {

    @Override
    public String getListenerName() {
        return "spring-method";
    }

    @Pointcut("@within(org.springframework.stereotype.Service) || " +
            "@within(org.springframework.stereotype.Repository) || " +
            "@within(org.springframework.stereotype.Component)")
    public void beanMethods() {}

    @Around("beanMethods()")
    public Object aroundMethod(ProceedingJoinPoint pjp) throws Throwable {
        if (GuidHolder.getHolder() == null) {
            return pjp.proceed();
        }

        String className = pjp.getTarget().getClass().getSimpleName();
        String methodName = pjp.getSignature().getName();
        String operationId = UUID.randomUUID().toString();

        // 记录方法进入
        LogEntity entryLog = createMethodLog(className, methodName, operationId);
        entryLog.setLogType(LogTypeEnum.METHOD_ENTRY);
        PostHelper.processLog(entryLog);

        try {
            Object result = pjp.proceed();

            // 记录方法退出
            LogEntity leaveLog = createMethodLog(className, methodName, operationId);
            leaveLog.setLogType(LogTypeEnum.METHOD_LEAVE);
            PostHelper.processLog(leaveLog);

            return result;

        } catch (Exception e) {
            // 记录方法异常
            LogEntity errorLog = createMethodLog(className, methodName, operationId);
            errorLog.setLogType(LogTypeEnum.METHOD_EXCEPTION);
            errorLog.setMessage(e.getMessage());
            errorLog.setStackTrace(getStackTraceAsString(e));
            PostHelper.processLog(errorLog);

            throw e;
        }
    }

    private LogEntity createMethodLog(String className, String methodName, String operationId) {
        LogEntity log = new LogEntity();
        log.setClassName(className);
        log.setMethodName(methodName);
        log.setGroupGuid(GuidHolder.getHolder());
        log.setHappenTime(LocalDateTime.now());
        log.setPageId(GuidHolder.getPageIdHolder());
        log.setTraceId(GuidHolder.getTraceIdHolder());
        log.setParentGuid(GuidHolder.getParentHolder());
        log.setParentHttpId(GuidHolder.getParentHttpHolder());
        log.setHttpId(operationId);

        return log;
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