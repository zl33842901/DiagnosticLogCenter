using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using xLiAd.MongoEx.Repository;

namespace xLiAd.DiagnosticLogCenter.CollectServer.Repositories
{
    public class EnvironmentRepository : MongoRepository<Models.Environment>, IEnvironmentRepository
    {
        public EnvironmentRepository(MongoUrl mongoUrl) : base(mongoUrl)
        {

        }
    }

    public interface IEnvironmentRepository : IRepository<Models.Environment>
    {

    }
}
