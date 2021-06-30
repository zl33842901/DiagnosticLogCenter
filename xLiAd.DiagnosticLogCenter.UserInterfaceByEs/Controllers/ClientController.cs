using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using xLiAd.DiagnosticLogCenter.UserInterfaceByEs.Models;
using xLiAd.DiagnosticLogCenter.UserInterfaceByEs.Services;

namespace xLiAd.DiagnosticLogCenter.UserInterfaceByEs.Controllers
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
        //[HttpPost]
        //public async Task<CallBackEntity> Delete(string Id)
        //{
        //    await configService.DeleteClient(Id);
        //    return string.Empty;
        //}
    }
}