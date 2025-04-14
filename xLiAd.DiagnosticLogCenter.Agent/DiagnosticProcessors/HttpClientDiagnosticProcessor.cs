using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using xLiAd.DiagnosticLogCenter.Abstract;

namespace xLiAd.DiagnosticLogCenter.Agent.DiagnosticProcessors
{
    public class HttpClientDiagnosticProcessor : ITracingDiagnosticProcessor
    {
        public string ListenerName { get; } = "HttpHandlerDiagnosticListener";
        [DiagnosticName("System.Net.Http.Request")]
        public void HttpRequest([Property(Name = "Request")] HttpRequestMessage request, [Property(Name = "LoggingRequestId")] Guid requestId)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            if(!GuidHolder.PageIdHolder.Value.NullOrEmpty() && !request.Headers.Contains(AspNetCoreDiagnosticProcessor.PageIdName))
                request.Headers.TryAddWithoutValidation(AspNetCoreDiagnosticProcessor.PageIdName, GuidHolder.PageIdHolder.Value);
            if (!GuidHolder.TraceIdHolder.Value.NullOrEmpty() && !request.Headers.Contains(AspNetCoreDiagnosticProcessor.TraceIdName))
                request.Headers.TryAddWithoutValidation(AspNetCoreDiagnosticProcessor.TraceIdName, GuidHolder.TraceIdHolder.Value);
            if (!request.Headers.Contains(AspNetCoreDiagnosticProcessor.ParentGuidName))
                request.Headers.TryAddWithoutValidation(AspNetCoreDiagnosticProcessor.ParentGuidName, GuidHolder.Holder.Value.ToString());
            if (!request.Headers.Contains(AspNetCoreDiagnosticProcessor.ParentHttpIdName))
                request.Headers.TryAddWithoutValidation(AspNetCoreDiagnosticProcessor.ParentHttpIdName, requestId.ToString());

            var log = ToLog(request, requestId, true);
            log.LogType = LogTypeEnum.HttpClientRequest;
            Helper.PostHelper.ProcessLog(log);
        }

        private LogEntity ToLog(HttpRequestMessage request, Guid? requestId, bool processContent)
        {
            var uri = request.RequestUri.ToString();
            var host = request.RequestUri.Host;
            var method = request.Method.ToString();
            string content;
            if (processContent)
            {
                var bytes = request?.Content?.ReadAsByteArrayAsync().Result;
                if(bytes == null)
                {
                    content = null;
                }
                else
                {
                    var stream = new MemoryStream(bytes);
                    var reader = new StreamReader(stream, Encoding.UTF8, true);
                    string text = reader.ReadToEnd() ?? string.Empty;

                    // 如果读取后位置未移动到末尾，说明可能有无效字符
                    bool isBinary = stream.Position != stream.Length || text.Contains('\uFFFD');
                    if(isBinary)
                    {
                        content = "检测到二进制，将提取一些文本：\n";
                        try
                        {
                            content += ExtractPrintableCharacters(bytes);
                        }
                        catch { }
                    }
                    else
                    {
                        bool isjson = text != null && ((text.Trim().StartsWith("{") && text.Trim().EndsWith("}")) || (text.Trim().StartsWith("[") && text.Trim().EndsWith("]")));
                        if (!text.NullOrEmpty() && text.Length > DiagnosticLogConfig.Config.RecordHttpClientRequestBodyMax && (!isjson || !DiagnosticLogConfig.Config.RecordHttpClientFullWhenJson))
                            content = text.Substring(0, DiagnosticLogConfig.Config.RecordHttpClientRequestBodyMax);
                        else
                            content = text;
                    }
                }
            }
            else
            {
                content = null;
            }
            
            
            LogEntity log = new LogEntity()
            {
                Message = uri,
                StackTrace = content,
                MethodName = method,
                Ip = host,
                GroupGuid = GuidHolder.Holder.Value.ToString(),
                HappenTime = DateTime.Now,
                PageId = GuidHolder.PageIdHolder.Value,
                TraceId = GuidHolder.TraceIdHolder.Value,
                ParentGuid = GuidHolder.ParentHolder.Value,
                HttpId = requestId?.ToString(),
                ParentHttpId = GuidHolder.ParentHttpHolder.Value
            };
            return log;
        }
        // 从二进制数据中提取可打印字符（包括中文）
        private string ExtractPrintableCharacters(byte[] bytes, int length = 100)
        {
            var sb = new StringBuilder();
            int position = 0;

            while (position < bytes.Length && sb.Length < length)
            {
                // 尝试UTF-8字符
                if (TryReadUtf8Char(bytes, ref position, out char utf8Char))
                {
                    if (IsPrintable(utf8Char))
                        sb.Append(utf8Char);
                    continue;
                }

                // 尝试GB18030字符（中文）
                if (TryReadGb18030Char(bytes, ref position, out char gbChar))
                {
                    sb.Append(gbChar);
                    continue;
                }

                // ASCII可打印字符
                if (bytes[position] >= 32 && bytes[position] <= 126)
                {
                    sb.Append((char)bytes[position]);
                }
                position++;
            }

            return sb.ToString();
        }
        private bool TryReadUtf8Char(byte[] bytes, ref int pos, out char result)
        {
            // 简化版UTF-8解码逻辑
            if (pos >= bytes.Length) { result = '\0'; return false; }

            byte b1 = bytes[pos];
            if (b1 < 0x80)
            {
                result = (char)b1;
                pos++;
                return true;
            }

            // 处理2-4字节UTF-8（实际实现需要更完整）
            if (pos + 1 < bytes.Length && (b1 & 0xE0) == 0xC0)
            {
                result = (char)(((b1 & 0x1F) << 6) | (bytes[pos + 1] & 0x3F));
                pos += 2;
                return true;
            }

            result = '\0';
            return false;
        }

