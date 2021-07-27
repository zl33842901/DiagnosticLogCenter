using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.CollectServerBoth
{
    public class DiaglogService : Diagloger.DiaglogerBase
    {
        private readonly CollectServer.Services.ILogBatchService logBatchService;
        private readonly CollectServerByEs.Services.ILogBatchService logBatchServiceEs;
        public DiaglogService(CollectServer.Services.ILogBatchService logBatchService, CollectServerByEs.Services.ILogBatchService logBatchServiceEs)
        {
            this.logBatchService = logBatchService;
            this.logBatchServiceEs = logBatchServiceEs;
        }

        public override async Task<LogReply> PostLog(LogDto request, ServerCallContext context)
        {
            await logBatchService.Process(request);
            await logBatchServiceEs.Process(request);
            var result = new LogReply() { Success = true };
            return result;
        }
    }
}
