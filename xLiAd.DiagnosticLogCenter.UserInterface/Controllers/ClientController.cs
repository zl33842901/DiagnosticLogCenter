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
        [HttpPost]
        public async Task<CallBackEntity> Add(ClientAddDto clients)
        {
            var c = clients.ToClient();
            var i = await configService.AddClient(c);
            if (!i.NullOrEmpty())
                return string.Empty;
            else
                return "添加时出现了未知错误（可能是已存在此客户端），请联系管理员";
        }
        [HttpPost]
        public async Task<CallBackEntity> Edit(ClientEditDto clients)
        {
            var c = clients.ToClient();
            var b = await configService.EditClient(c);
            return b ? string.Empty : "编辑时出现了未知错误，可能是未找到客户端";
        }
        [HttpPost]
        public async Task<CallBackEntity> Delete(string Id)
        {
            await configService.DeleteClient(Id);
            return string.Empty;
        }
    }

    public class ClientAddDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string DomainAccount { get; set; }
        public string Mobile { get; set; }
        public virtual Clients ToClient()
        {
            var result = new Clients() {
                Name = this.Name
            };
            return result;
        }
    }
    public class ClientEditDto : ClientAddDto
    {
        public string Id { get; set; }
        public override Clients ToClient()
        {
            var result = base.ToClient();
            result.Id = this.Id;
            return result;
        }
    }
}