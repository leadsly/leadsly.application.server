using Leadsly.Domain.Models.Entities.Campaigns;
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
    public class SendConnectionsPhaseRepository : ISendConnectionsPhaseRepository
    {
        public SendConnectionsPhaseRepository(DatabaseContext dbContext, ILogger<SendConnectionsPhaseRepository> logger)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private readonly DatabaseContext _dbContext;
        private readonly ILogger<SendConnectionsPhaseRepository> _logger;

        public async Task<IList<SendConnectionsStage>> GetStagesByCampaignIdAsync(string campaignId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving SendConnectionsStages by campaign id {campaignId}", campaignId);
            IList<SendConnectionsStage> sendConnectionsStages = default;
            try
            {
                sendConnectionsStages = await _dbContext.SendConnectionsStages
                    .Where(s => s.CampaignId == campaignId)
                    .ToListAsync(ct);

                _logger.LogDebug("Successfully retrieved all SendConnectionsStages by campaign id {campaignId}", campaignId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all SendConnectionsStages by campaign id {campaignId}. Returning an explicit null", campaignId);
                return null;
            }

            return sendConnectionsStages;
        }
    }
}
