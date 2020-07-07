using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using xLiAd.DiagnosticLogCenter.UserInterface.Models;
using xLiAd.DiagnosticLogCenter.UserInterface.Services;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace xLiAd.DiagnosticLogCenter.UserInterface.Controllers
{
    public class EnvironmentController : Controller
    {
        readonly IConfigService configService;
        public EnvironmentController(IConfigService configService)
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
            var l = (await configService.GetAllEnvironments()).ToList();
            return Json(l);
        }
        [HttpPost]
        public async Task<CallBackEntity> Add(EnvironmentAddDto clients)
        {
            var c = new Models.Environment() { Name = clients.Name };
            var i = await configService.AddEnvironment(c);
            if (!i.NullOrEmpty())
                return string.Empty;
            else
                return "添加时出现了未知错误（可能是已存在此环境），请联系管理员";
        }
        [HttpPost]
        public async Task<CallBackEntity> Edit(EnvironmentEditDto clients)
        {
            var b = await configService.EditEnvironment(new Models.Environment() { Name = clients.Name, Id = clients.Id });
            return b ? string.Empty : "编辑时出现了未知错误，可能是未找到环境";
        }
        [HttpPost]
        public async Task<CallBackEntity> Delete(string Id)
        {
            await configService.DeleteEnvironment(Id);
            return string.Empty;
        }
    }
    public class EnvironmentAddDto
    {
        public string Name { get; set; }
    }
    public class EnvironmentEditDto : EnvironmentAddDto
    {
        public string Id { get; set; }
    }
}
