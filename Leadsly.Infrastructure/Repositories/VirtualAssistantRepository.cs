using Leadsly.Application.Model.Entities;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
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
    }
}
