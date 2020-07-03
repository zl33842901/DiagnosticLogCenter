using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.Agent.DiagnosticProcessors
{
    public class DiagnosticName : Attribute
    {
        public string Name { get; }

        public DiagnosticName(string name)
        {
            Name = name;
        }
    }
}
