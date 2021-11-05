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
            rabbit = new MqConnAndChannel(config);
            var consumer = new EventingBasicConsumer(rabbit.Channel);
            consumer.Received += Consume;
            rabbit.Channel.BasicConsume(config.RabbitMqQueueName, false, consumerTag: "我是日志实时分析系统", noLocal: false, exclusive: false, arguments: null, consumer);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000);
            }
        }

        private readonly ConcurrentDictionary<string, ConcurrentBag<LogModel>> DataHolder = new ConcurrentDictionary<string, ConcurrentBag<LogModel>>();

        private void Consume(object model, BasicDeliverEventArgs eventArgs)
        {
            using var scope = serviceProvider.CreateScope();
            var sp = scope.ServiceProvider;
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
                var baseTime = dto.HappenTime.AddMinutes(-1);
                var callCount = DataHolder[key].Count(x => x.HappenTime > baseTime);
                var failCount = DataHolder[key].Count(x => x.HappenTime > baseTime && !x.Success);
                var messageCallCount = DataHolder[key].Count(x => x.HappenTime > baseTime && x.Message == dto.Message);
                var messageFailCount = DataHolder[key].Count(x => x.HappenTime > baseTime && x.Message == dto.Message && !x.Success);
                var messageNeedAlert = (messageFailCount * 100 / messageCallCount) > 20 && messageFailCount >= 3;//暂时的规则：错误率 20%以上 且采样数至少3个
                if (messageNeedAlert)
                {
                    var svc = sp.GetService<IAlertServicecs>();
                    svc.Alert(dto.ClientName, dto.EnvironmentName, dto.Message, 1, messageCallCount, messageFailCount);
                }
                else
                {
                    var needAlert = (failCount * 100 / callCount) > 5 && failCount >= 5;
                    if(needAlert)
                    {
                        var svc = sp.GetService<IAlertServicecs>();
                        svc.Alert(dto.ClientName, dto.EnvironmentName, "", 1, callCount, failCount);
                    }
                }
            }
            rabbit.Channel.BasicAck(eventArgs.DeliveryTag, false);
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
