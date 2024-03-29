﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.Abstract;
using xLiAd.DiagnosticLogCenter.CollectServer;

namespace xLiAd.DiagnosticLogCenter.CollectServerBoth.TraceAndPage
{
    public class TraceAndGroupService : ITraceAndGroupService
    {
        class Items<T>
        {
            public string Key { get; set; }
            public DateTime HappenTime { get; set; }
            public IEnumerable<T> Datas { get; set; }
        }
        private static ConcurrentQueue<Items<TraceItem>> TraceQueue = new ConcurrentQueue<Items<TraceItem>>();
        private static ConcurrentQueue<Items<PageItem>> PageQueue = new ConcurrentQueue<Items<PageItem>>();
        private static volatile bool Writing = false;

        private readonly IPageRepository pageRepository;
        private readonly ITraceRepository traceRepository;
        public TraceAndGroupService(ITraceRepository traceRepository, IPageRepository pageRepository)
        {
            this.traceRepository = traceRepository;
            this.pageRepository = pageRepository;
        }

        public async Task ProcessLogs(IEnumerable<CollectServer.Models.Log> logs)
        {
            var grps = logs.GroupBy(x => x.TraceId);
            foreach (var grp in grps)
            {
                var traceId = TracePageIdValue.FromString(grp.Key);
                var datas = grp.Where(x => !traceId.IsFirst(x)).ToArray();
                if (!datas.AnyX())
                    continue;
                var qe = new Items<TraceItem>()
                {
                    Key = grp.Key,
                    HappenTime = grp.First().HappenTime,
                    Datas = datas.Select(x => new TraceItem() { CollectionName = x.GetIndexName(), Guid = x.Guid }).Distinct().ToArray()
                };
                TraceQueue.Enqueue(qe);
                //await traceRepository.AddOrUpdate(grp.Key, grp.First().HappenTime, grp.Select(x => new TraceItem() { CollectionName = x.GetIndexName(), Guid = x.Guid }).Distinct());
            }
            var ggs = logs.GroupBy(x => x.PageId);
            foreach(var gg in ggs)
            {
                var pageId = TracePageIdValue.FromString(gg.Key);
                var datas = gg.Where(x => !pageId.IsFirst(TracePageIdValue.FromString(x.TraceId))).ToArray();
                if (!datas.AnyX())
                    continue;
                var qe = new Items<PageItem>()
                {
                    Key = gg.Key,
                    HappenTime = gg.First().HappenTime,
                    Datas = datas.Select(x => new PageItem() { TraceId = x.TraceId }).Distinct().ToArray()
                };
                PageQueue.Enqueue(qe);
                //await pageRepository.AddOrUpdate(gg.Key, gg.First().HappenTime, gg.Select(x => new PageItem() { TraceId = x.TraceId }).Distinct());
            }
            await WriteDown();
        }

        private async Task WriteDown()
        {
            if (Writing)
                return;
            Writing = true;
            try
            {
                while(TraceQueue.TryDequeue(out var item))
                    await traceRepository.AddOrUpdate(item.Key, item.HappenTime, item.Datas);
                while (PageQueue.TryDequeue(out var item))
                    await pageRepository.AddOrUpdate(item.Key, item.HappenTime, item.Datas);
            }
            finally { Writing = false; }
        }
    }

    public interface ITraceAndGroupService
    {
        Task ProcessLogs(IEnumerable<CollectServer.Models.Log> logs);
    }
}
