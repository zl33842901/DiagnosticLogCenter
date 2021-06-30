using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.CollectServerByEs.Models
{
    public abstract class EntityBase
    {
        public string Id { get; set; }

        public void DoId() => Id = DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString();
    }
}
