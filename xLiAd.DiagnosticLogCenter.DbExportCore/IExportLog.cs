using System;
using System.Collections.Generic;
using System.Text;

namespace xLiAd.DiagnosticLogCenter.DbExportCore
{
    public interface IExportLog
    {
        void Info(object message);
        void Info(object message, Exception exception);
        void Error(object message);
        void Error(object message, Exception exception);
    }
}
