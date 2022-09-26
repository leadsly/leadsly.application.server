using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Infrastructure.Repositories
{
    public class VirtualAssistantRepository : IVirtualAssistantRepository
    {
        public VirtualAssistantRepository(DatabaseContext dbContext, ILogger<VirtualAssistantRepository> logger)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private readonly DatabaseContext _dbContext;
        private readonly ILogger<VirtualAssistantRepository> _logger;

        private async Task<bool> VirtualAssistantExists(string id, CancellationToken ct = default)
        {
            return await _dbContext.VirtualAssistants.AnyAsync(v => v.VirtualAssistantId == id, ct);
        }

        public async Task<VirtualAssistant> CreateAsync(VirtualAssistant newVirtualAssistant, CancellationToken ct = default)
        {
            try
            {
                _dbContext.VirtualAssistants.Add(newVirtualAssistant);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add new virtual assistant");
            }
            return newVirtualAssistant;
        }

        public async Task<IList<VirtualAssistant>> GetAllByUserIdAsync(string userId, CancellationToken ct = default)
        {
            IList<VirtualAssistant> virtualAssistants = null;
            try
            {
                virtualAssistants = await _dbContext.VirtualAssistants
                    .Where(v => v.ApplicationUserId == userId)
                    .Include(v => v.SocialAccount)
                    .Include(v => v.HalUnit)
                    .Include(v => v.CloudMapDiscoveryServices)
                    .Include(v => v.EcsServices)
                    .Include(v => v.EcsTaskDefinitions)
                    .ToListAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all virtual assistants by user id {userId}", userId);
            }
            return virtualAssistants;
        }

        public async Task<bool> DeleteAsync(string virtualAssistantId, CancellationToken ct = default)
        {
            if (!await VirtualAssistantExists(virtualAssistantId, ct))
            {
                return false;
            }
            VirtualAssistant toRemove = _dbContext.VirtualAssistants.Find(virtualAssistantId);
            _dbContext.VirtualAssistants.Remove(toRemove);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<VirtualAssistant> GetByHalIdAsync(string halId, CancellationToken ct = default)
        {
            VirtualAssistant virtualAssistant = null;
            try
            {
                virtualAssistant = await _dbContext.VirtualAssistants
                    .Where(v => v.HalId == halId)
                    .Include(v => v.SocialAccount)
                    .Where(v => v.SocialAccount.Linked == true && v.HalUnit.HalId == halId)
                    .Include(v => v.HalUnit)
                    .Include(v => v.CloudMapDiscoveryServices)
                    .Include(v => v.EcsServices)
                        .ThenInclude(s => s.EcsTasks)
                    .Include(v => v.EcsTaskDefinitions)
                    .FirstOrDefaultAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve virtual assistant by hal id {halId}", halId);
            }
            return virtualAssistant;
        }

        public async Task<VirtualAssistant> UpdateAsync(VirtualAssistant updated, CancellationToken ct = default)
        {
            _logger.LogInformation("Updating {0} with HalId {1}", nameof(VirtualAssistant), updated.HalId);
            try
            {
                _dbContext.VirtualAssistants.Update(updated);
                await _dbContext.SaveChangesAsync(ct);
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
