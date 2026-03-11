package com.xliad.diagnosticlogcenter.agent.logger;

import com.xliad.diagnosticlogcenter.abstracts.LogEntity;
import com.xliad.diagnosticlogcenter.abstracts.LogTypeEnum;
import com.xliad.diagnosticlogcenter.agent.GuidHolder;
import com.xliad.diagnosticlogcenter.agent.helper.PostHelper;
import org.slf4j.Logger;
import org.slf4j.Marker;
import java.time.LocalDateTime;

public class DiagnosticLogCenterLogger implements Logger {

    private final org.slf4j.event.Level logLevel;

    public DiagnosticLogCenterLogger(org.slf4j.event.Level logLevel) {
        this.logLevel = logLevel;
    }

    @Override
    public String getName() {
        return "DiagnosticLogCenter";
    }

    @Override
    public boolean isTraceEnabled() {
        return logLevel.compareTo(org.slf4j.event.Level.TRACE) <= 0;
    }

    @Override
    public void trace(String msg) {
        if (isTraceEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.TRACE, msg, null);
        }
    }

    @Override
    public void trace(String format, Object arg) {
        if (isTraceEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.TRACE, String.format(format, arg), null);
        }
    }

    @Override
    public void trace(String format, Object arg1, Object arg2) {
        if (isTraceEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.TRACE, String.format(format, arg1, arg2), null);
        }
    }

    @Override
    public void trace(String format, Object... arguments) {
        if (isTraceEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.TRACE, String.format(format, arguments), null);
        }
    }

    @Override
    public void trace(String msg, Throwable t) {
        if (isTraceEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.TRACE, msg, t);
        }
    }

    @Override
    public boolean isTraceEnabled(Marker marker) {
        return isTraceEnabled();
    }

    @Override
    public void trace(Marker marker, String msg) {
        trace(msg);
    }

    @Override
    public void trace(Marker marker, String format, Object arg) {
        trace(format, arg);
    }

    @Override
    public void trace(Marker marker, String format, Object arg1, Object arg2) {
        trace(format, arg1, arg2);
    }

    @Override
    public void trace(Marker marker, String format, Object... argArray) {
        trace(format, argArray);
    }

    @Override
    public void trace(Marker marker, String msg, Throwable t) {
        trace(msg, t);
    }

    @Override
    public boolean isDebugEnabled() {
        return logLevel.compareTo(org.slf4j.event.Level.DEBUG) <= 0;
    }

    @Override
    public void debug(String msg) {
        if (isDebugEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.DEBUG, msg, null);
        }
    }

    @Override
    public void debug(String format, Object arg) {
        if (isDebugEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.DEBUG, String.format(format, arg), null);
        }
    }

    @Override
    public void debug(String format, Object arg1, Object arg2) {
        if (isDebugEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.DEBUG, String.format(format, arg1, arg2), null);
        }
    }

    @Override
    public void debug(String format, Object... arguments) {
        if (isDebugEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.DEBUG, String.format(format, arguments), null);
        }
    }

    @Override
    public void debug(String msg, Throwable t) {
        if (isDebugEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.DEBUG, msg, t);
        }
    }

    @Override
    public boolean isDebugEnabled(Marker marker) {
        return isDebugEnabled();
    }

    @Override
    public void debug(Marker marker, String msg) {
        debug(msg);
    }

    @Override
    public void debug(Marker marker, String format, Object arg) {
        debug(format, arg);
    }

    @Override
    public void debug(Marker marker, String format, Object arg1, Object arg2) {
        debug(format, arg1, arg2);
    }

    @Override
    public void debug(Marker marker, String format, Object... arguments) {
        debug(format, arguments);
    }

    @Override
    public void debug(Marker marker, String msg, Throwable t) {
        debug(msg, t);
    }

    @Override
    public boolean isInfoEnabled() {
        return logLevel.compareTo(org.slf4j.event.Level.INFO) <= 0;
    }

    @Override
    public void info(String msg) {
        if (isInfoEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.INFO, msg, null);
        }
    }

    @Override
    public void info(String format, Object arg) {
        if (isInfoEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.INFO, String.format(format, arg), null);
        }
    }

    @Override
    public void info(String format, Object arg1, Object arg2) {
        if (isInfoEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.INFO, String.format(format, arg1, arg2), null);
        }
    }

    @Override
    public void info(String format, Object... arguments) {
        if (isInfoEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.INFO, String.format(format, arguments), null);
        }
    }

    @Override
    public void info(String msg, Throwable t) {
        if (isInfoEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.INFO, msg, t);
        }
    }

    @Override
    public boolean isInfoEnabled(Marker marker) {
        return isInfoEnabled();
    }

    @Override
    public void info(Marker marker, String msg) {
        info(msg);
    }

    @Override
    public void info(Marker marker, String format, Object arg) {
        info(format, arg);
    }

    @Override
    public void info(Marker marker, String format, Object arg1, Object arg2) {
        info(format, arg1, arg2);
    }

    @Override
    public void info(Marker marker, String format, Object... arguments) {
        info(format, arguments);
    }

    @Override
    public void info(Marker marker, String msg, Throwable t) {
        info(msg, t);
    }

    @Override
    public boolean isWarnEnabled() {
        return logLevel.compareTo(org.slf4j.event.Level.WARN) <= 0;
    }

    @Override
    public void warn(String msg) {
        if (isWarnEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.WARN, msg, null);
        }
    }

    @Override
    public void warn(String format, Object arg) {
        if (isWarnEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.WARN, String.format(format, arg), null);
        }
    }

    @Override
    public void warn(String format, Object... arguments) {
        if (isWarnEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.WARN, String.format(format, arguments), null);
        }
    }

    @Override
    public void warn(String format, Object arg1, Object arg2) {
        if (isWarnEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.WARN, String.format(format, arg1, arg2), null);
        }
    }

    @Override
    public void warn(String msg, Throwable t) {
        if (isWarnEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.WARN, msg, t);
        }
    }

    @Override
    public boolean isWarnEnabled(Marker marker) {
        return isWarnEnabled();
    }

    @Override
    public void warn(Marker marker, String msg) {
        warn(msg);
    }

    @Override
    public void warn(Marker marker, String format, Object arg) {
        warn(format, arg);
    }

    @Override
    public void warn(Marker marker, String format, Object arg1, Object arg2) {
        warn(format, arg1, arg2);
    }

    @Override
    public void warn(Marker marker, String format, Object... arguments) {
        warn(format, arguments);
    }

    @Override
    public void warn(Marker marker, String msg, Throwable t) {
        warn(msg, t);
    }

    @Override
    public boolean isErrorEnabled() {
        return logLevel.compareTo(org.slf4j.event.Level.ERROR) <= 0;
    }

    @Override
    public void error(String msg) {
        if (isErrorEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.ERROR, msg, null);
        }
    }

    @Override
    public void error(String format, Object arg) {
        if (isErrorEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.ERROR, String.format(format, arg), null);
        }
    }

    @Override
    public void error(String format, Object arg1, Object arg2) {
        if (isErrorEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.ERROR, String.format(format, arg1, arg2), null);
        }
    }

    @Override
    public void error(String format, Object... arguments) {
        if (isErrorEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.ERROR, String.format(format, arguments), null);
        }
    }

    @Override
    public void error(String msg, Throwable t) {
        if (isErrorEnabled() && GuidHolder.getHolder() != null) {
            log(org.slf4j.event.Level.ERROR, msg, t);
        }
    }

    @Override
    public boolean isErrorEnabled(Marker marker) {
        return isErrorEnabled();
    }

    @Override
    public void error(Marker marker, String msg) {
        error(msg);
    }

    @Override
    public void error(Marker marker, String format, Object arg) {
        error(format, arg);
    }

    @Override
    public void error(Marker marker, String format, Object arg1, Object arg2) {
        error(format, arg1, arg2);
    }

    @Override
    public void error(Marker marker, String format, Object... arguments) {
        error(format, arguments);
    }

    @Override
    public void error(Marker marker, String msg, Throwable t) {
        error(msg, t);
    }

    private void log(org.slf4j.event.Level level, String message, Throwable t) {
        LogEntity log = new LogEntity();
        log.setLogType(LogTypeEnum.METHOD_ADDITION);
        log.setClassName("系统日志");
        log.setMethodName("");
        log.setGroupGuid(GuidHolder.getHolder());
        log.setHappenTime(LocalDateTime.now());
        log.setPageId(GuidHolder.getPageIdHolder());
        log.setTraceId(GuidHolder.getTraceIdHolder());
        log.setParentGuid(GuidHolder.getParentHolder());
        log.setParentHttpId(GuidHolder.getParentHttpHolder());

        if (t != null) {
            log.setMethodName(t.getMessage());
            StringBuilder sb = new StringBuilder();
            for (StackTraceElement element : t.getStackTrace()) {
                sb.append(element.toString()).append("\r\n");
            }
            log.setStackTrace(sb.toString());
        } else {
            log.setStackTrace(message);
        }

        PostHelper.processLog(log);
    }
}