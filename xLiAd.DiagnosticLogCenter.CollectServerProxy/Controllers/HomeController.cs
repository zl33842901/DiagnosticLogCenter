using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using xLiAd.DiagnosticLogCenter.Abstract;

namespace xLiAd.DiagnosticLogCenter.CollectServerProxy.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Index([FromBody]LogEntity logEntity)
        {
            PostHelper.ProcessLog(logEntity);
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult Multi([FromBody] LogEntity[] logEntity)
        {
            foreach (var item in logEntity)
                PostHelper.ProcessLog(item);
            return Json(new { success = true });
        }
    }
}