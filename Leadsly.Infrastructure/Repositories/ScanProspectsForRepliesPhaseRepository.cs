using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Infrastructure.Repositories
{
    public class ScanProspectsForRepliesPhaseRepository : IScanProspectsForRepliesPhaseRepository
    {
        public ScanProspectsForRepliesPhaseRepository(DatabaseContext dbContext, ILogger<ScanProspectsForRepliesPhaseRepository> logger)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private readonly DatabaseContext _dbContext;
        private readonly ILogger<ScanProspectsForRepliesPhaseRepository> _logger;
        public async Task<ScanProspectsForRepliesPhase> CreateAsync(ScanProspectsForRepliesPhase phase, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating ScanProspectsForRepliesPhase.");
            try
            {
                _dbContext.ScanProspectsForRepliesPhase.Add(phase);
                await _dbContext.SaveChangesAsync(ct);
                string phaseId = phase.ScanProspectsForRepliesPhaseId;
                _logger.LogDebug("Successfully created ScanProspectsForRepliesPhase. New ScanProspectsForRepliesPhase id is {phaseId}", phaseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create ScanProspectsForRepliesPhase. Returning an explict null");
                return null;
            }
            return phase;
        }

        public async Task<ScanProspectsForRepliesPhase> GetByIdAsync(string scanProspectsForRepliesPhaseId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving ScanProspectsForRepliesPhase by id {scanProspectsForRepliesPhaseId}", scanProspectsForRepliesPhaseId);
            ScanProspectsForRepliesPhase phase = default;
            try
            {
                phase = await _dbContext.ScanProspectsForRepliesPhase
                    .Where(p => p.ScanProspectsForRepliesPhaseId == scanProspectsForRepliesPhaseId)
                    .Include(p => p.SocialAccount)
                    .SingleAsync(ct);

                _logger.LogDebug("Successfully retrieved ScanProspectsForRepliesPhase by id {scanProspectsForRepliesPhaseId}", scanProspectsForRepliesPhaseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve ScanProspectsForRepliesPhase. Returning an explict null");
                return null;
            }
            return phase;
        }
    }
}
