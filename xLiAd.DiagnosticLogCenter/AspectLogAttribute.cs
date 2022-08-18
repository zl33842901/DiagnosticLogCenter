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
        /// <summary>
        /// 是否等待任务
        /// </summary>
        protected virtual bool WaitTasks => true;
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            if (!Enable)
                return context.Invoke(next);
            string className = context.Implementation.GetType().Name;
            string methodName = context.ImplementationMethod.Name;
            string cnt;
            try
            {
                cnt = Newtonsoft.Json.JsonConvert.SerializeObject(context.Parameters);
            }
            catch { cnt = "没有获取到方法参数值。"; }
            Listener.Write(LogTypeEnum.MethodEntry, className, methodName, cnt);
            var t = context.Invoke(next);
            if (t.Exception != null)
            {
                string cont;
                try
                {
                    cont = Newtonsoft.Json.JsonConvert.SerializeObject(t.Exception);
                }
                catch { cont = t.Exception.Message + "\r\nStackTrace:\r\n" + t.Exception.StackTrace; }
                Listener.Write(LogTypeEnum.MethodException, className, methodName, cont);
            }
            else
            {
                try
                {
                    object realResult;
                    if (context.IsAsync())
                    {
                        if (WaitTasks)
                            t.ConfigureAwait(false).GetAwaiter().GetResult();
                        var tp = context.ImplementationMethod.ReturnType.GetProperty("Result", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        realResult = tp.GetValue(context.ReturnValue);
                    }
                    else
                    {
                        realResult = context.ReturnValue;
                    }

                    Listener.Write(LogTypeEnum.MethodLeave, className, methodName, Newtonsoft.Json.JsonConvert.SerializeObject(realResult));
                }
                catch(Exception ex)
                {
                    Listener.Write(LogTypeEnum.MethodLeave, className, methodName, "没有获取到方法返回值。");
                }
            }
            return t;
        }
    }

    public class AspectLogUnWaitAttribute : AspectLogAttribute
    {
        protected override bool WaitTasks => false;
    }
}
