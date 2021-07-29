using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.MongoEx.Entities;

namespace xLiAd.DiagnosticLogCenter.CollectServerBoth.TraceAndPage
{
    public class PageGroup : EntityModel
    {
        public string PageId { get; set; }
        public PageItem[] Items { get; set; }
    }

    public class PageItem
    {
        public string TraceId { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType() != typeof(PageItem))
                return false;
            var o = (PageItem)obj;
            return o.TraceId == this.TraceId;
        }

        public override int GetHashCode()
        {
            return TraceId?.GetHashCode() ?? 0;
        }
    }
}
