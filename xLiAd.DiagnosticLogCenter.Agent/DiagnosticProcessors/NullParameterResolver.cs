using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.Agent.DiagnosticProcessors
{
    public class NullParameterResolver : IParameterResolver
    {
        public object Resolve(object value)
        {
            return null;
        }
    }
}
