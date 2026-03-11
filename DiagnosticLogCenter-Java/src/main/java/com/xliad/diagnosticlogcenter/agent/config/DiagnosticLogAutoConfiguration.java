package com.xliad.diagnosticlogcenter.agent.config;

import com.xliad.diagnosticlogcenter.agent.DiagnosticLogConfig;
import com.xliad.diagnosticlogcenter.agent.FilterRule;
import com.xliad.diagnosticlogcenter.agent.InstrumentationService;
import com.xliad.diagnosticlogcenter.agent.TracingDiagnosticProcessorObserver;
import com.xliad.diagnosticlogcenter.agent.diagnosticprocessors.*;
import com.xliad.diagnosticlogcenter.agent.helper.PostHelper;
import com.xliad.diagnosticlogcenter.agent.logger.DiagnosticLogCenterLoggerProvider;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.autoconfigure.condition.ConditionalOnClass;
import org.springframework.boot.autoconfigure.condition.ConditionalOnMissingBean;
import org.springframework.boot.autoconfigure.condition.ConditionalOnProperty;
import org.springframework.boot.context.properties.EnableConfigurationProperties;
import org.springframework.boot.web.servlet.FilterRegistrationBean;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.context.annotation.EnableAspectJAutoProxy;
import org.springframework.context.annotation.Primary;
import org.springframework.core.Ordered;
import org.springframework.core.env.Environment;
import org.springframework.web.client.RestTemplate;

import javax.sql.DataSource;
import java.util.Collections;

@Configuration
@EnableAspectJAutoProxy
@EnableConfigurationProperties(DiagnosticLogProperties.class)
@ConditionalOnProperty(prefix = "diagnostic.log", name = "enable", havingValue = "true", matchIfMissing = true)
public class DiagnosticLogAutoConfiguration {

    @Autowired
    private DiagnosticLogProperties properties;

    @Bean
    @ConditionalOnMissingBean
    public DiagnosticLogConfig diagnosticLogConfig() {
        DiagnosticLogConfig config = new DiagnosticLogConfig();
        config.setEnable(properties.isEnable());
        config.setEnableAspNetCore(properties.isEnableAspNetCore());
        config.setEnableHttpClient(properties.isEnableHttpClient());
        config.setEnableSqlClient(properties.isEnableSqlClient());
        config.setEnableMethod(properties.isEnableMethod());
        config.setEnableSystemLog(properties.isEnableSystemLog());
        config.setRecordSqlParameters(properties.isRecordSqlParameters());
        config.setRecordHttpClientBody(properties.isRecordHttpClientBody());
        config.setAllowPath(properties.getAllowPath());
        config.setForbiddenPath(properties.getForbiddenPath());
        config.setCollectServerAddress(properties.getCollectServerAddress());
        config.setClientName(properties.getClientName());
        config.setEnvName(properties.getEnvName());
        config.setTimeoutBySecond(properties.getTimeoutBySecond());
        config.setRecordHttpClientRequestBodyMax(properties.getRecordHttpClientRequestBodyMax());
        config.setRecordHttpClientResponseBodyMax(properties.getRecordHttpClientResponseBodyMax());
        config.setRecordHttpClientFullWhenJson(properties.isRecordHttpClientFullWhenJson());

        DiagnosticLogConfig.setConfig(config);

        // 初始化FilterRule
        FilterRule.setAllowPath(properties.getAllowPath());
        FilterRule.setForbiddenPath(properties.getForbiddenPath());

        // 初始化PostHelper
        PostHelper.init(
                properties.getCollectServerAddress(),
                properties.getClientName(),
                properties.getEnvName(),
                properties.getTimeoutBySecond()
        );

        return config;
    }

    @Bean
    @ConditionalOnProperty(prefix = "diagnostic.log", name = "enable-asp-net-core", havingValue = "true", matchIfMissing = true)
    public FilterRegistrationBean<HttpServletDiagnosticProcessor> servletFilter(
            HttpServletDiagnosticProcessor processor) {
        System.out.println("=== 注册 Servlet Filter ===");
        System.out.println("Processor class: " + processor.getClass().getName());
        System.out.println("Processor hash: " + System.identityHashCode(processor));
        FilterRegistrationBean<HttpServletDiagnosticProcessor> registration = new FilterRegistrationBean<>();
        registration.setFilter(processor);
        registration.setOrder(Ordered.HIGHEST_PRECEDENCE);
        registration.setUrlPatterns(Collections.singletonList("/*"));
        registration.setName("diagnosticLogFilter");
        System.out.println("FilterRegistrationBean created:");
        System.out.println("  Order: " + registration.getOrder());
        System.out.println("  URL Patterns: " + registration.getUrlPatterns());
        return registration;
    }

    @Bean
    @ConditionalOnMissingBean
    public HttpServletDiagnosticProcessor httpServletDiagnosticProcessor() {
        System.out.println("=== 创建 HttpServletDiagnosticProcessor Bean ===");
        HttpServletDiagnosticProcessor processor = new HttpServletDiagnosticProcessor();
        System.out.println("Processor created: " + processor);
        return processor;
    }

    @Bean
    @ConditionalOnClass(RestTemplate.class)
    @ConditionalOnMissingBean
    @ConditionalOnProperty(prefix = "diagnostic.log", name = "enable-httpclient", havingValue = "true", matchIfMissing = true)
    public HttpClientDiagnosticProcessor httpClientDiagnosticProcessor() {
        return new HttpClientDiagnosticProcessor();
    }

    @Bean
    @ConditionalOnMissingBean
    @ConditionalOnProperty(prefix = "diagnostic.log", name = "enable-method", havingValue = "true", matchIfMissing = true)
    public SpringMethodDiagnosticProcessor springMethodDiagnosticProcessor() {
        return new SpringMethodDiagnosticProcessor();
    }

    @Bean
    @ConditionalOnMissingBean
    public TracingDiagnosticProcessorObserver tracingDiagnosticProcessorObserver(
            java.util.List<ITracingDiagnosticProcessor> processors) {
        return new TracingDiagnosticProcessorObserver(processors);
    }

    @Bean
    @ConditionalOnMissingBean
    public InstrumentationService instrumentationService() {
        return new InstrumentationService();
    }

    @Bean
    @ConditionalOnMissingBean
    @ConditionalOnProperty(prefix = "diagnostic.log", name = "enable-system-log", havingValue = "true", matchIfMissing = true)
    public DiagnosticLogCenterLoggerProvider diagnosticLogCenterLoggerProvider(Environment environment) {
        return new DiagnosticLogCenterLoggerProvider(environment);
    }


    // 确保 SqlClientDiagnosticAspect 被创建
    @Bean
    @ConditionalOnProperty(prefix = "diagnostic.log", name = "enable-sqlclient", havingValue = "true", matchIfMissing = true)
    public SqlClientDiagnosticAspect sqlClientDiagnosticAspect() {
        return new SqlClientDiagnosticAspect();
    }
}