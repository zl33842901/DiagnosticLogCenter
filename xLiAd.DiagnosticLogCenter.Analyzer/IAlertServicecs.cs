using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.Analyzer
{
    public interface IAlertServicecs
    {
        /// <summary>
        /// 实现一个通知 clientName系统的 envName环境，message场景，在 minutes 分钟内 访问次数 logcount，发生 exceptioncount 次异常，请关注。
        /// </summary>
        /// <param name="clientName"></param>
        /// <param name="envName"></param>
        /// <param name="message"></param>
        /// <param name="minutes"></param>
        /// <param name="logcount"></param>
        /// <param name="exceptioncount"></param>
        void Alert(string clientName, string envName, string message, int minutes, int logcount, int exceptioncount);
    }
}
