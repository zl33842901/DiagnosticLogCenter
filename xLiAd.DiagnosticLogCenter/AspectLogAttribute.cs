using AspectCore.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.Abstract;

namespace xLiAd.DiagnosticLogCenter
{
    public class AspectLogAttribute : AbstractInterceptorAttribute
    {
        internal static bool Enable { get; set; } = true;
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            if (!Enable)
                return context.Invoke(next);
            string className = context.Implementation.GetType().Name;
            string methodName = context.ImplementationMethod.Name;
            string cnt = Newtonsoft.Json.JsonConvert.SerializeObject(context.Parameters);
            Listener.Write(LogTypeEnum.MethodEntry, className, methodName, cnt);
            var t = context.Invoke(next);
            if (t.Exception != null)
            {
                Listener.Write(LogTypeEnum.MethodException, className, methodName, t.Exception.Message + "\r\nStackTrace:\r\n" + t.Exception.StackTrace);
            }
            else
            {
                try
                {
                    bool isTask = context.ImplementationMethod.ReturnType.IsGenericType && context.ImplementationMethod.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);
                    object realResult;
                    if (isTask)
                        realResult = context.ImplementationMethod.ReturnType.GetProperty("Result", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(context.ReturnValue);
                    else
                        realResult = context.ReturnValue;
                    Listener.Write(LogTypeEnum.MethodLeave, className, methodName, Newtonsoft.Json.JsonConvert.SerializeObject(realResult));
                }
                catch
                {
                    Listener.Write(LogTypeEnum.MethodLeave, className, methodName, "没有获取到方法返回值。");
                }
            }
            return t;
        }
    }
}
