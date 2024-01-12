using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace xLiAd.DiagnosticLogCenter.AgentFramework
{
    public static class PostHelper
    {
        private static string address;
        private static string clientName;
        private static string evn;
        private static int timeoutBySeconds;
        private static ConcurrentQueue<LogEntity> LogContainer = new ConcurrentQueue<LogEntity>();
        private static System.Timers.Timer Timer;
        private static readonly LocalCacheHelper localCacheHelper = new LocalCacheHelper();
        private static object forClearLock = new object();

        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) => Timer_Elapsed(sender);
        private static void Timer_Elapsed(object sender)
        {
            List<LogEntity> items = new List<LogEntity>();
            while (LogContainer.TryDequeue(out var log))
            {
                items.Add(log);
            }
            var shouldClearCacheLogs = false;//在没有新日志、成功上传的情况下可以清本地日志缓存
            if (!items.AnyX())
                shouldClearCacheLogs = true;
            else
            {
                try
                {
                    var httpClient = new HttpClient();
                    httpClient.Timeout = TimeSpan.FromSeconds(timeoutBySeconds);
                    var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(items));
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                    var r = httpClient.PostAsync(address, content).ConfigureAwait(false).GetAwaiter().GetResult();
                    if (r.StatusCode == System.Net.HttpStatusCode.OK)
                        shouldClearCacheLogs = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DiagnosticLogCenter: An error occurred while Post api:");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    if (localCacheHelper.WriteLog(items))
                        Console.WriteLine("DiagnosticLogCenter: Component has cached logs locally.");
                }
            }
            if (shouldClearCacheLogs)
            {
                lock (forClearLock)
                {
                    try
                    {
                        var loge = localCacheHelper.PeekClearLog();
                        if (loge != null && loge.Logs.AnyX())
                        {
                            var httpClient = new HttpClient();
                            httpClient.Timeout = TimeSpan.FromSeconds(timeoutBySeconds);
                            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(items));
                            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                            var r = httpClient.PostAsync(address, content).ConfigureAwait(false).GetAwaiter().GetResult();
                            loge.action.Invoke();
                        }
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                }
            }
        }

        public static void Init(string _address, string _clientName, string _envName, int _timeoutBySeconds)
        {
            Timer = new System.Timers.Timer();
            Timer.AutoReset = true;
            Timer.Enabled = true;
            Timer.Interval = 3000;
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();
            address = _address;
            clientName = _clientName;
            evn = _envName;
            timeoutBySeconds = _timeoutBySeconds;
        }

        public static void ProcessLog(LogEntity logEntity)
        {
            logEntity.ClientName = clientName;
            logEntity.EnvironmentName = evn;
            LogContainer.Enqueue(logEntity);
        }
    }
}
