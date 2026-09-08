using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using xLiAd.DiagnosticLogCenter.UserInterface.Models;
using xLiAd.DiagnosticLogCenter.UserInterface.Services;
using xLiAd.DiagnosticLogCenter.UserInterfaceBoth;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace xLiAd.DiagnosticLogCenter.UserInterface.Controllers
{
    public class LogController : Controller
    {
        private readonly ILogReadService logService;
        private readonly IConfigService configService;
        private readonly ConfigEntity configEntity;
        private readonly ITraceAndPageService traceAndPageService;
        public LogController(ILogReadService logService, ConfigEntity configEntity, IConfigService configService, ITraceAndPageService traceAndPageService)
        {
            this.logService = logService;
            this.configEntity = configEntity;
            this.configService = configService;
            this.traceAndPageService = traceAndPageService;
        }
        // GET: /<controller>/
        [Route("[controller]/[action]/{ClientName}/{EnvName}/{Date}")]
        public IActionResult Look(string ClientName, string EnvName, DateTime Date, string key)
        {
            ViewBag.ClientName = ClientName;
            ViewBag.EnvName = EnvName;
            ViewBag.Date = Date.ToString("yyyy-MM-dd");
            ViewBag.Key = key;
            return View();
        }
        // GET: /<controller>/
        [Route("[controller]/[action]/{ClientName}/{EnvName}")]
        public IActionResult LookMultiDate(string ClientName, string EnvName, string key)
        {
            ViewBag.ClientName = ClientName;
            ViewBag.EnvName = EnvName;
            ViewBag.Key = key;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Look(LogLookQuery query)
        {
            try
            {
                var client = (await configService.GetAllClients()).Where(x => x.Name == query.ClientName).FirstOrDefault();
                if (client == null)
                    return Json(new { Succes = false, Message = "未找到此客户端配置" });
                var p = query.GetIndexName();
                (var l, var count) = logService.GetLogData(query, query.PageIndex, query.PageSize);
                l.ProcessEndAndException();
                return Json(new { Succes = true, Items = l, Total = count });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message, ex.StackTrace });
            }
        }
        [HttpPost]
        public async Task<IActionResult> LookMultiDate(LogLookQuery query)
        {
            try
            {
                if(query.HappenTimeRegion == null || !query.HappenTimeRegion.Any())
                    return Json(new { Succes = false, Message = "请指定查询日期" });
                if (query.Key.NullOrEmpty())
                    return Json(new { Succes = false, Message = "请指定关键字" });
                var client = (await configService.GetAllClients()).Where(x => x.Name == query.ClientName).FirstOrDefault();
                if (client == null)
                    return Json(new { Succes = false, Message = "未找到此客户端配置" });
                var p = query.GetIndexName();
                var start = query.HappenTimeRegion.FirstOrDefault();
                var end = query.HappenTimeRegion.LastOrDefault();
                List<Log> result = new List<Log>();
                var curDate = start.Date;
                while(curDate <= end.Date)
                {
                    query.HappenTime = curDate;
                    query.HappenTimeRegion = new DateTime[0];
                    (var l, var count) = logService.GetLogData(query, 1, 10000);
                    l.ProcessEndAndException();
                    result.AddRange(l);
                    curDate = curDate.AddDays(1);
                }
                return Json(new { Succes = true, Items = result, Total = result.Count });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Message = ex.Message, ex.StackTrace });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetTracePageExist(string traceId, string pageId, string guid, DateTime happenTime)
        {
            var result = await traceAndPageService.GetTracePageExist(traceId, pageId, guid, happenTime);
            return Json(new { trace = result.Item1, page = result.Item2 });
        }

        public async Task<IActionResult> GetTraceModel(string traceId, DateTime happenTime)
        {
            var result = await traceAndPageService.GetTraceAll(traceId, happenTime);
            return Json(new { result = true, data = result });
        }
    }

}
