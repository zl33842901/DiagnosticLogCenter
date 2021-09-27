using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.Abstract
{
    public class TracePageIdValue
    {
        public TracePageIdValue(DateTime happenTime, string clientName, string envName, Guid guid)
        {
            HappenTime = happenTime;
            ClientName = clientName;
            EnvName = envName;
            Guid = guid;
        }
        public TracePageIdValue(DateTime happenTime, string clientName, string envName) : this(happenTime, clientName, envName, Guid.NewGuid()) { }

        public DateTime HappenTime { get; }
        public string ClientName { get; }
        public string EnvName { get; }
        public Guid Guid { get; }

        public override string ToString()
        {
            return HappenTime.ToString("yyyyMMdd-HHmmss-fff-") + ClientName + "-" + EnvName + "-" + Guid.ToString();
        }

        public static TracePageIdValue FromString(string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentNullException("s", "试图转化 null/空串 到一个 TraceId/PageId 。");
            var sa = s.Split('-');
            //if (sa.Length != 6)
            //    throw new ArgumentException($"转化一个 TraceId/PageId 时提供了一个不合规的字符串，应为6段，实为：{sa.Length}段。", "s");
            if(sa[0].Length != 8 || sa[1].Length != 6 || !int.TryParse(sa[0], out var ymd) || !int.TryParse(sa[1], out var hms) || !int.TryParse(sa[2], out var fff))
                throw new ArgumentException($"转化一个 TraceId/PageId 时提供了一个不合规的字符串{s}。", "s");
            DateTime dt = new DateTime(ymd / 10000, (ymd % 10000) / 100, ymd % 100, hms / 10000, (hms % 10000) / 100, hms % 100, fff);
            var result = new TracePageIdValue(dt, sa[3], sa[4], Guid.Parse(string.Join("-",sa.Skip(5))));
            return result;
        }
    }
}
