using Leadsly.Application.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IOrphanedCloudResourcesRepository
    {
        Task<OrphanedCloudResource> AddOrphanedCloudResourceAsync(OrphanedCloudResource orphanedResource, CancellationToken ct = default);
    }
}
