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
        //private readonly CollectServerByEs.Services.ILogBatchService logBatchServiceEs;
        private readonly ITraceAndGroupService traceAndGroupService;
        private readonly IRabbitMqService rabbitMqService;
        public DiaglogService(CollectServer.Services.ILogBatchService logBatchService, //CollectServerByEs.Services.ILogBatchService logBatchServiceEs,
            ITraceAndGroupService traceAndGroupService, IRabbitMqService rabbitMqService)
        {
            this.logBatchService = logBatchService;
            //this.logBatchServiceEs = logBatchServiceEs;
            this.traceAndGroupService = traceAndGroupService;
            this.rabbitMqService = rabbitMqService;
        }

        public override async Task<LogReply> PostLog(LogDto request, ServerCallContext context)
        {
            //先记录
            (string, CollectServer.Models.Log)[] datas = logBatchService.ProcessConvert(request);
            try
            {
                await logBatchService.ProcessWriteDown(datas);
            }
            catch { }
            //try
            //{
            //    await logBatchServiceEs.Process(request);
            //}
            //catch { }
            //最后发送到分析队列
            try
            {
                var anglist = datas.Where(x => x.Item2.Addtions.Any(y => y.LogType == Abstract.LogTypeEnum.RequestEndSuccess || y.LogType == Abstract.LogTypeEnum.RequestEndException));
                foreach (var ang in anglist)
                    rabbitMqService.Publish(Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        ang.Item2.ClientName,
                        ang.Item2.EnvironmentName,
                        ang.Item2.HappenTime,
                        ang.Item2.Guid,
                        ang.Item2.Message,
                        ang.Item2.Success,
                        ang.Item2.TotalMillionSeconds
                    }));
            }
            catch(Exception ex) { Console.WriteLine(ex.Message);Console.WriteLine(ex.StackTrace); }
            //计算 TraceId  PageId
            try
            {
                await traceAndGroupService.ProcessLogs(datas.Select(x => x.Item2));
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); Console.WriteLine(ex.StackTrace); }

            var result = new LogReply() { Success = true };
            return result;
        }
    }
}
