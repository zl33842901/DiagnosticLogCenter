using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using xLiAd.DiagnosticLogCenter.UserInterface.Models;
using xLiAd.DiagnosticLogCenter.UserInterface.Services;

namespace xLiAd.DiagnosticLogCenter.UserInterface.Controllers
{
    public class ClientController : Controller
    {
        readonly IConfigService configService;
        public ClientController(IConfigService configService)
        {
            this.configService = configService;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GetIndexData()
        {
            var l = (await configService.GetAllClients()).ToList();
            return Json(l);
        }
    }
}