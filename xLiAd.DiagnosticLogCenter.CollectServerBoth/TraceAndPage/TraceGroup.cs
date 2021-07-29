using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.MongoEx.Entities;

namespace xLiAd.DiagnosticLogCenter.CollectServerBoth.TraceAndPage
{
    public class TraceGroup : EntityModel
    {
        public string TraceId { get; set; }
        public TraceItem[] Items { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }

    public class TraceItem
    {
        public string CollectionName { get; set; }
        public string Guid { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != typeof(TraceItem))
                return false;
            var o = (TraceItem) obj;
            return o.CollectionName == this.CollectionName && o.Guid == this.Guid;
        }

        public override int GetHashCode()
        {
            return (CollectionName?.GetHashCode() ?? 0) ^ (Guid?.GetHashCode() ?? 0);
        }
    }
}
