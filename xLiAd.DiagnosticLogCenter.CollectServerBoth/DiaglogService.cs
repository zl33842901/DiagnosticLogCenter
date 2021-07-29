using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.CollectServerBoth.TraceAndPage;

namespace xLiAd.DiagnosticLogCenter.CollectServerBoth
{
    public class DiaglogService : Diagloger.DiaglogerBase
    {
        private readonly CollectServer.Services.ILogBatchService logBatchService;
        private readonly CollectServerByEs.Services.ILogBatchService logBatchServiceEs;
        private readonly ITraceAndGroupService traceAndGroupService;
        public DiaglogService(CollectServer.Services.ILogBatchService logBatchService, CollectServerByEs.Services.ILogBatchService logBatchServiceEs,
            ITraceAndGroupService traceAndGroupService)
        {
            this.logBatchService = logBatchService;
            this.logBatchServiceEs = logBatchServiceEs;
            this.traceAndGroupService = traceAndGroupService;
        }

        public override async Task<LogReply> PostLog(LogDto request, ServerCallContext context)
        {
            (string, CollectServer.Models.Log)[] datas = logBatchService.ProcessConvert(request);
            try
            {
                await logBatchService.ProcessWriteDown(datas);
            }
            catch { }
            try
            {
                await logBatchServiceEs.Process(request);
            }
            catch { }
            try
            {
                await traceAndGroupService.ProcessLogs(datas.Select(x => x.Item2));
            }
            catch { }
            var result = new LogReply() { Success = true };
            return result;
        }
    }
}
