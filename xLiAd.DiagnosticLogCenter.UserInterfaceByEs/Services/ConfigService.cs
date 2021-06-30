using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.UserInterfaceByEs.Models;
using xLiAd.DiagnosticLogCenter.UserInterfaceByEs.Repositories;

namespace xLiAd.DiagnosticLogCenter.UserInterfaceByEs.Services
{
    public class ConfigService : IConfigService
    {
        private readonly IClientRepository clients;
        public ConfigService(IClientRepository clients)
        {
            this.clients = clients;
        }

        //public async Task<bool> DeleteClient(string Id)
        //{
        //    var result = await clients.DeleteAsync(x => x.Id == Id);
        //    return result.DeletedCount > 0;
        //}

        public async Task<Client[]> GetAllClients()
        {
            var result = await clients.AllAsync();
            return result.ToArray();
        }
    }

    public interface IConfigService
    {
        Task<Client[]> GetAllClients();
        //Task<bool> DeleteClient(string Id);
    }
}
