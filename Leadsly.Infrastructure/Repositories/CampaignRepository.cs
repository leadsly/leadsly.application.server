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

        public Task<CampaignWarmUp> CreateCampaignWarmUpAsync(CampaignWarmUp warmUp, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<ProspectListPhase> GetProspectListPhaseByCampaignIdAsync(string campaignId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<PrimaryProspectList> GetProspectListByProspectListPhaseIdAsync(string prospectListPhaseId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<Campaign> GetCampaignByIdAsync(string campaignId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<ProspectListPhase> GetProspectListPhaseByIdAsync(string prospectListId, CancellationToken ct = default)
        {
            ProspectListPhase propsectListPhase = default;

            try
            {
                propsectListPhase = await _dbContext.ProspectListPhases.Where(p => p.Id == prospectListId).Include(p => p.Campaign).SingleAsync(ct);

            }
            catch(Exception ex)
            {

            }

            return propsectListPhase;
        }

        public Task<SentConnectionsStatus> CreateProspectListStatus(string campaignId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<SentConnectionsStatus> GetSentConnectionStatusAsync(string campaignId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CampaignProspect>> GetCampaignProspectsByIdAsync(string campaignId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
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

            }

            if(profile == null)
            {
                return null;
            }

            return profile.Profile;
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

            }
            return chromeProfileName;
        }
    }
}
