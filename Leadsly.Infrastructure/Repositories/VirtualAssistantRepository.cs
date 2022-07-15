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
                    .Include(v => v.CloudMapDiscoveryService)
                    .Include(v => v.EcsService)
                    .Include(v => v.EcsTaskDefinition)
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
    }
}
