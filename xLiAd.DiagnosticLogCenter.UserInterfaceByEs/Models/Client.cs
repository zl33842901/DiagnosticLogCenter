using System;

namespace xLiAd.DiagnosticLogCenter.UserInterfaceByEs.Models
{
    public class Client : EntityBase
    {
        public string Name { get; set; }
        public Environment[] Environments { get; set; }
    }
}
