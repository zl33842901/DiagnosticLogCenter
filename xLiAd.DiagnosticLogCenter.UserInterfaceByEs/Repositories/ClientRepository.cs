using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.DiagnosticLogCenter.UserInterfaceByEs.Models;

namespace xLiAd.DiagnosticLogCenter.UserInterfaceByEs.Repositories
{
    public class ClientRepository : RepositoryBase<Client>, IClientRepository
    {
        public ClientRepository(ElasticClient client) : base(client, "client")
        {

        }
    }

    public interface IClientRepository : IRepository<Client>
    {

    }
}
