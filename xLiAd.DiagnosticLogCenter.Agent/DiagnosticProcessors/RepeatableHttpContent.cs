using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace xLiAd.DiagnosticLogCenter.Agent.DiagnosticProcessors
{
    public class RepeatableHttpContent : HttpContent
    {
        private readonly byte[] _content;
        private readonly HttpContent _originalContent;

        public RepeatableHttpContent(HttpContent originalContent)
        {
            _originalContent = originalContent;
            foreach (var header in originalContent.Headers)
            {
                // 尝试添加每个标头到目标 Headers 中
                if (!Headers.TryAddWithoutValidation(header.Key, header.Value))
                {
                    Console.WriteLine($"Failed to add header {header.Key} to target content.");
                }
                //Headers.ContentType = originalContent.Headers.ContentType;
            }
            _content = originalContent.ReadAsByteArrayAsync().Result;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            => stream.WriteAsync(_content, 0, _content.Length);

        protected override bool TryComputeLength(out long length)
        {
            length = _content.Length;
            return true;
        }

        protected override Task<Stream> CreateContentReadStreamAsync()
            => Task.FromResult<Stream>(new MemoryStream(_content));
    }
}
