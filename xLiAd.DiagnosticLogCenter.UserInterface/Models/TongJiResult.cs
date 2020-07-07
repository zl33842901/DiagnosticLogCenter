using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.UserInterface.Models
{
    /// <summary>
    /// 这种是专为异常日志做的类
    /// </summary>
    public class TongJiResult : ICliEnvDate
    {
        public string ClientName { get; set; }
        public string EnvironmentName { get; set; }
        public DateTime HappenTime { get; set; }

        public int Count { get; set; }
        public List<TongJiItem> Items { get; set; } = new List<TongJiItem>();
    }

    public class TongJiItem
    {
        public string Message { get; set; }
        public int Count { get; set; }
    }
    /// <summary>
    /// 这种是专为接口日志写的
    /// </summary>
    public class TongJiInterfaceResult
    {
        public string Title { get; set; }
        public List<TongJiInterfaceItem> Items { get; set; } = new List<TongJiInterfaceItem>();
    }
    public class TongJiInterfaceItem
    {
        public string MethodName { get; set; }
        public int Count { get; set; }
        public int AvgSeconds { get; set; }
    }
}
