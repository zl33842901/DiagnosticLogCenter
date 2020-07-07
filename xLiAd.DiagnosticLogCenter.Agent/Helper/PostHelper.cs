using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using xLiAd.DiagnosticLogCenter.Abstract;

namespace xLiAd.DiagnosticLogCenter.Agent.Helper
{
    public static class PostHelper
    {
        private static string address;
        private static string clientName;
        private static string evn;
        private static int timeoutBySeconds;
        private static ConcurrentQueue<LogDtoItem> LogContainer = new ConcurrentQueue<LogDtoItem>();
        private static System.Timers.Timer Timer = new System.Timers.Timer();

        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            LogDto dto = new LogDto();
            while (LogContainer.TryDequeue(out var log))
            {
                dto.Items.Add(log);
            }
            if (!dto.Items.AnyX())
                return;
            try
            {
                var channel = new Channel(address, ChannelCredentials.Insecure);
                var grpcClient = new Diagloger.DiaglogerClient(channel);
                grpcClient.PostLog(dto, new CallOptions(null, DateTime.Now.ToUniversalTime().AddSeconds(timeoutBySeconds)));
            }
            catch { }
        }

        public static void Init(string _address, string _clientName, string _envName, int _timeoutBySeconds)
        {
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
                StatuCode = logEntity.StatuCode
            };
            LogContainer.Enqueue(logDtoItem);
        }
    }
}
