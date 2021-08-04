using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.CollectServerBoth.TraceAndPage;
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
            traceExists = trace.Items.Any(x => x.Guid != guid);
            var page = await pageRepository.FindByPageId(pageId, happenTime);
            pageExists = page.Items.Any(x => x.TraceId != traceId);
            return (traceExists, pageExists);
        }

        public async Task<List<UserInterface.Models.Log>> GetTraceAll(string traceId, DateTime happenTime)
        {
            var trace = await traceRepository.FindByTraceId(traceId, happenTime);
            var guids = trace.Items.Distinct().ToArray();
            var groups = guids.GroupBy(x => x.CollectionName);
            List<UserInterface.Models.Log> result = new List<UserInterface.Models.Log>();
            foreach(var group in groups)
            {
                var l = logRepository.GetByCollectionNameAndId(group.Key, group.Select(x => x.Guid));
                l.ProcessEndAndException();
                result.AddRange(l);
            }
            //还要处理子日志的情况。
            result = await ProcessShowLine(result);
            return result;
        }

        private Task<List<UserInterface.Models.Log>> ProcessShowLine(List<UserInterface.Models.Log> logs)
        {
            var maxlength = logs.Max(x => x.TotalMillionSeconds);
            var earliest = logs.Min(x => x.HappenTime);
            var i = 0;
            logs = logs.OrderBy(x => x.ParentGuid != string.Empty).ThenBy(x => x.HappenTime).ToList();
            foreach (var x in logs)
            {
                x.Length = maxlength > 0 ? (x.TotalMillionSeconds * 100 / maxlength) : 100;
                x.StartPoint = Convert.ToInt32((x.HappenTime - earliest).TotalMilliseconds * 100) / maxlength;
                x.Line = i++;
                foreach(var y in x.Addtions.Where(y => y.WithEnd))
                {
                    y.Length = maxlength > 0 ? (y.TotalMillionSeconds * 100 / maxlength) : 100;
                    y.StartPoint = Convert.ToInt32((y.HappenTime - earliest).TotalMilliseconds * 100) / maxlength;
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
