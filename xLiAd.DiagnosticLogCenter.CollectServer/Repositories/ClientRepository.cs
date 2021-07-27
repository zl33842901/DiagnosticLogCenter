using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.CollectServer.Models;
using xLiAd.MongoEx.Repository;

namespace xLiAd.DiagnosticLogCenter.CollectServer.Repositories
{
    public class ClientRepository : MongoRepository<Clients>, IClientRepository
    {
        public ClientRepository(MongoUrl mongoUrl) : base(mongoUrl)
        {

        }

        public async Task<bool> AddEnvAsync(string name, Models.Environment environment)
        {
            var update = Builders<Clients>.Update.Push(x => x.Environments, environment);
            var result = await UpdateAsync(x => x.Name == name, update);
            return result.ModifiedCount > 0;
        }
    }

    public interface IClientRepository : IRepository<Clients>
    {
        Task<bool> AddEnvAsync(string name, Models.Environment environment);
    }
}
