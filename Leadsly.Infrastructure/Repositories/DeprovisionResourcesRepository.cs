using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Infrastructure.Repositories
{
    public class DeprovisionResourcesRepository : IDeprovisionResourcesRepository
    {
        private readonly Func<DatabaseContext> _dbContextFactory;
        private readonly ILogger<DeprovisionResourcesRepository> _logger;

        public DeprovisionResourcesRepository(Func<DatabaseContext> dbContextFactory, ILogger<DeprovisionResourcesRepository> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }


        public async Task<VirtualAssistant> GetVirtualAssistantByHalIdAsync(string halId, CancellationToken ct = default)
        {
            VirtualAssistant virtualAssistant = null;
            try
            {
                using (DatabaseContext dbContext = _dbContextFactory())
                {
                    virtualAssistant = await dbContext.VirtualAssistants
                                        .Where(v => v.HalId == halId)
                                        .Include(v => v.CloudMapDiscoveryServices)
                                        .Include(v => v.EcsServices)
                                            .ThenInclude(s => s.EcsTasks)
                                        .Include(v => v.EcsTaskDefinitions)
                                        .FirstOrDefaultAsync(ct);
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve virtual assistant by hal id {halId}", halId);
            }
            return virtualAssistant;
        }

        public async Task<VirtualAssistant> UpdateVirtualAssistantAsync(VirtualAssistant updated, CancellationToken ct = default)
        {
            _logger.LogInformation("Updating {0} with HalId {1}", nameof(VirtualAssistant), updated.HalId);
            try
            {
                using (DatabaseContext dbContext = _dbContextFactory())
                {
                    dbContext.VirtualAssistants.Update(updated);
                    await dbContext.SaveChangesAsync(ct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update {0}", nameof(VirtualAssistant));
                return null;
            }
            return updated;
        }
    }
}
