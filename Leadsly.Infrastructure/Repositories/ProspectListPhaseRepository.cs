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
    public class ProspectListPhaseRepository : IProspectListPhaseRepository
    {
        public ProspectListPhaseRepository(DatabaseContext dbContext, ILogger<ProspectListPhaseRepository> logger)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private readonly DatabaseContext _dbContext;
        private readonly ILogger<ProspectListPhaseRepository> _logger;

        public async Task<IList<ProspectListPhase>> GetAllActiveAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving all prospect list phases that are associated with an active campaign");
            IList<ProspectListPhase> prospectListPhases = new List<ProspectListPhase>();
            try
            {
                prospectListPhases = await _dbContext.ProspectListPhases.Include(p => p.Campaign).Where(p => p.Campaign.Active == true).ToListAsync(ct);
                _logger.LogDebug("Successfully found prospect list phases for active campaigns");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve prospect list phases that are associated with active campaigns. Returning an explict null");
                return null;
            }

            return prospectListPhases;
        }

        public async Task<IList<ProspectListPhase>> GetAllActiveByHalIdAsync(string halId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving all prospect list phases that are associated with an active campaign for hal id {halId}", halId);
            IList<ProspectListPhase> prospectListPhases = new List<ProspectListPhase>();
            try
            {
                prospectListPhases = await _dbContext.ProspectListPhases.Include(p => p.Campaign).Where(p => p.Campaign.Active == true && p.Campaign.HalId == halId).ToListAsync(ct);
                _logger.LogDebug("Successfully found prospect list phases for active campaigns for hal id {halId}", halId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve prospect list phases that are associated with active campaigns for hal id {halId}. Returning an explict null", halId);
                return null;
            }

            return prospectListPhases;
        }

        public async Task<ProspectListPhase> GetByCampaignIdAsync(string campaignId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving prospect list phase by campaign id {campaignId}", campaignId);
            ProspectListPhase prospectListPhase = null;
            try
            {
                prospectListPhase = await _dbContext.ProspectListPhases.Where(p => p.CampaignId == campaignId).SingleAsync(ct);
                _logger.LogDebug("Successfully found prospect list phase for campaign id {campaignId}", campaignId);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to retrieve prospect list phase by campaign id. Returning an explict null");
                return null;
            }
            return prospectListPhase;
        }

        public async Task<ProspectListPhase> GetByIdAsync(string prospectListPhaseId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving prospect list phase by prospect list phase id {prospectListPhaseId}", prospectListPhaseId);
            ProspectListPhase propsectListPhase = default;
            try
            {
                propsectListPhase = await _dbContext.ProspectListPhases
                    .Where(p => p.ProspectListPhaseId == prospectListPhaseId)
                    .Include(p => p.Campaign)
                        .ThenInclude(c => c.CampaignProspectList)
                    .SingleAsync(ct);

                _logger.LogDebug("Successfully found prospect list phase by id {prospectListPhaseId}", prospectListPhaseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve prospect list phase by id {prospectListPhaseId}. Returning an explicty null", prospectListPhaseId);
                return null;
            }
            return propsectListPhase;
        }

        public async Task<ProspectListPhase> UpdateAsync(ProspectListPhase prospectListPhase, CancellationToken ct = default)
        {
            string prospectListPhaseId = prospectListPhase.ProspectListPhaseId;
            _logger.LogInformation("Updating prospect list phase with id {prospectListPhaseId}", prospectListPhaseId);
            try
            {
                _dbContext.ProspectListPhases.Update(prospectListPhase);
                await _dbContext.SaveChangesAsync(ct);
                _logger.LogDebug("Successfully updated prospect list with id {prospectListPhaseId}", prospectListPhaseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update prospect list phase with id {prospectListPhaseId}. returning explicit null", prospectListPhaseId);
                return null;
            }
            return prospectListPhase;
        }

        public async Task<bool> AnyIncompleteByHalIdAsync(string halId, CancellationToken ct = default)
        {
            _logger.LogInformation("Checking for any incomplete prospect list phases for hal id {halId}", halId);
            bool anyIncomplete = false;
            try
            {
                anyIncomplete = await _dbContext.ProspectListPhases.Include(p => p.Campaign).Where(p => p.Campaign.Active == true && p.Completed == false).AnyAsync(ct);                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check for any incomplete prospect list phases with hal id {halId}. returning explicit false", halId);
                return false;
            }
            return anyIncomplete;
        }
    }
}
