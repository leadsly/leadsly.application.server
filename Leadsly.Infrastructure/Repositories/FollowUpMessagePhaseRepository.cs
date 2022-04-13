using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Infrastructure.Repositories
{
    public class FollowUpMessagePhaseRepository : IFollowUpMessagePhaseRepository
    {
        public FollowUpMessagePhaseRepository(DatabaseContext dbContext, ILogger<FollowUpMessagePhaseRepository> logger)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private readonly DatabaseContext _dbContext;
        private readonly ILogger<FollowUpMessagePhaseRepository> _logger;

        public async Task<FollowUpMessagePhase> GetByCampaignIdAsync(string campaignId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieiving FollowUpMessagePhase by campaign id {campaignId}", campaignId);
            FollowUpMessagePhase phase = default;
            try
            {
                phase = await _dbContext.FollowUpMessagesPhases.Where(p => p.CampaignId == campaignId).Include(p => p.Campaign).SingleAsync(ct);
                _logger.LogDebug("Successfully retrieved FollowUpMessagePhase by campaign id {campaignId}", campaignId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve FollowUpMessagePhase by campaign id {campaignId}. Returning an explicit null", campaignId);
                return null;
            }
            return phase;
        }
    }
}
