using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.AgentFramework
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
