package com.xliad.diagnosticlogcenter.agent.core;

import com.xliad.diagnosticlogcenter.agent.GuidHolder;

import java.util.Collection;
import java.util.List;
import java.util.concurrent.*;

/**
 * 线程池装饰器，用于传递链路上下文
 * 类似于 SkyWalking 的线程池插件
 */
public class TraceContextPropagator {

    public static Runnable wrap(Runnable task) {
        String holder = GuidHolder.getHolder();
        String traceId = GuidHolder.getTraceIdHolder();
        String pageId = GuidHolder.getPageIdHolder();
        String parentGuid = GuidHolder.getParentHolder();
        String parentHttp = GuidHolder.getParentHttpHolder();

        return () -> {
            String oldHolder = GuidHolder.getHolder();
            String oldTraceId = GuidHolder.getTraceIdHolder();
            String oldPageId = GuidHolder.getPageIdHolder();
            String oldParentGuid = GuidHolder.getParentHolder();
            String oldParentHttp = GuidHolder.getParentHttpHolder();

            try {
                GuidHolder.setHolder(holder);
                GuidHolder.setTraceIdHolder(traceId);
                GuidHolder.setPageIdHolder(pageId);
                GuidHolder.setParentHolder(parentGuid);
                GuidHolder.setParentHttpHolder(parentHttp);

                task.run();
            } finally {
                GuidHolder.setHolder(oldHolder);
                GuidHolder.setTraceIdHolder(oldTraceId);
                GuidHolder.setPageIdHolder(oldPageId);
                GuidHolder.setParentHolder(oldParentGuid);
                GuidHolder.setParentHttpHolder(oldParentHttp);
            }
        };
    }

    public static <T> Callable<T> wrap(Callable<T> task) {
        String holder = GuidHolder.getHolder();
        String traceId = GuidHolder.getTraceIdHolder();
        String pageId = GuidHolder.getPageIdHolder();
        String parentGuid = GuidHolder.getParentHolder();
        String parentHttp = GuidHolder.getParentHttpHolder();

        return () -> {
            String oldHolder = GuidHolder.getHolder();
            String oldTraceId = GuidHolder.getTraceIdHolder();
            String oldPageId = GuidHolder.getPageIdHolder();
            String oldParentGuid = GuidHolder.getParentHolder();
            String oldParentHttp = GuidHolder.getParentHttpHolder();

            try {
                GuidHolder.setHolder(holder);
                GuidHolder.setTraceIdHolder(traceId);
                GuidHolder.setPageIdHolder(pageId);
                GuidHolder.setParentHolder(parentGuid);
                GuidHolder.setParentHttpHolder(parentHttp);

                return task.call();
            } finally {
                GuidHolder.setHolder(oldHolder);
                GuidHolder.setTraceIdHolder(oldTraceId);
                GuidHolder.setPageIdHolder(oldPageId);
                GuidHolder.setParentHolder(oldParentGuid);
                GuidHolder.setParentHttpHolder(oldParentHttp);
            }
        };
    }

    public static ExecutorService wrap(ExecutorService executor) {
        return new ExecutorServiceWrapper(executor);
    }

    private static class ExecutorServiceWrapper implements ExecutorService {
        private final ExecutorService delegate;

        public ExecutorServiceWrapper(ExecutorService delegate) {
            this.delegate = delegate;
        }

        @Override
        public void execute(Runnable command) {
            delegate.execute(wrap(command));
        }

        @Override
        public Future<?> submit(Runnable task) {
            return delegate.submit(wrap(task));
        }

        @Override
        public <T> Future<T> submit(Runnable task, T result) {
            return delegate.submit(wrap(task), result);
        }

        @Override
        public <T> Future<T> submit(Callable<T> task) {
            return delegate.submit(wrap(task));
        }

        // 其他方法委托...
        @Override
        public void shutdown() { delegate.shutdown(); }
        @Override
        public List<Runnable> shutdownNow() { return delegate.shutdownNow(); }
        @Override
        public boolean isShutdown() { return delegate.isShutdown(); }
        @Override
        public boolean isTerminated() { return delegate.isTerminated(); }
        @Override
        public boolean awaitTermination(long timeout, TimeUnit unit) throws InterruptedException {
            return delegate.awaitTermination(timeout, unit);
        }
        @Override
        public <T> List<Future<T>> invokeAll(Collection<? extends Callable<T>> tasks) throws InterruptedException {
            return delegate.invokeAll(tasks);
        }
        @Override
        public <T> List<Future<T>> invokeAll(Collection<? extends Callable<T>> tasks, long timeout, TimeUnit unit) throws InterruptedException {
            return delegate.invokeAll(tasks, timeout, unit);
        }
        @Override
        public <T> T invokeAny(Collection<? extends Callable<T>> tasks) throws InterruptedException, ExecutionException {
            return delegate.invokeAny(tasks);
        }
        @Override
        public <T> T invokeAny(Collection<? extends Callable<T>> tasks, long timeout, TimeUnit unit) throws InterruptedException, ExecutionException, TimeoutException {
            return delegate.invokeAny(tasks, timeout, unit);
        }
    }
}