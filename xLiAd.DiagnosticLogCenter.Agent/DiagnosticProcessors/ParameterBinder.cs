using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.Agent.DiagnosticProcessors
{
    public abstract class ParameterBinder : Attribute, IParameterResolver
    {
        public abstract object Resolve(object value);
    }
}
