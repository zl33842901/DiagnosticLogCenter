using Grpc.Core;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using System.Linq;
using xLiAd.DiagnosticLogCenter.Abstract;

namespace xLiAd.DiagnosticLogCenter.CollectServerProxy
{
    public static class PostHelper
    {
        private static string address;
        private static int timeoutBySeconds;
        private static ConcurrentQueue<LogDtoItem> LogContainer = new ConcurrentQueue<LogDtoItem>();
        private static System.Timers.Timer Timer = new System.Timers.Timer();
        private static readonly LocalCacheHelper localCacheHelper = new LocalCacheHelper();
        private static object forClearLock = new object();
        private static void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            LogDto dto = new LogDto();
            while (LogContainer.TryDequeue(out var log))
            {
                dto.Items.Add(log);
            }
            var shouldClearCacheLogs = false;//在没有新日志、成功上传的情况下可以清本地日志缓存
            if (!dto.Items.AnyX())
                shouldClearCacheLogs = true;
            else
            {
                try
                {
                    var channel = new Channel(address, ChannelCredentials.Insecure);
                    var grpcClient = new Diagloger.DiaglogerClient(channel);
                    grpcClient.PostLog(dto, new CallOptions(null, DateTime.Now.ToUniversalTime().AddSeconds(timeoutBySeconds)));
                    shouldClearCacheLogs = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DiagnosticLogCenter: An error occurred while call Grpc:");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    if (localCacheHelper.WriteLog(dto))
                        Console.WriteLine("DiagnosticLogCenter: Component has cached logs locally.");
                }
            }
            if (shouldClearCacheLogs)
            {
                lock (forClearLock)
                {
                    try
                    {
                        (var logs, var action) = localCacheHelper.PeekClearLog();
                        if (logs.AnyX())
                        {
                            var channel = new Channel(address, ChannelCredentials.Insecure);
                            var grpcClient = new Diagloger.DiaglogerClient(channel);
                            foreach (var log in logs)
                                grpcClient.PostLog(log, new CallOptions(null, DateTime.Now.ToUniversalTime().AddSeconds(timeoutBySeconds)));
                            action.Invoke();
                        }
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                }
            }
        }

        public static void Init(string _address, int _timeoutBySeconds)
        {
            Timer.AutoReset = true;
            Timer.Enabled = true;
            Timer.Interval = 3000;
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();
            address = _address;
            timeoutBySeconds = _timeoutBySeconds;
        }

        public static void ProcessLog(LogEntity logEntity)
        {
            LogDtoItem logDtoItem = new LogDtoItem()
            {
                ClassName = logEntity.ClassName ?? string.Empty,
                CommandText = logEntity.CommandText ?? string.Empty,
                ClientName = logEntity.ClientName ?? string.Empty,
                Database = logEntity.Database ?? string.Empty,
                DataSource = logEntity.DataSource ?? string.Empty,
                EnvironmentName = logEntity.EnvironmentName ?? string.Empty,
                GroupGuid = logEntity.GroupGuid,
                HappenTime = System.ExtMethods.ToTimeStamp(logEntity.HappenTime, true),
                Ip = logEntity.Ip ?? string.Empty,
                Level = (int)logEntity.Level,
                LogType = (int)logEntity.LogType,
                Message = logEntity.Message ?? string.Empty,
                MethodName = logEntity.MethodName ?? string.Empty,
                Parameters = logEntity.Parameters ?? string.Empty,
                StackTrace = logEntity.StackTrace ?? string.Empty,
                StatuCode = logEntity.StatuCode,
                PageId = logEntity.PageId ?? string.Empty,
                TraceId = logEntity.TraceId ?? string.Empty,
                ParentGuid = logEntity.ParentGuid ?? string.Empty,
                HttpId = logEntity.HttpId ?? string.Empty,
                ParentHttpId = logEntity.ParentHttpId ?? string.Empty
            };
            LogContainer.Enqueue(logDtoItem);
        }
    }
}
