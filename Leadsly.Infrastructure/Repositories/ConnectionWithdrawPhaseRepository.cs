using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Infrastructure.Repositories
{
    public class ConnectionWithdrawPhaseRepository : IConnectionWithdrawPhaseRepository
    {
        public ConnectionWithdrawPhaseRepository(DatabaseContext dbContext, ILogger<ConnectionWithdrawPhaseRepository> logger)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private readonly DatabaseContext _dbContext;
        private readonly ILogger<ConnectionWithdrawPhaseRepository> _logger;

        public async Task<ConnectionWithdrawPhase> CreateAsync(ConnectionWithdrawPhase phase, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating ConnectionWithdrawPhase.");
            try
            {
                _dbContext.ConnectionWithdrawPhases.Add(phase);
                await _dbContext.SaveChangesAsync(ct);
                string phaseId = phase.ConnectionWithdrawPhaseId;
                _logger.LogDebug("Successfully created ConnectionWithdrawPhase. New ConnectionWithdrawPhase id is: {phaseId}", phaseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to ConnectionWithdrawPhase. Returning an explict null");
                return null;
            }
            return phase;
        }
    }
}
