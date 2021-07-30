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
        public IActionResult Look(string ClientName, string EnvName, DateTime Date)
        {
            ViewBag.ClientName = ClientName;
            ViewBag.EnvName = EnvName;
            ViewBag.Date = Date.ToString("yyyy-MM-dd");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Look(LogLookQuery query)
        {
            var client = (await configService.GetAllClients()).Where(x => x.Name == query.ClientName).FirstOrDefault();
            if(client == null)
                return Json(new { Succes = false, Message = "未找到此客户端配置" });
            var p = query.GetIndexName();
            (var l, var count) = logService.GetLogData(query, query.PageIndex, query.PageSize);
            l.ProcessEndAndException();
            return Json(new { Succes = true, Items = l, Total = count });
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
