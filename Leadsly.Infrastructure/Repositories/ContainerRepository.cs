using Leadsly.Domain.Repositories;
using Leadsly.Models.Entities;
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

        public async Task<List<DockerContainerInfo>> GetContainersByUserId(string userId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<DockerContainerInfo> GetContainerById(string id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
