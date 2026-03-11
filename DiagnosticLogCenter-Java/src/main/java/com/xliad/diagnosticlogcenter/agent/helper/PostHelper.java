package com.xliad.diagnosticlogcenter.agent.helper;

import com.xliad.diagnosticlogcenter.abstracts.LogEntity;
import com.xliad.diagnosticlogcenter.grpc.*;
import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;
import io.grpc.StatusRuntimeException;

import java.time.Instant;
import java.time.LocalDateTime;
import java.time.ZoneId;
import java.util.concurrent.ConcurrentLinkedQueue;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;

public class PostHelper {
    private static String address;
    private static String clientName;
    private static String envName;
    private static int timeoutBySeconds;

    private static final ConcurrentLinkedQueue<LogDtoItem> logContainer = new ConcurrentLinkedQueue<>();
    private static final ScheduledExecutorService scheduler = Executors.newSingleThreadScheduledExecutor();
    private static final LocalCacheHelper localCacheHelper = new LocalCacheHelper();
    private static final Object clearLock = new Object();

    // gRPC 相关
    private static ManagedChannel channel;
    private static DiaglogerGrpc.DiaglogerBlockingStub blockingStub;

    public static void init(String _address, String _clientName, String _envName, int _timeoutBySeconds) {
        address = _address;
        clientName = _clientName;
        envName = _envName;
        timeoutBySeconds = _timeoutBySeconds;

        // 初始化 gRPC 连接
        initGrpcChannel();

        System.out.println("=== DiagnosticLogCenter Initialized ===");
        System.out.println("Collect Server Address: " + address);
        System.out.println("Client Name: " + clientName);
        System.out.println("Environment: " + envName);
        System.out.println("Timeout: " + timeoutBySeconds + "s");
        scheduler.scheduleAtFixedRate(PostHelper::timerElapsed, 3, 3, TimeUnit.SECONDS);
    }

    private static void initGrpcChannel() {
        try {
            // 解析地址（格式：host:port）
            String[] parts = address.split(":");
            String host = parts[0];
            int port = parts.length > 1 ? Integer.parseInt(parts[1]) : 50051;

            // 创建 gRPC 通道
            channel = ManagedChannelBuilder.forAddress(host, port)
                    .usePlaintext()  // .NET 版本使用 ChannelCredentials.Insecure，对应 Java 的 usePlaintext
                    .keepAliveTime(30, TimeUnit.SECONDS)
                    .keepAliveTimeout(10, TimeUnit.SECONDS)
                    .keepAliveWithoutCalls(true)  // 即使没有调用也保持连接
                    .idleTimeout(60, TimeUnit.SECONDS)
                    .maxInboundMessageSize(10 * 1024 * 1024)
                    .build();

            // 创建阻塞式存根
//            blockingStub = DiaglogerGrpc.newBlockingStub(channel)
//                    .withDeadlineAfter(timeoutBySeconds, TimeUnit.SECONDS);

            System.out.println("gRPC channel initialized for: " + address);
        } catch (Exception e) {
            System.err.println("Failed to initialize gRPC channel: " + e.getMessage());
            e.printStackTrace();
        }
    }

    private static void timerElapsed() {
        LogDto dto = LogDto.newBuilder().build();
        LogDtoItem item;
        while ((item = logContainer.poll()) != null) {
            dto = dto.toBuilder().addItems(item).build();
        }

        boolean shouldClearCacheLogs = false;
        if (dto.getItemsList().isEmpty()) {
            shouldClearCacheLogs = true;
        } else {
            try {
                // 发送日志到收集服务器
                sendLogs(dto);
                shouldClearCacheLogs = true;
            } catch (Exception ex) {
                System.out.println("DiagnosticLogCenter: An error occurred while sending logs:");
                ex.printStackTrace();
                if (localCacheHelper.writeLog(dto)) {
                    System.out.println("DiagnosticLogCenter: Component has cached logs locally.");
                }
            }
        }

        if (shouldClearCacheLogs) {
            synchronized (clearLock) {
                try {
                    LocalCacheHelper.CacheResult result = localCacheHelper.peekClearLog();
                    if (result != null && result.getLogs() != null && !result.getLogs().isEmpty()) {
                        for (LogDto log : result.getLogs()) {
                            sendLogs(log);
                        }
                        result.getAction().run();
                    }
                } catch (Exception ex) {
                    ex.printStackTrace();
                }
            }
        }
    }

