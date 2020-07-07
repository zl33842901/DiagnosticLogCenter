using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.CollectServer.Models
{
    public interface ICliEnvDate
    {
        string EnvironmentName { get; }
        string ClientName { get; }
        DateTime HappenTime { get; }
    }
}
