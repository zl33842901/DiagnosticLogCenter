using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using xLiAd.DiagnosticLogCenter.UserInterfaceByEs.Models;
using xLiAd.DiagnosticLogCenter.UserInterfaceByEs.Services;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace xLiAd.DiagnosticLogCenter.UserInterfaceByEs.Controllers
{
    public class LogController : Controller
    {
        private readonly ILogReadService logService;
        private readonly IConfigService configService;
        private readonly ConfigEntity configEntity;
        public LogController(ILogReadService logService, ConfigEntity configEntity, IConfigService configService)
        {
            this.logService = logService;
            this.configEntity = configEntity;
            this.configService = configService;
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
            (var l, var count) = await logService.GetLogData(query, query.PageIndex, query.PageSize);
            return Json(new { Succes = true, Items = l, Total = count });
        }
    }

}
