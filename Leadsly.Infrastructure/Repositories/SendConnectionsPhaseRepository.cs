using Leadsly.Application.Model.Entities.Campaigns;
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
    public class SendConnectionsPhaseRepository : ISendConnectionsPhaseRepository
    {
        public SendConnectionsPhaseRepository(DatabaseContext dbContext, ILogger<SendConnectionsPhaseRepository> logger)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private readonly DatabaseContext _dbContext;
        private readonly ILogger<SendConnectionsPhaseRepository> _logger;

        public async Task<SentConnectionsSearchUrlStatus> UpdateSentConnectionsStatusAsync(SentConnectionsSearchUrlStatus updatedSearchUrlStatus, CancellationToken ct = default)
        {
            string sentConnectionsSearchurlStatusId = updatedSearchUrlStatus.SentConnectionsSearchUrlStatusId;
            _logger.LogInformation("Updating SentConnectionsSearchUrlStatus with id {sentConnectionsSearchurlStatusId}", sentConnectionsSearchurlStatusId);
            try
            {
                _dbContext.SentConnectionsStatuses.Update(updatedSearchUrlStatus);
                await _dbContext.SaveChangesAsync(ct);

                _logger.LogDebug("Successfully updated SentConnectionsSearchUrlStatus with id {sentConnectionsSearchurlStatusId}", sentConnectionsSearchurlStatusId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update SentConnectionsSearchUrlStatus with id {sentConnectionsSearchurlStatusId}. Returning an explicit null", sentConnectionsSearchurlStatusId);
                return null;
            }
            return updatedSearchUrlStatus;
        }

        public async Task<IList<SentConnectionsSearchUrlStatus>> GetAllSentConnectionsStatusesAsync(string campaignId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving all SentConnectionsSearchUrlStatus for campaign id {campaignId}", campaignId);
            IList<SentConnectionsSearchUrlStatus> sentConnectionSearchUrlStatuses = default;
            try
            {
                sentConnectionSearchUrlStatuses = await _dbContext.SentConnectionsStatuses
                    .Where(status => status.CampaignId == campaignId)
                    .ToListAsync(ct);

                _logger.LogDebug("Successfully retrieved all SentConnectionsSearchUrlStatus for campaign id {campaignId}", campaignId);              
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all SentConnectionsSearchUrlStatus for campaign id {campaignId}. Returning an explicit null", campaignId);
                return null;
            }
            return sentConnectionSearchUrlStatuses;
        }

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
