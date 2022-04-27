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
    public class FollowUpMessageRepository : IFollowUpMessageRepository
    {
        public FollowUpMessageRepository(DatabaseContext dbContext, ILogger<FollowUpMessageRepository> logger)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private readonly DatabaseContext _dbContext;
        private readonly ILogger<FollowUpMessageRepository> _logger;

        public async Task<IList<FollowUpMessage>> GetAllByCampaignIdAsync(string campaignId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving first FollowUpMessage by campaign id {campaignId}", campaignId);
            IList<FollowUpMessage> messages = default;
            try
            {
                messages = await _dbContext.FollowUpMessages.Where(f => f.CampaignId == campaignId).Include(f => f.Delay).ToListAsync(ct);
                _logger.LogDebug("Successfully retrieved first FollowUpMessage for campaign id: {campaignId}", campaignId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve FollowUpMessage by campaign id {campaignId}. Returning an explicit null", campaignId);
                return null;
            }
            return messages;
        }

        public async Task<CampaignProspectFollowUpMessage> CreateAsync(CampaignProspectFollowUpMessage message, CancellationToken ct = default)
        {
            string campaignProspectId = message.CampaignProspectId;
            _logger.LogInformation("Create CampaignProspectFollowUpMessage for campaign prospect id {campaignProspectId}", campaignProspectId);            
            try
            {
                _dbContext.CampaignProspectFollowUpMessages.Add(message);
                await _dbContext.SaveChangesAsync(ct);
                _logger.LogDebug("Successfully created CampaignProspectFollowUpMessage for campaign prospect id {campaignProspectId}", campaignProspectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create CampaignProspectFollowUpMessage for campaign prospect id {campaignProspectId}. Returning an explicit null", campaignProspectId);
                return null;
            }
            return message;
        }

        public async Task<CampaignProspectFollowUpMessage> GetCampaignProspectFollowUpMessageByIdAsync(string campaignProspectFollowUpMessageId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving CampaignProspectFollowUpMessage for campaign prospect follow up message id {campaignProspectFollowUpMessageId}", campaignProspectFollowUpMessageId);
            CampaignProspectFollowUpMessage message = default;
            try
            {
                message = await _dbContext.CampaignProspectFollowUpMessages.Where(m => m.CampaignProspectFollowUpMessageId == campaignProspectFollowUpMessageId).Include(m => m.CampaignProspect).SingleAsync(ct);                
                _logger.LogDebug("Successfully retrieved CampaignProspectFollowUpMessage for campaign prospect follow up message id {campaignProspectFollowUpMessageId}", campaignProspectFollowUpMessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieved CampaignProspectFollowUpMessage for campaign prospect follow up message id {campaignProspectFollowUpMessageId}. Returning an explicit null", campaignProspectFollowUpMessageId);
                return null;
            }
            return message;
        }
    }
}
