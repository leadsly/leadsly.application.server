using Leadsly.Domain.Repositories;
using Leadsly.Application.Model.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Infrastructure.Repositories
{
    public class OrphanedCloudResourcesRepository : IOrphanedCloudResourcesRepository
    {
        public OrphanedCloudResourcesRepository(DatabaseContext databaseContext, ILogger<OrphanedCloudResourcesRepository> logger)
        {
            _dbContext = databaseContext;
            _logger = logger;
        }

        private readonly DatabaseContext _dbContext;
        private readonly ILogger<OrphanedCloudResourcesRepository> _logger;

        public async Task<OrphanedCloudResource> AddOrphanedCloudResourceAsync(OrphanedCloudResource orphanedResource, CancellationToken ct = default)
        {
            try
            {
                _dbContext.OrphanedCloudResources.Add(orphanedResource);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to add orphaned cloud resources to the database.");
            }
            
            return orphanedResource;
        }
    }
}
