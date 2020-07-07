using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace xLiAd.DiagnosticLogCenter.Agent
{
    public static class GuidHolder
    {
        public static AsyncLocal<Guid> Holder = new AsyncLocal<Guid>();
    }
}
