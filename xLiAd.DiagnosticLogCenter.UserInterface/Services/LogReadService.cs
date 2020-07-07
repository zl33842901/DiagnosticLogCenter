using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.Abstract;
using xLiAd.DiagnosticLogCenter.UserInterface.Models;
using xLiAd.DiagnosticLogCenter.UserInterface.Repositories;

namespace xLiAd.DiagnosticLogCenter.UserInterface.Services
{
    public class LogReadService : ILogReadService
    {
        private readonly ILogRepository logRepository;
        public LogReadService(ILogRepository logRepository)
        {
            this.logRepository = logRepository;
        }

        public (TongJiResult, TongJiInterfaceResult, TongJiInterfaceResult, TongJiInterfaceResult) GetTongJi(Models.ICliEnvDate cliEnv)
        {
            var ie = logRepository.GetAllData(cliEnv);
            var iee = ie.Where(x => x.Level == LogLeveEnum.Error);//代表异常日志
            TongJiResult r = new TongJiResult
            {
                ClientName = cliEnv.ClientName,
                EnvironmentName = cliEnv.EnvironmentName,
                HappenTime = cliEnv.HappenTime,
                Count = iee.Count(),
                Items = iee.GroupBy(x => x.Message).Select(x => new TongJiItem() { Message = x.Key, Count = x.Count() }).OrderByDescending(x => x.Count).ToList()
            };
            //r代表异常日志的结果
            var ieie = ie.Where(x => !x.Success);
            TongJiInterfaceResult tongJiResultError = BornTongJi("异常接口", ieie);
            var ieis = ie.Where(x => x.Success && x.TotalMillionSeconds > 999);
            TongJiInterfaceResult tongJiResultTimeout = BornTongJi("成功但执行时间较长的", ieis);
            TongJiInterfaceResult tongJiResult = BornTongJi("总的接口", ie);
            return (r, tongJiResultError, tongJiResultTimeout, tongJiResult);
        }
        /// <summary>
        /// 这是统计接口日志的方法
        /// </summary>
        /// <param name="name"></param>
        /// <param name="l"></param>
        /// <returns></returns>
        private TongJiInterfaceResult BornTongJi(string name, IEnumerable<Log> l)
        {
            TongJiInterfaceResult tongJiResult = new TongJiInterfaceResult()
            {
                Title = "以下是" + name + "统计情况，共执行调用" + l.Count() + "次",
                Items = l.GroupBy(x => x.Message).Select(x => new TongJiInterfaceItem()
                {
                    MethodName = x.Key,
                    Count = x.Count(),
                    AvgSeconds = Convert.ToInt32(x.Average(y => y.TotalMillionSeconds) / 1000)
                }).OrderByDescending(x => x.Count).ToList()
            };
            return tongJiResult;
        }


        public (List<Log>, long) GetLogData(LogLookQuery query, int pageIndex, int pageSize)
        {
            return logRepository.GetLogData(query, pageIndex, pageSize);
        }
        public bool Exist(ICliEnvDate cli)
        {
            return logRepository.Exist(cli);
        }
    }
    public interface ILogReadService
    {
        (List<Log>, long) GetLogData(LogLookQuery query, int pageIndex, int pageSize);
        (TongJiResult, TongJiInterfaceResult, TongJiInterfaceResult, TongJiInterfaceResult) GetTongJi(Models.ICliEnvDate cliEnv);
        //Log GetModel(Entities.ICliEnvDate cliEnv, int id);
        bool Exist(ICliEnvDate cli);
    }
}
