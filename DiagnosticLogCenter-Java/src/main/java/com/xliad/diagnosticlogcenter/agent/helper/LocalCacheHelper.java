package com.xliad.diagnosticlogcenter.agent.helper;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.xliad.diagnosticlogcenter.grpc.LogDto;
import com.xliad.diagnosticlogcenter.grpc.LogDtoItem;
import java.io.*;
import java.nio.file.*;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.*;
import java.util.concurrent.locks.ReentrantLock;

public class LocalCacheHelper {
    private String realSavePath;
    private static final String TRY_FILE_NAME = "try.txt";
    private static final Object WRITE_LOCK = new Object();
    private static final ObjectMapper objectMapper = new ObjectMapper();

    public LocalCacheHelper(String savePath) {
        boolean success = false;
        if (savePath != null && !savePath.isEmpty()) {
            success = trySavePath(savePath);
        }
        if (!success) {
            success = trySavePath(Paths.get(System.getProperty("user.dir"), "DlcCache").toString());
        }
        if (!success) {
            success = trySavePath("C:\\DlcCache");
        }
        if (!success) {
            success = trySavePath("/DlcCache");
        }
    }

    public LocalCacheHelper() {
        this(null);
    }

    private boolean trySavePath(String path) {
        try {
            Path dirPath = Paths.get(path);
            if (!Files.exists(dirPath)) {
                Files.createDirectories(dirPath);
            }
        } catch (Exception e) {
            return false;
        }

        Path filePath = Paths.get(path, TRY_FILE_NAME);
        synchronized (WRITE_LOCK) {
            try {
                // 尝试写入文件
                Files.write(filePath,
                        LocalDateTime.now().format(DateTimeFormatter.ISO_LOCAL_DATE_TIME).getBytes());
                // 尝试删除文件
                Files.deleteIfExists(filePath);
                if (Files.exists(filePath)) {
                    return false;
                }
                realSavePath = path;
                return true;
            } catch (Exception e) {
                System.out.println("Dlc在检查 " + path + " 位置是否可用时发生异常：" + e.getMessage());
                return false;
            }
        }
    }

    public boolean writeLog(LogDto log) {
        if (realSavePath == null || realSavePath.isEmpty()) {
            return false;
        }

        String fileName = LocalDateTime.now().format(DateTimeFormatter.ofPattern("yyyy-MM-dd_HH")) + ".dlc";
        Path filePath = Paths.get(realSavePath, fileName);

        try {
            String json = objectMapper.writeValueAsString(log) + System.lineSeparator();
            Files.write(filePath, json.getBytes(),
                    StandardOpenOption.CREATE, StandardOpenOption.APPEND);
            return true;
        } catch (Exception e) {
            return false;
        }
    }

    public CacheResult peekClearLog() {
        if (realSavePath == null || realSavePath.isEmpty()) {
            return null;
        }

        try {
            Path dir = Paths.get(realSavePath);
            Optional<Path> oldestFile = Files.list(dir)
                    .filter(p -> p.toString().endsWith(".dlc"))
                    .min((p1, p2) -> {
                        try {
                            return Files.getLastModifiedTime(p1).compareTo(Files.getLastModifiedTime(p2));
                        } catch (IOException e) {
                            return 0;
                        }
                    });

            if (oldestFile.isPresent()) {
                Path file = oldestFile.get();
                List<LogDto> result = new ArrayList<>();

                List<String> lines = Files.readAllLines(file);
                for (String line : lines) {
                    if (line != null && !line.trim().isEmpty()) {
                        result.add(objectMapper.readValue(line, LogDto.class));
                    }
                }

                Runnable action = () -> {
                    try {
                        Files.deleteIfExists(file);
                    } catch (IOException e) {
                        e.printStackTrace();
                    }
                };

                return new CacheResult(result, action);
            }
        } catch (Exception e) {
            e.printStackTrace();
        }

        return null;
    }

    public static class CacheResult {
        private final List<LogDto> logs;
        private final Runnable action;

        public CacheResult(List<LogDto> logs, Runnable action) {
            this.logs = logs;
            this.action = action;
        }

        public List<LogDto> getLogs() {
            return logs;
        }

        public Runnable getAction() {
            return action;
        }
    }
}