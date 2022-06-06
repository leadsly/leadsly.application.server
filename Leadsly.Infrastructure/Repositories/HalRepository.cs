using Leadsly.Application.Model.Entities;
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
    public class HalRepository : IHalRepository
    {
        public HalRepository(DatabaseContext dbContext, ILogger<HalRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;            
        }

        private readonly ILogger<HalRepository> _logger;
        private readonly DatabaseContext _dbContext;

        public async Task<HalUnit> CreateAsync(HalUnit halDetails, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating HalUnit.");
            try
            {
                _dbContext.HalUnits.Add(halDetails);
                await _dbContext.SaveChangesAsync(ct);

                string halId = halDetails.HalId;
                _logger.LogDebug("Successfully created HalUnit. New HalUnit' hal id is : {halId}", halId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured adding hal details to the database");
            }

            return halDetails;
        }   
        
        public async Task<HalUnit> GetBySocialAccountUsernameAsync(string connectedAccountUsername, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving HalUnit by socialaccount username {connectedAccountUserName}", connectedAccountUsername);
            HalUnit halDetails = null;
            try
            {                
                // should always be just one
                halDetails = await _dbContext.HalUnits
                    .Include(h => h.SocialAccount)
                    .Where(h => h.SocialAccount.Username == connectedAccountUsername)
                    .SingleAsync(ct);

                _logger.LogDebug("Successfully retrieved HalUnit for social account username {connectedAccountUsername}", connectedAccountUsername);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured getting HalUnit for {connectedAccountUsername}. Returning an explicit null", connectedAccountUsername);
                return null;
            }
            return halDetails;
        }

        public async Task<ChromeProfile> CreateChromeProfileAsync(ChromeProfile chromeProfile, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating ChromeProfileName.");
            try
            {
                _dbContext.ChromeProfileNames.Add(chromeProfile);
                await _dbContext.SaveChangesAsync(ct);

                string chromeProfileName = chromeProfile.Name;
                _logger.LogDebug("Successfully created ChromeProfileName. New ChromeProfileName is {chromeProfileName}", chromeProfileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add 'ChromeProfileName' to the database");
            }
            return chromeProfile;
        }

        public async Task<ChromeProfile> GetChromeProfileAsync(PhaseType campaignType, CancellationToken ct = default)
        {
            string campaignTypeName = Enum.GetName(campaignType);

            _logger.LogInformation("Retrieving ChromeProfileName by {campaignTypeName}", campaignTypeName);
            ChromeProfile profile = default;
            try
            {
                profile = await _dbContext.ChromeProfileNames.Where(p => p.CampaignPhaseType == campaignType).SingleOrDefaultAsync(ct);
                string log = profile == null ? "ChromeProfileName does not exist for {campaignTypeName} campaign type name " : "ChromeProfileName was found for {campaignTypeName} campaign type name";
                _logger.LogDebug(log, campaignTypeName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve ChromeProfileName by campaign type {campaignTypeName}", campaignTypeName);
            }

            return profile;
        }

        public async Task<HalUnit> GetByHalIdAsync(string halId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving HalUnit by hal id {halId}", halId);
            HalUnit halUnit = default;
            try
            {
                halUnit = await _dbContext.HalUnits.Where(h => h.HalId == halId)
                                                    .Include(h => h.SocialAccount)
                                                        .ThenInclude(s => s.ConnectionWithdrawPhase)
                                                     .Include(h => h.SocialAccount)
                                                        .ThenInclude(s => s.MonitorForNewProspectsPhase)
                                                    .Include(h => h.SocialAccount)
                                                        .ThenInclude(s => s.ScanProspectsForRepliesPhase)
                                                    .SingleAsync(ct);                

                _logger.LogDebug("Successfully found HalUnit by hal id {halId}", halId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve HalUnit by hal id {halId}. Returning an explicit null", halId);
                return null;
            }

            return halUnit;
        }

        public async Task<IList<string>> GetAllHalIdsAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving all hal ids");
            IList<string> halIds = new List<string>();
            try
            {
                halIds = await _dbContext.HalUnits.Select(h => h.HalId).ToListAsync(ct);

                _logger.LogDebug("Successfully found hal ids");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve hal ids. Returning an explicit null");
                return null;
            }

            return halIds;
        }
    }
}
