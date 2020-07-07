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
    }

    public interface IClientRepository : IRepository<Clients>
    {

    }
}