        private bool TryReadGb18030Char(byte[] bytes, ref int pos, out char result)
        {
            // 简化的GB18030双字节检测
            if (pos + 1 >= bytes.Length) { result = '\0'; return false; }

            byte b1 = bytes[pos], b2 = bytes[pos + 1];
            if (b1 >= 0x81 && b1 <= 0xFE && b2 >= 0x40 && b2 <= 0xFE)
            {
                try
                {
                    result = Encoding.GetEncoding("GB18030").GetChars(new[] { b1, b2 })[0];
                    pos += 2;
                    return true;
                }
                catch { }
            }

            result = '\0';
            return false;
        }

        private bool IsPrintable(char c)
        {
            // Unicode分类判断可打印字符
            var cat = CharUnicodeInfo.GetUnicodeCategory(c);
            return cat != UnicodeCategory.Control &&
                   cat != UnicodeCategory.Format &&
                   cat != UnicodeCategory.OtherNotAssigned;
        }

        [DiagnosticName("System.Net.Http.Response")]
        public void HttpResponse([Property(Name = "Response")] HttpResponseMessage response, [Property(Name = "LoggingRequestId")] Guid requestId)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            if (response == null)//这种情况是报异常了。
                return;
            int statuCode = (int?)response.StatusCode ?? 0;
            string content;
            if(DiagnosticLogConfig.Config?.RecordHttpClientBody ?? false)//是否需要记录
            {
                //bool contentTypeIsText = response.Content.Headers.ContentType?.MediaType?.StartsWith("text/") == true ||
                //    response.Content.Headers.ContentType?.MediaType?.Contains("json") == true ||
                //    response.Content.Headers.ContentType?.MediaType?.Contains("xml") == true;
                // 上面这种方式不准， 比如 word 文档就是 application/xml
                response.Content = new RepeatableHttpContent(response.Content);//让 HttpContent 可以重复读取
                var bytes = response.Content?.ReadAsByteArrayAsync().Result;
                var stream = new MemoryStream(bytes);
                var reader = new StreamReader(stream, Encoding.UTF8, true);
                string text = reader.ReadToEnd() ?? string.Empty;

                // 如果读取后位置未移动到末尾，说明可能有无效字符
                bool isBinary = stream.Position != stream.Length || text.Contains('\uFFFD');
                if (isBinary)
                {
                    content = $"Content-Type: {response.Content?.Headers.ContentType.MediaType}\nContent-Length: {response.Content?.Headers.ContentLength?.ToString()}\n检测到二进制，将提取一些文本：\n";
                    try
                    {
                        content += ExtractPrintableCharacters(bytes);
                    }
                    catch { }
                }
                else
                {
                    bool isjson = (text.Trim().StartsWith("{") && text.Trim().EndsWith("}")) || (text.Trim().StartsWith("[") && text.Trim().EndsWith("]"));
                    content = $"Content-Type: {response.Content?.Headers.ContentType.MediaType}\nContent-Length: {response.Content?.Headers.ContentLength?.ToString()}\n";
                    if (text.Length > DiagnosticLogConfig.Config.RecordHttpClientResponseBodyMax && (!isjson || !DiagnosticLogConfig.Config.RecordHttpClientFullWhenJson))
                        content += text.Substring(0, DiagnosticLogConfig.Config.RecordHttpClientResponseBodyMax);
                    else
                        content += text;
                }
            }
            else
            {
                content = $"Content-Type: {response.Content?.Headers.ContentType.MediaType}\nContent-Length: {response.Content?.Headers.ContentLength?.ToString()}";
            }
            
            LogEntity log = new LogEntity()
            {
                StatuCode = statuCode,
                StackTrace = content,
                GroupGuid = GuidHolder.Holder.Value.ToString(),
                LogType = LogTypeEnum.HttpClientResponse,
                HappenTime = DateTime.Now,
                PageId = GuidHolder.PageIdHolder.Value,
                TraceId = GuidHolder.TraceIdHolder.Value,
                ParentGuid = GuidHolder.ParentHolder.Value,
                HttpId = requestId.ToString(),
                ParentHttpId = GuidHolder.ParentHttpHolder.Value
            };
            Helper.PostHelper.ProcessLog(log);
        }

        [DiagnosticName("System.Net.Http.Exception")]
        public void HttpException([Property(Name = "Request")] HttpRequestMessage request,
            [Property(Name = "Exception")] Exception exception, [Property(Name = "LoggingRequestId")] Guid? requestId)
        {
            if (GuidHolder.Holder.Value == Guid.Empty)
                return;
            var log = ToLog(request, requestId, false);
            log.LogType = LogTypeEnum.HttpClientException;
            log.StackTrace = exception.StackTrace;
            log.Message = exception.Message;
            log.PageId = GuidHolder.PageIdHolder.Value;
            log.TraceId = GuidHolder.TraceIdHolder.Value;
            log.ParentGuid = GuidHolder.ParentHolder.Value;
            log.ParentHttpId = GuidHolder.ParentHttpHolder.Value;
            Helper.PostHelper.ProcessLog(log);
        }
    }
}
