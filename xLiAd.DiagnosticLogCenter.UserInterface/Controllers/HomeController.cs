using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using xLiAd.DiagnosticLogCenter.UserInterface.Models;
using xLiAd.DiagnosticLogCenter.UserInterface.Services;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace xLiAd.DiagnosticLogCenter.UserInterface.Controllers
{
    public class HomeController : Controller
    {
        readonly IConfigService configService;
        readonly ILogReadService logService;
        readonly ConfigEntity configEntity;
        private readonly IHttpClientFactory clientFactory;
        public HomeController(IConfigService configService, ILogReadService logService, ConfigEntity configEntity, IHttpClientFactory clientFactory)
        {
            this.configService = configService;
            this.logService = logService;
            this.configEntity = configEntity;
            this.clientFactory = clientFactory;
        }
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetIndexData(IndexDataQuery query)
        {
            var lc = await configService.GetAllClients();
            var le = await configService.GetAllEnvironments();
            List<CliEvnDate> lced = new List<CliEvnDate>();
            foreach(var c in lc)
            {
                foreach(var e in le)
                {
                    var rr = new CliEvnDate() { ClientName = c.Name, EnvironmentName = e.Name, HappenTime = query.Date };
                    bool b = logService.Exist(rr);
                    rr.Exist = b;
                    lced.Add(rr);
                }
            }
            return Json(lced);
        }
        [HttpPost]
        [Route("[controller]/[action]/{Date}")]
        public IActionResult GetPrevDay(string Date)
        {
            var D = DateTime.TryParse(Date, out var a) ? a : DateTime.Today;
            D = D.AddDays(-1);
            return Json(new { Date = D.ToString("yyyy-MM-dd") });
        }
        [HttpPost]
        [Route("[controller]/[action]/{Date}")]
        public IActionResult GetNextDay(string Date)
        {
            var D = DateTime.TryParse(Date, out var a) ? a : DateTime.Today;
            D = D.AddDays(1);
            return Json(new { Date = D.ToString("yyyy-MM-dd") });
        }
    }

    public class IndexDataQuery
    {
        public DateTime Date { get; set; }
    }
}
