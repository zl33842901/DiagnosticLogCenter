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

        void Alert(LogModel logModel);

        AlertDetailConfig GetAlertConfig(string clientName);
    }

    public class AlertDetailConfig
    {
        /// <summary>
        /// 当发生异常时，往前追的分钟数  大于80无效
        /// </summary>
        public int MinutesCatchedForProcess { get; set; }

        public MessageAlertDetailConfig[] AlterSettings { get; set; }
    }

    public class MessageAlertDetailConfig
    {
        /// <summary>
        /// 异常的URL   *代表所有未定义   ** 代表总体
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 报警的最低异常率
        /// </summary>
        public int LowestFailRate { get; set; }
        /// <summary>
        /// 报警的最低异常数量
        /// </summary>
        public int LowestFailCount { get; set; }
    }
}
