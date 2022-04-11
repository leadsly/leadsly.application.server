using Leadsly.Application.Model.Entities.Campaigns;
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
    public class CampaignRepository : ICampaignRepository
    {
        public CampaignRepository(DatabaseContext dbContext, ILogger<CampaignRepository> logger)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private readonly DatabaseContext _dbContext;
        private readonly ILogger<CampaignRepository> _logger;

        public async Task<List<Campaign>> GetAllActiveAsync(CancellationToken ct = default)
        {
            return await _dbContext.Campaigns.Where(c => c.Active == true).ToListAsync();
        }

        public async Task<Campaign> CreateAsync(Campaign newCampaign, CancellationToken ct = default)
        {
            try
            {
                _dbContext.Campaigns.Add(newCampaign);
                await _dbContext.SaveChangesAsync(ct);                
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error occured creating new campaign");
            }
            return newCampaign;
        }

        public Task<List<ProspectListPhase>> GetAllActivePropspectListPhasesAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<Campaign>> GetAllActiveByHalIdAsync(string halId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<FollowUpMessage> GetFollowUpMessageByCampaignIdAsync(int order, string campaignId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<CampaignWarmUp> CreateCampaignWarmUpAsync(CampaignWarmUp warmUp, CancellationToken ct = default)
        {
            try
            {
                _dbContext.CampaignWarmUps.Add(warmUp);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add campaign warm up object");
            }
            return warmUp;
        }

        public Task<ProspectListPhase> GetProspectListPhaseByCampaignIdAsync(string campaignId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<PrimaryProspectList> GetProspectListByPhaseIdAsync(string prospectListPhaseId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<Campaign> GetCampaignByIdAsync(string campaignId, CancellationToken ct = default)
        {
            Campaign campaign = default;
            try
            {
                campaign = await _dbContext.Campaigns.Where(c => c.CampaignId == campaignId).SingleAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve campaign by id");
            }

            return campaign;
        }

        public async Task<ProspectListPhase> GetProspectListPhaseByPhaseIdAsync(string prospectListPhaseId, CancellationToken ct = default)
        {
            ProspectListPhase propsectListPhase = default;

            try
            {
                propsectListPhase = await _dbContext.ProspectListPhases.Where(p => p.ProspectListPhaseId == prospectListPhaseId).Include(p => p.Campaign).ThenInclude(c => c.CampaignProspectList).SingleAsync(ct);

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve prospect list phase by its id");
            }

            return propsectListPhase;
        }

        public async Task<SentConnectionsSearchUrlStatus> UpdateSentConnectionsStatusAsync(SentConnectionsSearchUrlStatus updatedSearchUrlStatus, CancellationToken ct = default)
        {
            try
            {
                _dbContext.SentConnectionsStatuses.Update(updatedSearchUrlStatus);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update sent connections status");
                updatedSearchUrlStatus = null;
            }
            return updatedSearchUrlStatus;
        }
        
        public async Task<IList<SentConnectionsSearchUrlStatus>> GetSentConnectionStatusesAsync(string campaignId, CancellationToken ct = default)
        {
            IList<SentConnectionsSearchUrlStatus> sentConnectionSearchUrlStatuses = default;
            try
            {
                sentConnectionSearchUrlStatuses = await _dbContext.SentConnectionsStatuses.Where(status => status.CampaignId == campaignId).ToListAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve 'SentConnectionsStatus' list for the given campaign");                
            }
            return sentConnectionSearchUrlStatuses;
        }

        public async Task<SentConnectionsSearchUrlStatus> GetSentConnectionStatusAsync(string campaignId, CancellationToken ct = default)
        {
            SentConnectionsSearchUrlStatus sentConnectionsStatus = default;
            try
            {
                sentConnectionsStatus = await _dbContext.SentConnectionsStatuses.Where(s => s.CampaignId == campaignId).SingleAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve 'SentConnectionsStatus' for the given campaign");
            }
            return sentConnectionsStatus;
        }

        public async Task<IList<CampaignProspect>> GetCampaignProspectsByIdAsync(string campaignId, CancellationToken ct = default)
        {
            IList<CampaignProspect> campaignProspects = default;
            try
            {
                campaignProspects = await _dbContext.CampaignProspects.Where(c => c.CampaignId == campaignId).Include(p => p.PrimaryProspect).ToListAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get campaign prospects by campaign id");
            }
            return campaignProspects;
        }

        public async Task<string> GetChromeProfileNameByCampaignPhaseTypeAsync(PhaseType campaignType, CancellationToken ct = default)
        {
            ChromeProfileName profile = default;
            try
            {
                profile = await _dbContext.ChromeProfileNames.Where(p => p.CampaignPhaseType == campaignType).SingleAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve 'ChromeProfileName' by the given campaign phase type");
            }

            return profile?.Profile;
        }

        public async Task<ChromeProfileName> CreateChromeProfileNameAsync(ChromeProfileName chromeProfileName, CancellationToken ct = default)
        {
            try
            {
                _dbContext.ChromeProfileNames.Add(chromeProfileName);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add 'ChromeProfileName' to the database");
            }
            return chromeProfileName;
        }

        public async Task<PrimaryProspectList> GetPrimaryProspectListByIdAsync(string primaryProspectListId, CancellationToken ct = default)
        {
            PrimaryProspectList primaryProspectList = default;
            try
            {
                primaryProspectList = await _dbContext.PrimaryProspectLists.Where(p => p.PrimaryProspectListId == primaryProspectListId).Include(p => p.PrimaryProspects).SingleAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve primary prospect list by id");
            }

            return primaryProspectList;
        }

        public async Task<PrimaryProspect> GetPrimaryProspectByIdAsync(string primaryProspectId, CancellationToken ct = default)
        {
            PrimaryProspect primaryProspect = default;
            try
            {
                primaryProspect = await _dbContext.PrimaryProspects.Where(p => p.PrimaryProspectId == primaryProspectId).SingleAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve primary prospect by id");
            }

            return primaryProspect;
        }
        

        public async Task<IList<PrimaryProspect>> CreatePrimaryProspectsAsync(IList<PrimaryProspect> primaryProspectList, CancellationToken ct = default)
        {
            try
            {
                await _dbContext.PrimaryProspects.AddRangeAsync(primaryProspectList);
                await _dbContext.SaveChangesAsync(ct);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add prospects");
            }

            return primaryProspectList;
        }

        public async Task<IList<CampaignProspect>> CreatePrimaryProspectsAsync(IList<CampaignProspect> campaignProspectList, CancellationToken ct = default)
        {
            try
            {
                await _dbContext.CampaignProspects.AddRangeAsync(campaignProspectList);
                await _dbContext.SaveChangesAsync(ct);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add campaign prospects");
            }

            return campaignProspectList;
        }

        public async Task<ProspectListPhase> UpdateCampaignProspectListPhaseAsync(ProspectListPhase prospectListPhase, CancellationToken ct = default)
        {
            try
            {
                _dbContext.ProspectListPhases.Update(prospectListPhase);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update prospect list phase");
            }
            return prospectListPhase;
        }

        public async Task<ProspectListPhase> GetProspectListPhaseByIdAsync(string campaignId, CancellationToken ct = default)
        {
            ProspectListPhase prospectListPhase = default;
            try
            {
                prospectListPhase = await _dbContext.ProspectListPhases.Where(p => p.CampaignId == campaignId).SingleAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get prospect list phase by its id");
            }
            return prospectListPhase;
        }

        public async Task<IList<CampaignProspect>> CreateCampaignProspectsAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default)
        {
            try
            {
                await _dbContext.CampaignProspects.AddRangeAsync(campaignProspects);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create campaign prospects");
                campaignProspects = null;
            }

            return campaignProspects;
        }

        public async Task<CampaignWarmUp> GetCampaignWarmUpByIdAsync(string campaignId, CancellationToken ct = default)
        {
            CampaignWarmUp campaignWarmUp = default;
            try
            {
                campaignWarmUp = await _dbContext.CampaignWarmUps.Where(w => w.CampaignId == campaignId).SingleAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve campaign warm up by campaign id");
            }
            return campaignWarmUp;
        }

        public async Task<IList<SendConnectionsStage>> GetSendConnectionStagesByIdAsync(string campaignId, CancellationToken ct = default)
        {
            IList<SendConnectionsStage> sendConnectionsStages = default;
            try
            {
                sendConnectionsStages = await _dbContext.SendConnectionsStages.Where(s => s.CampaignId == campaignId).ToListAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve campaign 'SendConnectionStages' by campaign id");
            }

            return sendConnectionsStages;
        }

        public async Task<IList<CampaignProspect>> UpdateCampaignProspectsAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default)
        {
            try
            {
                _dbContext.CampaignProspects.UpdateRange(campaignProspects);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update campaign prospects");
                campaignProspects = null;
            }

            return campaignProspects;
        }

        public async Task<MonitorForNewConnectionsPhase> CreateMonitorForNewConnectionsPhaseAsync(MonitorForNewConnectionsPhase phase, CancellationToken ct = default)
        {
            try
            {
                _dbContext.MonitorForNewConnectionsPhases.Add(phase);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create monitor for new connections phase");
                phase = null;
            }
            return phase;
        }

        public async Task<IList<MonitorForNewConnectionsPhase>> GetAllMonitorForNewConnectionsPhasesByUserIdAsync(string userId, CancellationToken ct = default)
        {
            IList<MonitorForNewConnectionsPhase> monitorForNewConnectionsPhases = null;
            try
            {
                monitorForNewConnectionsPhases = await _dbContext.MonitorForNewConnectionsPhases.Include("SocialAccount").Where(p => p.SocialAccount.UserId == userId).ToListAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create monitor for new connections phase");
                monitorForNewConnectionsPhases = null;
            }
            return monitorForNewConnectionsPhases;
        }

        public async Task<MonitorForNewConnectionsPhase> GetMonitorForNewConnectionsPhaseBySocialAccountIdAsync(string socialAccountId, CancellationToken ct = default)
        {
            MonitorForNewConnectionsPhase monitorForNewConnectionsPhase = null;
            try
            {
                monitorForNewConnectionsPhase = await _dbContext.MonitorForNewConnectionsPhases.Where(p => p.SocialAccountId == socialAccountId).SingleAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve monitofr for new connections phase for this social account id {socialAccountId}", socialAccountId);
                monitorForNewConnectionsPhase = null;
            }
            return monitorForNewConnectionsPhase;
        }

        public async Task<ScanProspectsForRepliesPhase> CreateScanProspectsForRepliesPhaseAsync(ScanProspectsForRepliesPhase phase, CancellationToken ct = default)
        {
            try
            {
                _dbContext.ScanProspectsForRepliesPhase.Add(phase);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create scan prospects for replies");
                phase = null;
            }
            return phase;
        }

        public async Task<ConnectionWithdrawPhase> CreateConnectionWithdrawPhaseAsync(ConnectionWithdrawPhase phase, CancellationToken ct = default)
        {
            try
            {
                _dbContext.ConnectionWithdrawPhases.Add(phase);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create connection withdraw phase");
                phase = null;
            }
            return phase;
        }

        public async Task<Campaign> UpdateAsync(Campaign updatedCampaign, CancellationToken ct = default)
        {
            try
            {
                _dbContext.Campaigns.Update(updatedCampaign);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update campaign");
                updatedCampaign = null;
            }
            return updatedCampaign;
        }

        public async Task<IList<Campaign>> GetAllActiveByUserIdAsync(string applicationUserId, CancellationToken ct = default)
        {
            return await _dbContext.Campaigns.Where(c => c.Active == true && c.ApplicationUserId == applicationUserId).Include(c => c.CampaignProspectList).ToListAsync(ct);
        }

        public async Task<CampaignProspectList> GetCampaignProspectListByCampaignProspectListIdAsync(string campaignProspectListId, CancellationToken ct = default)
        {
            return await _dbContext.CampaignProspectLists
                .Where(cpl => cpl.CampaignProspectListId == campaignProspectListId)
                .Include(cpl => cpl.CampaignProspects)                    
                .SingleAsync(ct);
        }        
    }
}
