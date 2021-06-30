using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.CollectServerByEs.Models;

namespace xLiAd.DiagnosticLogCenter.CollectServerByEs.Repositories
{
    public class ClientRepository : RepositoryBase<Models.Client>, IClientRepository
    {
        public ClientRepository(ElasticClient client) : base(client)
        {

        }

        private async Task<Client> GetByName(string name)
        {
            var c = await client.SearchAsync<Client>(x => x.Index(indexName).Query(y => y.Match(z => z.Name(name))));
            return c.Documents.FirstOrDefault();
        }

        public async Task<bool> AddEnvAsync(string name, Models.Environment environment)
        {
            var c = await GetByName(name);
            c.Environments = c.Environments.Concat(new Models.Environment[] { environment }).ToArray();
            var result = await base.UpdateAsync(c);
            return result;
        }
    }

    public interface IClientRepository : IRepository<Client>
    {
        Task<bool> AddEnvAsync(string name, Models.Environment environment);
    }
}
