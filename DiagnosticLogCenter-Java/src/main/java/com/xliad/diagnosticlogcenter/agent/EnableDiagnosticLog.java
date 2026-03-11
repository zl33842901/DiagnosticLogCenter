package com.xliad.diagnosticlogcenter.agent;

import com.xliad.diagnosticlogcenter.agent.config.DiagnosticLogAutoConfiguration;
import org.springframework.context.annotation.Import;

import java.lang.annotation.*;

/**
 * 启用诊断日志跟踪功能
 * 在 Spring Boot 启动类上添加此注解即可启用
 */
@Target(ElementType.TYPE)
@Retention(RetentionPolicy.RUNTIME)
@Documented
@Import(DiagnosticLogAutoConfiguration.class)
public @interface EnableDiagnosticLog {
}