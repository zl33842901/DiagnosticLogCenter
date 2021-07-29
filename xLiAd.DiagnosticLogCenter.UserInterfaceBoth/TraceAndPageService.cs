using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.CollectServerBoth.TraceAndPage;

namespace xLiAd.DiagnosticLogCenter.UserInterfaceBoth
{
    public class TraceAndPageService : ITraceAndPageService
    {
        private readonly ITraceRepository traceRepository;
        private readonly IPageRepository pageRepository;
        public TraceAndPageService(IPageRepository pageRepository, ITraceRepository traceRepository)
        {
            this.traceRepository = traceRepository;
            this.pageRepository = pageRepository;
        }

        /// <summary>
        /// 某个 trace 除了某个 guid 外，还存不存在其他的 guid
        /// </summary>
        /// <param name="traceId"></param>
        /// <param name="guid"></param>
        /// <param name="happenTime"></param>
        /// <returns></returns>
        public async Task<bool> GetTraceGroupExist(string traceId, string guid, DateTime happenTime)
        {
            var trace = await traceRepository.FindByTraceId(traceId, happenTime);
            if (trace.Items.Any(x => x.Guid != guid))
                return true;
            else
                return false;
        }
    }

    public interface ITraceAndPageService
    {
        Task<bool> GetTraceGroupExist(string traceId, string guid, DateTime happenTime);
    }
}
