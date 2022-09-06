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
    public class CampaignRepository : ICampaignRepository
    {
        public CampaignRepository(DatabaseContext dbContext, ILogger<CampaignRepository> logger)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private readonly DatabaseContext _dbContext;
        private readonly ILogger<CampaignRepository> _logger;

        private async Task<bool> CampaignExists(string id, CancellationToken ct = default)
        {
            return await _dbContext.Campaigns.AnyAsync(c => c.CampaignId == id, ct);
        }


        public async Task<IList<Campaign>> GetAllActiveAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving all active campaigns.");
            IList<Campaign> campaigns = default;
            try
            {
                campaigns = await _dbContext.Campaigns.Where(c => c.Active == true).Include(c => c.CampaignProspectList).ToListAsync();
                _logger.LogDebug("Successfully retrieved all active campaigns");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all active campaigns. Returning explicit null");
                return null;
            }
            return campaigns;
        }

        public async Task<Campaign> CreateAsync(Campaign newCampaign, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating Campaign.");
            try
            {
                _dbContext.Campaigns.Add(newCampaign);
                await _dbContext.SaveChangesAsync(ct);

                string campaignId = newCampaign.CampaignId;
                _logger.LogDebug("Successfully created new Campaign. New Campaign has id {campaignId}", campaignId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Campaign. Returning an explicit null");
                return null;
            }
            return newCampaign;
        }

        public async Task<CampaignWarmUp> CreateCampaignWarmUpAsync(CampaignWarmUp warmUp, CancellationToken ct = default)
        {
            string campaignId = warmUp.CampaignId;
            _logger.LogInformation("Creating CampaignWarmUp for campaign id {campaignId}", campaignId);
            try
            {
                _dbContext.CampaignWarmUps.Add(warmUp);
                await _dbContext.SaveChangesAsync(ct);

                string warmUpId = warmUp.CampaignWarmUpId;
                _logger.LogDebug("Successfully created CampaignWarmUp. New CampaignWarmUp has the id of {warmUpId}", warmUpId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create CampaignWarmUp. Returning an explicit null");
                return null;
            }

            return warmUp;
        }

        public async Task<Campaign> GetByIdAsync(string campaignId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving campaign by id {campaignId}", campaignId);
            Campaign campaign = default;
            try
            {
                campaign = await _dbContext.Campaigns.Where(c => c.CampaignId == campaignId).Include(c => c.CampaignProspectList).SingleAsync(ct);
                _logger.LogDebug("Successfully retrieved campaign by id {campaignId}", campaignId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve campaign by id {campaignId}. Returning an explicit null", campaignId);
                return null;
            }
            return campaign;
        }

        public async Task<CampaignWarmUp> GetCampaignWarmUpByIdAsync(string campaignId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving CampaignWarmUp by campaign id {campaignId}", campaignId);
            CampaignWarmUp campaignWarmUp = default;
            try
            {
                campaignWarmUp = await _dbContext.CampaignWarmUps.Where(w => w.CampaignId == campaignId).SingleAsync(ct);
                _logger.LogDebug("Succesfully retrieved CampaignWarmUp by campaign id {campaignId}", campaignId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve CampaignWarmUp by campaign id {campaignId}. Returning an explicit null", campaignId);
                return null;
            }
            return campaignWarmUp;
        }

        public async Task<Campaign> UpdateAsync(Campaign updatedCampaign, CancellationToken ct = default)
        {
            string campaignId = updatedCampaign.CampaignId;
            _logger.LogInformation("Updating Campaign with id {campaignId}", campaignId);
            try
            {
                _dbContext.Campaigns.Update(updatedCampaign);
                await _dbContext.SaveChangesAsync(ct);

                _logger.LogDebug("Successfully updated Campaign with id {campaignId}", campaignId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Campaign with id {campaignId}. Returning an explicit null", campaignId);
                return null;
            }
            return updatedCampaign;
        }

        public async Task<IList<Campaign>> GetAllActiveByUserIdAsync(string applicationUserId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving all active campaigns by user id {applicationUserId}", applicationUserId);
            IList<Campaign> campaigns = default;
            try
            {
                campaigns = await _dbContext.Campaigns
                    .Where(c => c.Active == true && c.ApplicationUserId == applicationUserId)
                    .Include(c => c.CampaignProspectList)
                    .ToListAsync(ct);

                _logger.LogDebug("Successfully retrieved all active campaigns by user id {applicationUserId}", applicationUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all active campaigns by user id {applicationUserId}. Returning an explicit null", applicationUserId);
                return null;
            }
            return campaigns;
        }

        public async Task<IList<Campaign>> GetAllByUserIdAsync(string userId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving all campaigns by user id {userId}", userId);
            IList<Campaign> campaigns = default;
            try
            {
                campaigns = await _dbContext.Campaigns
                    .Where(c => c.ApplicationUserId == userId)
                    .Include(c => c.CampaignProspectList)
                        .ThenInclude(c => c.CampaignProspects)
                    .ToListAsync(ct);

                _logger.LogDebug("Successfully retrieved all campaigns by user id {userId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all campaigns by user id {userId}. Returning an explicit null", userId);
                return null;
            }
            return campaigns;
        }

        public async Task<IList<Campaign>> GetAllActiveByHalIdAsync(string halId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving all active campaigns by hal id {halId}", halId);
            IList<Campaign> campaigns = default;
            try
            {
                campaigns = await _dbContext.Campaigns
                    .Where(c => c.Active == true && c.HalId == halId)
                    .Include(c => c.CampaignProspectList)
                    .Include(c => c.SearchUrlsProgress)
                    .ToListAsync(ct);

                _logger.LogDebug("Successfully retrieved all active campaigns by hal id {halId}", halId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all active campaigns by hal id {halId}. Returning an explicit null", halId);
                return null;
            }
            return campaigns;
        }

        public async Task<bool> DeleteAsync(string campaignId, CancellationToken ct = default)
        {
            if (!await CampaignExists(campaignId, ct))
            {
                return false;
            }

            try
            {
                Campaign toRemove = await _dbContext
                    .Campaigns
                    .Include(x => x.FollowUpMessagePhase)
                    .Include(x => x.ProspectListPhase)
                    .Include(x => x.SearchUrlsProgress)
                    .Include(x => x.SendConnectionStages)
                    .Include(x => x.SentConnectionsStatuses)
                    .Include(x => x.CampaignProspectList)
                        .ThenInclude(y => y.CampaignProspects)
                    .FirstOrDefaultAsync(x => x.CampaignId == campaignId);
                _dbContext.Campaigns.Remove(toRemove);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Failed to delete campaign with id: {campaignId}", campaignId);
                return false;
            }

            return true;
        }

        public async Task<bool> AnyActiveByHalIdAsync(string halId, CancellationToken ct = default)
        {
            bool anyActive = false;
            try
            {
                anyActive = await _dbContext.Campaigns.Where(c => c.HalId == halId).AnyAsync(c => c.Active == true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if there were any active campaigns for for halId {halId}", halId);
            }
            return anyActive;
        }
    }
}
