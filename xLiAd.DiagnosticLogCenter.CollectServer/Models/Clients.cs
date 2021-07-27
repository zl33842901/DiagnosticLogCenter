using System;
using xLiAd.MongoEx.Entities;

namespace xLiAd.DiagnosticLogCenter.CollectServer.Models
{
    public class Clients : EntityModel
    {
        public string Name { get; set; }

        public Environment[] Environments { get; set; }
        public string[] Emails { get; set; } = new string[0];
        public string[] DomainAccounts { get; set; } = new string[0];
        public string[] Mobiles { get; set; } = new string[0];
    }
}
