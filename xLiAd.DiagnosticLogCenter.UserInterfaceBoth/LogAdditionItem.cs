using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.UserInterface.Models
{
    public partial class LogAdditionItem
    {
        /// <summary>
        /// 计算字段，本步骤是否成功
        /// </summary>
        public bool Sucess { get; set; }
        /// <summary>
        /// 计算字段，本步骤用时
        /// </summary>
        public int TotalMillionSeconds { get; set; }
        /// <summary>
        /// 本步骤完成时间
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 找到了对应的END或 EXCEPTION
        /// </summary>
        public bool WithEnd { get; set; } = false;
    }
}
