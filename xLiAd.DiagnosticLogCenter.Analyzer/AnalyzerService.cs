using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace xLiAd.DiagnosticLogCenter.Analyzer
{
    public class AnalyzerService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private MqConnAndChannel rabbit;
        public AnalyzerService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = serviceProvider.GetService<AnalyzerConfig>();
            rabbit = new MqConnAndChannel(config, Consume);
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000);
            }
            Console.WriteLine("哇，AnalyzerService 退出了，说明 stoppingToken.IsCancellationRequested 刚刚为真了，现在是：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        private readonly ConcurrentDictionary<string, ConcurrentBag<LogModel>> DataHolder = new ConcurrentDictionary<string, ConcurrentBag<LogModel>>();

        private void Consume(object model, BasicDeliverEventArgs eventArgs)
        {
            using var scope = serviceProvider.CreateScope();
            var sp = scope.ServiceProvider;
            try
            {
                var body = eventArgs.Body;
                var message = Encoding.UTF8.GetString(body.ToArray());
                if (string.IsNullOrEmpty(message))
                    rabbit.Channel.BasicAck(eventArgs.DeliveryTag, false);
                var dto = Newtonsoft.Json.JsonConvert.DeserializeObject<LogModel>(message);
                var key = dto.ClientName + "|" + dto.EnvironmentName;
                if (DataHolder.ContainsKey(key))
                    DataHolder[key].Add(dto);
                else
                    DataHolder.TryAdd(key, new ConcurrentBag<LogModel>() { dto });
                DataHolder[key] = new ConcurrentBag<LogModel>(DataHolder[key].Where(x => x.HappenTime > DateTime.Now.AddMinutes(-80)));//只留80分钟内的
                if (!dto.Success && dto.HappenTime > DateTime.Now.AddMinutes(-5))//只有未成功时需要处理，再过滤掉可能是缓存的；假设服务器间时间差不大于5分钟。
                {
                    var svc = sp.GetService<IAlertServicecs>();
                    var config = svc.GetAlertConfig(dto.ClientName);
                    var baseTime = dto.HappenTime.AddMinutes(-config.MinutesCatchedForProcess);
                    var callCount = DataHolder[key].Count(x => x.HappenTime > baseTime);
                    var failCount = DataHolder[key].Count(x => x.HappenTime > baseTime && !x.Success);
                    var messageCallCount = DataHolder[key].Count(x => x.HappenTime > baseTime && x.Message == dto.Message);
                    var messageFailCount = DataHolder[key].Count(x => x.HappenTime > baseTime && x.Message == dto.Message && !x.Success);
                    var thisMessageConfig = config.AlterSettings.Where(x => x.Message == dto.Message).FirstOrDefault();
                    if (thisMessageConfig == null)
                        thisMessageConfig = config.AlterSettings.Where(x => x.Message == "*").FirstOrDefault();
                    if (thisMessageConfig == null)
                        thisMessageConfig = new MessageAlertDetailConfig() { LowestFailRate = 20, LowestFailCount = 3 };
                    var messageNeedAlert = (messageFailCount * 100 / messageCallCount) >= thisMessageConfig.LowestFailRate && messageFailCount >= thisMessageConfig.LowestFailCount;//暂时的规则：错误率 20%以上 且采样数至少3个
                    if (messageNeedAlert)
                    {
                        svc.Alert(dto.ClientName, dto.EnvironmentName, dto.Message, 1, messageCallCount, messageFailCount);
                    }
                    else
                    {
                        var zongtiConfig = config.AlterSettings.Where(x => x.Message == "**").FirstOrDefault();
                        if (zongtiConfig == null)
                            zongtiConfig = new MessageAlertDetailConfig() { LowestFailRate = 5, LowestFailCount = 5 };
                        var needAlert = (failCount * 100 / callCount) > zongtiConfig.LowestFailRate && failCount >= zongtiConfig.LowestFailCount;
                        if (needAlert)
                        {
                            svc.Alert(dto.ClientName, dto.EnvironmentName, "", 1, callCount, failCount);
                        }
                    }
                }
                rabbit.Channel.BasicAck(eventArgs.DeliveryTag, false);
            }
            catch(Exception ex)
            {
                var logger = sp.GetService<ILogger<AnalyzerService>>();
                if(logger != null)
                {
                    logger.LogError(ex.Message);
                    logger.LogWarning(ex.StackTrace);
                }
                else
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
                rabbit.Channel.BasicReject(eventArgs.DeliveryTag, true);
            }
        }

        public List<StatisticsResultModel> Statistics(DateTime start, DateTime end)
        {
            List<StatisticsResultModel> result = new List<StatisticsResultModel>();
            foreach(var system in DataHolder)
            {
                var client = system.Key.Split('|')[0];
                var env = system.Key.Split('|')[1];
                var list = system.Value.Where(x => x.HappenTime >= start && x.HappenTime < end);
                if (!list.Any())
                    continue;
                result.Add(StatisticsOne(list, "*", start, end, client, env));
                foreach (var group in list.GroupBy(x => x.Message).OrderByDescending(x => x.Count()))
                    result.Add(StatisticsOne(group, group.Key, start, end, client, env));
            }
            return result;
        }

        private StatisticsResultModel StatisticsOne(IEnumerable<LogModel> list, string interfaceName, DateTime start, DateTime end, string client, string env)
        {
            if (list == null || !list.Any())
                throw new ArgumentNullException("list", "被分析的列表不能为空！");
            var total = list.Count();
            var failCount = list.Count(x => !x.Success);
            var maxTime = list.Max(x => x.TotalMillionSeconds);
            var minTime = list.Min(x => x.TotalMillionSeconds);
            var avgTime = list.Average(x => x.TotalMillionSeconds);
            StatisticsResultModel srst = new StatisticsResultModel()
            {
                ClientName = client,
                EnvironmentName = env,
                Start = start,
                End = end,
                Total = total,
                FailCount = failCount,
                MaxTotalMillionSeconds = maxTime,
                MinTotalMillionSeconds = minTime,
                AvgTotalMillionSeconds = avgTime,
                Interface = interfaceName
            };
            return srst;
        }
    }
}
