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
        private readonly IEnvironmentRepository environments;
        private readonly IClientRepository clients;
        public ConfigService(IClientRepository clients, IEnvironmentRepository environments)
        {
            this.environments = environments;
            this.clients = clients;
        }

        public async Task<string> AddClient(Clients client)
        {
            return (await clients.AddAsync(client)).Id;
        }

        public async Task<string> AddEnvironment(Models.Environment environment)
        {
            var result = await environments.AddAsync(environment);
            return result.Id;
        }

        public async Task<bool> DeleteClient(string Id)
        {
            var result = await clients.DeleteAsync(x => x.Id == Id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> DeleteEnvironment(string Id)
        {
            var result = await environments.DeleteAsync(x => x.Id == Id);
            return result.DeletedCount > 0;
        }

        public async Task<bool> EditClient(Clients client)
        {
            var result = await clients.UpdateAsync(x => x.Id == client.Id,
                Builders<Clients>.Update
                .Set(x => x.Name, client.Name)
                .Set(x => x.ModifiedOn, DateTime.Now));
            return result.ModifiedCount > 0;
        }

        public async Task<bool> EditEnvironment(Models.Environment environment)
        {
            var result = await environments.UpdateAsync(x => x.Id == environment.Id,
                Builders<Models.Environment>.Update.Set(x => x.Name, environment.Name)
                .Set(x => x.ModifiedOn, DateTime.Now));
            return result.ModifiedCount > 0;
        }

        public async Task<Clients[]> GetAllClients()
        {
            var result = await clients.AllAsync();
            return result.ToArray();
        }

        public async Task<Models.Environment[]> GetAllEnvironments()
        {
            var result = await environments.AllAsync();
            return result.ToArray();
        }
    }

    public interface IConfigService
    {
        Task<Clients[]> GetAllClients();
        Task<Models.Environment[]> GetAllEnvironments();
        Task<string> AddClient(Clients clients);
        Task<bool> EditClient(Clients clients);
        Task<string> AddEnvironment(Models.Environment environment);
        Task<bool> DeleteClient(string Id);
        Task<bool> DeleteEnvironment(string Id);
        Task<bool> EditEnvironment(Models.Environment environment);
    }
}