    private static void sendLogs(LogDto dto) {
        int maxRetries = 3;
        int retryCount = 0;
        long baseDelay = 1000; // 1秒

        while (retryCount < maxRetries) {
            try {
                if (channel == null || channel.isShutdown()) {
                    initGrpcChannel();
                }

                // 创建带 deadline 的 stub
                DiaglogerGrpc.DiaglogerBlockingStub callStub = DiaglogerGrpc.newBlockingStub(channel)
                        .withDeadlineAfter(timeoutBySeconds, TimeUnit.SECONDS);

                LogReply reply = callStub.postLog(dto);

                if (!reply.getSuccess()) {
                    throw new RuntimeException("Server returned error: " + reply.getMessage());
                }

                // 发送成功，退出循环
                System.out.println("Logs sent successfully on attempt " + (retryCount + 1));
                return;

            } catch (StatusRuntimeException e) {
                retryCount++;

                // 如果是 deadline 相关错误，增加延迟重试
                if (e.getStatus().getCode() == io.grpc.Status.Code.DEADLINE_EXCEEDED) {
                    System.err.println("Deadline exceeded, retry " + retryCount + "/" + maxRetries);

                    if (retryCount < maxRetries) {
                        try {
                            // 指数退避
                            long delay = baseDelay * (long) Math.pow(2, retryCount - 1);
                            Thread.sleep(delay);
                        } catch (InterruptedException ie) {
                            Thread.currentThread().interrupt();
                            break;
                        }
                    } else {
                        System.err.println("Max retries reached, giving up");
                        throw new RuntimeException("Failed to send logs after " + maxRetries + " retries", e);
                    }
                } else {
                    // 其他类型的错误，直接抛出
                    System.err.println("gRPC call failed: " + e.getStatus());
                    throw new RuntimeException("Failed to send logs via gRPC", e);
                }
            }
        }
    }

    public static void processLog(LogEntity logEntity) {
        if (logEntity == null) {
            System.out.println("WARN: processLog called with null entity");
            return;
        }

        // 设置客户端和环境信息
        logEntity.setClientName(clientName);
        logEntity.setEnvironmentName(envName);
        // 获取原始时间（LocalDateTime）
        LocalDateTime happenTime = logEntity.getHappenTime();
        // 假设这个时间是东八区时间，转换为时间戳
        // 方法1：使用系统默认时区
        long timestamp = happenTime
                .atZone(ZoneId.of("Asia/Shanghai"))  // 指定这是东八区时间
                .toInstant()                          // 转换为UTC时间戳（会自动减去8小时）
                .toEpochMilli();
        // 验证
        System.out.println("=== Time Conversion ===");
        System.out.println("Original LocalDateTime (Asia/Shanghai): " + happenTime);
        System.out.println("Timestamp: " + timestamp);
        System.out.println("Converted back to Asia/Shanghai: " +
                Instant.ofEpochMilli(timestamp).atZone(ZoneId.of("Asia/Shanghai")).toLocalDateTime());

        // 构建 LogDtoItem
        LogDtoItem logDtoItem = LogDtoItem.newBuilder()
                .setClassName(nullToEmpty(logEntity.getClassName()))
                .setCommandText(nullToEmpty(logEntity.getCommandText()))
                .setClientName(nullToEmpty(logEntity.getClientName()))
                .setDatabase(nullToEmpty(logEntity.getDatabase()))
                .setDataSource(nullToEmpty(logEntity.getDataSource()))
                .setEnvironmentName(nullToEmpty(logEntity.getEnvironmentName()))
                .setGroupGuid(nullToEmpty(logEntity.getGroupGuid()))
                .setHappenTime(timestamp)
                .setIp(nullToEmpty(logEntity.getIp()))
                .setLevel(logEntity.getLevel() != null ? logEntity.getLevel().getValue() : 0)
                .setLogType(logEntity.getLogType() != null ? logEntity.getLogType().getValue() : 0)
                .setMessage(nullToEmpty(logEntity.getMessage()))
                .setMethodName(nullToEmpty(logEntity.getMethodName()))
                .setParameters(nullToEmpty(logEntity.getParameters()))
                .setStackTrace(nullToEmpty(logEntity.getStackTrace()))
                .setStatuCode(logEntity.getStatusCode())
                .setPageId(nullToEmpty(logEntity.getPageId()))
                .setTraceId(nullToEmpty(logEntity.getTraceId()))
                .setParentGuid(nullToEmpty(logEntity.getParentGuid()))
                .setHttpId(nullToEmpty(logEntity.getHttpId()))
                .setParentHttpId(nullToEmpty(logEntity.getParentHttpId()))
                .build();

        logContainer.offer(logDtoItem);
    }

    private static String nullToEmpty(String str) {
        return str != null ? str : "";
    }

    // 添加关闭方法，用于优雅关闭
    public static void shutdown() {
        if (channel != null) {
            try {
                channel.shutdown().awaitTermination(5, TimeUnit.SECONDS);
            } catch (InterruptedException e) {
                Thread.currentThread().interrupt();
            }
        }
        scheduler.shutdown();
        try {
            scheduler.awaitTermination(5, TimeUnit.SECONDS);
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
        }
    }
}