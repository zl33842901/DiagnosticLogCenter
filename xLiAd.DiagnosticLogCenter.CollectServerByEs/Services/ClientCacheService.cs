using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.CollectServerByEs.Models;
using xLiAd.DiagnosticLogCenter.CollectServerByEs.Repositories;

namespace xLiAd.DiagnosticLogCenter.CollectServerByEs.Services
{
    public class ClientCacheService : IClientCacheService
    {
        private readonly IClientRepository clientRepository;
        private const string cache_key = "BrowserLogCenter.CollectServer.ClientCacheService.GetAllClient";
        public ClientCacheService(IClientRepository clientRepository)
        {
            this.clientRepository = clientRepository;
        }

        private static readonly MemoryCache mc = new MemoryCache(new MemoryCacheOptions() { });
        private static void Set(string key, object value, TimeSpan timeSpan)
        {
            mc.Set(key, value, timeSpan);
        }

        private static object Get(string key)
        {
            return mc.Get(key);
        }

        private static void Delete(string key)
        {
            mc.Remove(key);
        }

        private async Task<Client[]> GetAllClient()
        {
            var obj = Get(cache_key) as Client[];
            if (obj == null)
            {
                obj = (await clientRepository.AllAsync()).ToArray();
                Set(cache_key, obj, TimeSpan.FromMinutes(3));
            }
            return obj;
        }

        public async Task LoadClient(Log log)
        {
            var clients = await GetAllClient();
            var client = clients.Where(x => x.Name.Equals(log.ClientName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (client == null)
            {
                await clientRepository.AddAsync(new Client()
                {
                    Name = log.ClientName,
                    DomainAccounts = new string[0],
                    Emails = new string[0],
                    Mobiles = new string[0],
                    Environments = new Models.Environment[]
                    {
                        new Models.Environment()
                        {
                            Name = log.EnvironmentName
                        }
                    }
                });
                Delete(cache_key);
            }
            else
            {
                var env = client.Environments.Where(x => x.Name.Equals(log.EnvironmentName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (env == null)
                {
                    var update = clientRepository.AddEnvAsync(client.Name, new Models.Environment() { Name = log.EnvironmentName });
                }
            }
        }
    }

    public interface IClientCacheService
    {
        Task LoadClient(Log log);
    }
}
