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
                campaign = await _dbContext.Campaigns.Where(c => c.Id == campaignId).SingleAsync(ct);
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
                propsectListPhase = await _dbContext.ProspectListPhases.Where(p => p.Id == prospectListPhaseId).Include(p => p.Campaign).ThenInclude(c => c.CampaignProspectList).SingleAsync(ct);

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve prospect list phase by its id");
            }

            return propsectListPhase;
        }

        public async Task<SentConnectionsStatus> GetSentConnectionStatusAsync(string campaignId, CancellationToken ct = default)
        {
            SentConnectionsStatus sentConnectionsStatus = default;
            try
            {
                sentConnectionsStatus = await _dbContext.SentConnectionsStatus.Where(s => s.CampaignId == campaignId).SingleAsync(ct);
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
                campaignProspects = await _dbContext.CampaignProspects.Where(c => c.CampaignId == campaignId).ToListAsync(ct);
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
                primaryProspectList = await _dbContext.PrimaryProspectLists.FindAsync(primaryProspectListId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve primary prospect list by id");
            }

            return primaryProspectList;
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

        public Task<IList<CampaignProspect>> CreateCampaignProspectsAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default)
        {
            throw new NotImplementedException();
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
    }
}
