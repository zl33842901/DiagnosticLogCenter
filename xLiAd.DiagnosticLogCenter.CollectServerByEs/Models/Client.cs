using System;

namespace xLiAd.DiagnosticLogCenter.CollectServerByEs.Models
{
    public class Client : EntityBase
    {
        public string Name { get; set; }

        public Environment[] Environments { get; set; }
        public string[] Emails { get; set; } = new string[0];
        public string[] DomainAccounts { get; set; } = new string[0];
        public string[] Mobiles { get; set; } = new string[0];
    }
}
