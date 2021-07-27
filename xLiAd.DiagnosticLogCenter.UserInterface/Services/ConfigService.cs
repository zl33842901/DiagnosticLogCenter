using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.UserInterface.Models;
using xLiAd.DiagnosticLogCenter.UserInterface.Repositories;

namespace xLiAd.DiagnosticLogCenter.UserInterface.Services
{
    public class ConfigService : IConfigService
    {
        private readonly IClientRepository clients;
        public ConfigService(IClientRepository clients)
        {
            this.clients = clients;
        }

        //public async Task<string> AddClient(Clients client)
        //{
        //    return (await clients.AddAsync(client)).Id;
        //}

        //public async Task<bool> DeleteClient(string Id)
        //{
        //    var result = await clients.DeleteAsync(x => x.Id == Id);
        //    return result.DeletedCount > 0;
        //}

        //public async Task<bool> EditClient(Clients client)
        //{
        //    var result = await clients.UpdateAsync(x => x.Id == client.Id,
        //        Builders<Clients>.Update
        //        .Set(x => x.Name, client.Name)
        //        .Set(x => x.ModifiedOn, DateTime.Now));
        //    return result.ModifiedCount > 0;
        //}

        public async Task<Clients[]> GetAllClients()
        {
            var result = await clients.AllAsync();
            return result.ToArray();
        }
    }

    public interface IConfigService
    {
        Task<Clients[]> GetAllClients();
        //Task<string> AddClient(Clients clients);
        //Task<bool> EditClient(Clients clients);
        //Task<bool> DeleteClient(string Id);
    }
}
