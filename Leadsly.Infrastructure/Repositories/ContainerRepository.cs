using Leadsly.Domain.Repositories;
using Leadsly.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Infrastructure.Repositories
{
    public class ContainerRepository : IContainerRepository
    {
        public ContainerRepository(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        private readonly DatabaseContext _databaseContext;

        public Task<List<DockerContainerInfo>> GetContainersByUserId(string userId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
