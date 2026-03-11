package com.xliad.diagnosticlogcenter.agent.diagnosticprocessors;

import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.nio.charset.StandardCharsets;

public class RepeatableHttpContent {
    private final byte[] content;
    private final String contentType;

    public RepeatableHttpContent(byte[] content, String contentType) {
        this.content = content != null ? content : new byte[0];
        this.contentType = contentType;
    }

    public byte[] getContent() {
        return content;
    }

    public String getContentType() {
        return contentType;
    }

    public InputStream getContentStream() {
        return new ByteArrayInputStream(content);
    }

    public String getContentAsString() {
        return new String(content, StandardCharsets.UTF_8);
    }

    public long getContentLength() {
        return content.length;
    }
}