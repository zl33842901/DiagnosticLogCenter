using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.Analyzer
{
    public class LogModel
    {
        public string ClientName { get; set; }

        public string EnvironmentName { get; set; }

        public DateTime HappenTime { get; set; }

        public string Guid { get; set; }

        public string Message { get; set; }

        public bool Success { get; set; }

        public int TotalMillionSeconds { get; set; }
    }
}
