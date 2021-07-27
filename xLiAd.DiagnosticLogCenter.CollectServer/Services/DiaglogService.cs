using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using xLiAd.DiagnosticLogCenter.CollectServer.Services;

namespace xLiAd.DiagnosticLogCenter.CollectServer
{
    public class DiaglogService : Diagloger.DiaglogerBase
    {
        private readonly ILogger<DiaglogService> _logger;
        private readonly ILogBatchService logBatchService;
        public DiaglogService(ILogger<DiaglogService> logger, ILogBatchService logBatchService)
        {
            _logger = logger;
            this.logBatchService = logBatchService;
        }

        public override async Task<LogReply> PostLog(LogDto request, ServerCallContext context)
        {
            await logBatchService.Process(request);
            var result = new LogReply() { Success = true };
            return result;
        }
    }
}
