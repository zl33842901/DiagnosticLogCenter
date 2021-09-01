using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace xLiAd.DiagnosticLogCenter.DbExportCore.Settings
{
    public class ExportSetting
    {
        /// <summary>
        /// 要处理的 client 名称  null 代表所有
        /// </summary>
        public IEnumerable<string> ClientNames { get; set; }
        /// <summary>
        /// 前辍
        /// </summary>
        public string Preffix { get; set; } = "LogCenter";
        /// <summary>
        /// 要处理的环境名称 null 代表所有
        /// </summary>
        public IEnumerable<string> EnvNames { get; set; }
        /// <summary>
        /// 要处理的最大日期 null 代表今天
        /// </summary>
        public DateTime? MaxDate { get; set; }
        private DateTime RealMaxDate => MaxDate ?? DateTime.Today;
        /// <summary>
        /// 备份完成是否删除Collection
        /// </summary>
        public bool DropAfterBak { get; set; }
        /// <summary>
        /// 是否处理同时期的 PageIdTraceId，当 DropAfterBak 为真时，只有 ClientNames 和 EnvNames 同时为 null，
        /// 才会真的删除 本月1号之前的 PageId TraceId 表
        /// </summary>
        public bool ProcessTraceIdPageId { get; set; }

        public ProcessEnum Check(CollectionNameModel nameModel)
        {
            if (nameModel is LogCollectionNameModel logNameModel)
            {
                if (ClientNames.AnyX() && !ClientNames.Contains(logNameModel.ClientName))
                    return ProcessEnum.None;
                if (EnvNames.AnyX() && !EnvNames.Contains(logNameModel.EnvName))
                    return ProcessEnum.None;
                if (Preffix != logNameModel.Preffix)
                    return ProcessEnum.None;
                if (logNameModel.Date > RealMaxDate)
                    return ProcessEnum.None;
                return DropAfterBak ? ProcessEnum.BakAndRemove : ProcessEnum.BakOnly;
            }
            else if (nameModel is TraceOrPageIdNameModel tpNameMode)
            {
                if (!ProcessTraceIdPageId)
                    return ProcessEnum.None;
                if (ClientNames.AnyX() || EnvNames.AnyX())
                    return ProcessEnum.None;
                var modelDate = new DateTime(tpNameMode.Year, tpNameMode.Month, 1);
                if (modelDate < new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1))
                    return ProcessEnum.BakOnly;
                else
                    return ProcessEnum.BakAndRemove;
            }
            else
                return ProcessEnum.None;
        }
    }

    public enum ProcessEnum : int
    {
        BakAndRemove = 1,
        BakOnly = 2,
        None = 0
    }
}
