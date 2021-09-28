using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.Abstract;
using xLiAd.DiagnosticLogCenter.CollectServerBoth.TraceAndPage;
using xLiAd.DiagnosticLogCenter.UserInterface;
using xLiAd.DiagnosticLogCenter.UserInterface.Repositories;

namespace xLiAd.DiagnosticLogCenter.UserInterfaceBoth
{
    public class TraceAndPageService : ITraceAndPageService
    {
        private readonly ITraceRepository traceRepository;
        private readonly IPageRepository pageRepository;
        private readonly ILogRepository logRepository;
        public TraceAndPageService(IPageRepository pageRepository, ITraceRepository traceRepository, ILogRepository logRepository)
        {
            this.traceRepository = traceRepository;
            this.pageRepository = pageRepository;
            this.logRepository = logRepository;
        }

        /// <summary>
        /// 某个 trace 除了某个 guid 外，还存不存在其他的 guid
        /// </summary>
        /// <param name="traceId"></param>
        /// <param name="guid"></param>
        /// <param name="happenTime"></param>
        /// <returns></returns>
        public async Task<(bool, bool)> GetTracePageExist(string traceId, string pageId, string guid, DateTime happenTime)
        {
            bool traceExists, pageExists;
            var trace = await traceRepository.FindByTraceId(traceId, happenTime);
            //traceExists = trace?.Items.Any(x => x.Guid != guid) ?? false;//trace 为空可能是没来得及写入
            traceExists = trace != null;
            var page = await pageRepository.FindByPageId(pageId, happenTime);
            //pageExists = page?.Items.Any(x => x.TraceId != traceId) ?? false;//page 为空可能是没来得及写入
            pageExists = page != null;
            return (traceExists, pageExists);
        }

        public async Task<List<UserInterface.Models.Log>> GetTraceAll(string traceId, DateTime happenTime)
        {
            var trace = await traceRepository.FindByTraceId(traceId, happenTime);
            var traceValue = TracePageIdValue.FromString(traceId);
            var guids = trace.Items.Distinct().ToArray();
            var groups = guids.GroupBy(x => x.CollectionName);
            List<UserInterface.Models.Log> result = new List<UserInterface.Models.Log>();
            var first = logRepository.GetByCollectionNameAndTraceId(new UserInterface.Models.CliEvnDate() {
                    ClientName = traceValue.ClientName,
                    EnvironmentName = traceValue.EnvName,
                    HappenTime = traceValue.HappenTime
                }.GetIndexName(), traceId);
            first.ProcessEndAndException();
            result.AddRange(first);
            foreach (var group in groups)
            {
                var l = logRepository.GetByCollectionNameAndId(group.Key, group.Select(x => x.Guid));
                l.ProcessEndAndException();
                result.AddRange(l);
            }
            foreach (var item in result)
                item.PrepareLogForRead();
            //还要处理子日志的情况。
            result = await ProcessShowLine(result);
            return result;
        }

        private Task<List<UserInterface.Models.Log>> ProcessShowLine(List<UserInterface.Models.Log> logs)
        {
            var addtions = logs.SelectMany(x => x.Addtions).ToArray();
            var earliest = logs.Min(x => x.HappenTime);
            var latest = addtions.Max(x => x.HappenTime.AddMilliseconds(x.TotalMillionSeconds));
            var latest1 = logs.Max(x => x.HappenTime.AddMilliseconds(x.TotalMillionSeconds));
            latest = new DateTime[] { latest, latest1 }.Max();
            int maxlength = Convert.ToInt32((latest - earliest).TotalMilliseconds);
            var i = 0;
            logs = logs.OrderBy(x => x.ParentGuid != string.Empty).ThenBy(x => x.HappenTime).ToList();
            foreach (var x in logs)
            {
                x.Length = maxlength > 0 ? (x.TotalMillionSeconds * 100 / maxlength) : 100;
                int? startPoint = null;
                if (!x.ParentHttpId.NullOrEmpty())
                {
                    var parent = addtions.Where(y => y.HttpId == x.ParentHttpId).FirstOrDefault();
                    if (parent != null)
                        startPoint = parent.StartPoint + 1;
                }
                if (startPoint == null)
                    startPoint = Convert.ToInt32((x.HappenTime - earliest).TotalMilliseconds * 100) / maxlength;
                x.StartPoint = startPoint.Value;
                x.Line = i++;
                foreach(var y in x.Addtions.Where(y => y.WithEnd))
                {
                    y.Length = maxlength > 0 ? (y.TotalMillionSeconds * 100 / maxlength) : 100;
                    y.StartPoint = startPoint.Value + Convert.ToInt32((y.HappenTime - x.HappenTime).TotalMilliseconds * 100) / maxlength;
                }
            }
            return Task.FromResult(logs);
        }
    }

    public interface ITraceAndPageService
    {
        Task<(bool, bool)> GetTracePageExist(string traceId, string pageId, string guid, DateTime happenTime);
        Task<List<UserInterface.Models.Log>> GetTraceAll(string traceId, DateTime happenTime);
    }
}
