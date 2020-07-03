using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using xLiAd.DiagnosticLogCenter.Abstract;

namespace xLiAd.DiagnosticLogCenter
{
    public abstract class ServiceLogBase
    {
        public void AdditionLog(string content)
        {
            StackTrace stackTrace = new StackTrace(false);
            var method = stackTrace.GetFrame(1).GetMethod();
            Listener.Write(LogTypeEnum.MethodAddition, method.DeclaringType.Name, method.Name, content);
        }
        public void AdditionLog(object content)
        {
            AdditionLog(Newtonsoft.Json.JsonConvert.SerializeObject(content));
        }
    }
}
