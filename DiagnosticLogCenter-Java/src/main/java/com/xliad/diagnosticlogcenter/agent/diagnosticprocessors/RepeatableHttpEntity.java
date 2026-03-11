package com.xliad.diagnosticlogcenter.agent.diagnosticprocessors;

import org.apache.hc.core5.http.ContentType;
import org.apache.hc.core5.http.HttpEntity;
import org.apache.hc.core5.http.io.entity.AbstractHttpEntity;

import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

/**
 * 可重复读取的 HttpEntity
 * 对应 .NET 版本的 RepeatableHttpContent
 */
public class RepeatableHttpEntity extends AbstractHttpEntity {

    private final byte[] content;
    private final ContentType contentType;
    private final String contentEncoding;

    public RepeatableHttpEntity(HttpEntity originalEntity) throws IOException {
        // 调用父类构造器，传入contentType和contentEncoding
        super(
                originalEntity.getContentType(),
                originalEntity.getContentEncoding(),
                originalEntity.isChunked()
        );

        // 保存原始内容类型和编码
        String originalContentType = originalEntity.getContentType();
        this.contentType = originalContentType != null ? ContentType.parse(originalContentType) : null;
        this.contentEncoding = originalEntity.getContentEncoding();

        // 读取原始内容到字节数组
        try (InputStream inputStream = originalEntity.getContent()) {
            this.content = inputStream.readAllBytes();
        }
    }

    @Override
    public boolean isRepeatable() {
        return true;
    }

    @Override
    public InputStream getContent() throws IOException {
        return new ByteArrayInputStream(content);
    }

    @Override
    public void writeTo(OutputStream outStream) throws IOException {
        outStream.write(content);
    }

    @Override
    public boolean isStreaming() {
        return false;
    }

    @Override
    public long getContentLength() {
        return content.length;
    }


    @Override
    public void close() throws IOException {
        // 不需要关闭
    }

    // 获取原始内容（用于日志记录）
    public byte[] getContentBytes() {
        return content;
    }

    public String getContentAsString() {
        return new String(content, java.nio.charset.StandardCharsets.UTF_8);
    }
}