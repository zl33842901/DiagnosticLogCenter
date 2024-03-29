﻿using System;
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
            var guids = trace?.Items.Distinct().ToArray();
            var groups = guids?.GroupBy(x => x.CollectionName);
            List<UserInterface.Models.Log> result = new List<UserInterface.Models.Log>();
            var first = logRepository.GetByCollectionNameAndTraceId(new UserInterface.Models.CliEvnDate() {
                    ClientName = traceValue.ClientName,
                    EnvironmentName = traceValue.EnvName,
                    HappenTime = traceValue.HappenTime
                }.GetIndexName(), traceId);
            first[0].PrepareLogForRead();
            first.ProcessEndAndException();
            result.AddRange(first);
            if(groups.AnyX())
                foreach (var group in groups)
                {
                    var l = logRepository.GetByCollectionNameAndId(group.Key, group.Select(x => x.Guid));
                    foreach (var item in l)
                        item.PrepareLogForRead();
                    l.ProcessEndAndException();
                    result.AddRange(l);
                }
            //还要处理子日志的情况。
            result = await ProcessShowLine(result);
            return result;
        }

        private Task<List<UserInterface.Models.Log>> ProcessShowLine(List<UserInterface.Models.Log> logs)
        {
            //因为系统间会有时间差，所以得把时间差算出来，才显示得准
            logs = logs.OrderBy(x => x.ParentGuid != string.Empty).ThenBy(x => x.HappenTime).ToList();
            var root = logs.Where(x => x.ParentGuid.NullOrEmpty()).OrderByDescending(x => x.TotalMillionSeconds).FirstOrDefault();
            if (root == null)
                root = logs.OrderByDescending(x => x.TotalMillionSeconds).FirstOrDefault();
            root.Corrected = true;
            while(logs.Any(x => !x.Corrected))
            {
                var afct = 0;
                foreach(var x in logs.Where(x => !x.Corrected))
                {
                    if (!x.ParentHttpId.NullOrEmpty())
                    {
                        var adts = logs.Where(x => x.Corrected).SelectMany(x => x.Addtions).ToArray();
                        var parent = adts.Where(y => y.HttpId == x.ParentHttpId).FirstOrDefault();
                        if (parent != null)
                        {
                            x.Corrected = true;
                            x.MillSecondDiffToParent = Convert.ToInt32((x.HappenTime - parent.HappenTime).TotalMilliseconds);
                            x.MillSecondDiffToRoot = x.MillSecondDiffToParent + parent.MillSecondDiffToRoot;
                        }
                        foreach (var adt in x.Addtions)
                            adt.MillSecondDiffToRoot = x.MillSecondDiffToRoot;
                        afct++;
                    }
                }
                if (afct == 0)
                    break;
            }
            var addtions = logs.SelectMany(x => x.Addtions).ToArray();
            var earliest = root.HappenTime;
            var latest = addtions.Max(x => x.HappenTime.AddMilliseconds(x.TotalMillionSeconds - x.MillSecondDiffToRoot));
            var latest1 = logs.Max(x => x.HappenTime.AddMilliseconds(x.TotalMillionSeconds - x.MillSecondDiffToRoot));
            latest = new DateTime[] { latest, latest1 }.Max();
            int maxlength = Convert.ToInt32((latest - earliest).TotalMilliseconds);
            var i = 0;
            foreach (var x in logs)
            {
                x.Length = maxlength > 0 ? (x.TotalMillionSeconds * 100 / maxlength) : 100;
                var startPoint = Convert.ToInt32((x.HappenTime.AddMilliseconds(-x.MillSecondDiffToRoot) - earliest).TotalMilliseconds * 100) / maxlength;
                x.StartPoint = startPoint;
                x.Line = i++;
                foreach(var y in x.Addtions.Where(y => y.WithEnd))
                {
                    y.Length = maxlength > 0 ? (y.TotalMillionSeconds * 100 / maxlength) : 100;
                    y.StartPoint = startPoint + Convert.ToInt32((y.HappenTime - x.HappenTime).TotalMilliseconds * 100) / maxlength;
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
