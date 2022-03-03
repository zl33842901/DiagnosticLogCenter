using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using xLiAd.DiagnosticLogCenter.Abstract;

namespace xLiAd.DiagnosticLogCenter
{
    public static class DiagnosticLogCenter
    {
        public static void AdditionLog(string content)
        {
            StackTrace stackTrace = new StackTrace(false);
            var method = stackTrace.GetFrame(1).GetMethod();
            Listener.Write(LogTypeEnum.MethodAddition, method.DeclaringType.Name, method.Name, content);
        }
        public static void AdditionLog(object content)
        {
            AdditionLog(Newtonsoft.Json.JsonConvert.SerializeObject(content));
        }

        public static void MethodIn(string className, string methodName, object[] inparams)
        {
            string cnt;
            try
            {
                cnt = Newtonsoft.Json.JsonConvert.SerializeObject(inparams);
            }
            catch { cnt = "没有获取到方法参数值。"; }
            Listener.Write(LogTypeEnum.MethodEntry, className, methodName, cnt);
        }
        public static void MethodIn(object[] inparams)
        {
            StackTrace stackTrace = new StackTrace(false);
            var method = stackTrace.GetFrame(1).GetMethod();
            MethodIn(method.DeclaringType.Name, method.Name, inparams);
        }

        public static void MethodException(string className, string methodName, Exception ex)
        {
            string cnt;
            try
            {
                cnt = Newtonsoft.Json.JsonConvert.SerializeObject(ex);
            }
            catch { cnt = ex.Message + "\r\nStackTrace:\r\n" + ex.StackTrace; }
            Listener.Write(LogTypeEnum.MethodException, className, methodName, cnt);
        }
        public static void MethodException(Exception ex)
        {
            StackTrace stackTrace = new StackTrace(false);
            var method = stackTrace.GetFrame(1).GetMethod();
            MethodException(method.DeclaringType.Name, method.Name, ex);
        }

        public static void MethodLeave(string className, string methodName, object[] results)
        {
            string cnt;
            try
            {
                cnt = Newtonsoft.Json.JsonConvert.SerializeObject(results);
            }
            catch { cnt = "没有获取到方法返回值"; }
            Listener.Write(LogTypeEnum.MethodLeave, className, methodName, cnt);
        }
        public static void MethodLeave(object[] results)
        {
            StackTrace stackTrace = new StackTrace(false);
            var method = stackTrace.GetFrame(1).GetMethod();
            MethodLeave(method.DeclaringType.Name, method.Name, results);
        }
    }
}
