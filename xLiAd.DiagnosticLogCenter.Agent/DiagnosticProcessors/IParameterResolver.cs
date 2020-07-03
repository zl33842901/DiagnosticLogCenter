using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.Agent.DiagnosticProcessors
{
    public interface IParameterResolver
    {
        object Resolve(object value);
    }
}
