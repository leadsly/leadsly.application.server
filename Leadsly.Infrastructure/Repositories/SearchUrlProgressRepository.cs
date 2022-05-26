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
    public class SearchUrlProgressRepository : ISearchUrlProgressRepository
    {
        public SearchUrlProgressRepository(DatabaseContext dbContext, ILogger<SearchUrlProgressRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        private readonly ILogger<SearchUrlProgressRepository> _logger;
        private readonly DatabaseContext _dbContext;

        public async Task<SearchUrlProgress> CreateAsync(SearchUrlProgress searchUrlProgress, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating HalUnit.");
            try
            {
                _dbContext.SearchUrlsProgress.Add(searchUrlProgress);
                await _dbContext.SaveChangesAsync(ct);

                _logger.LogDebug("Successfully created SearchUrlProgress");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured adding SearchUrlProgress to the database");
            }

            return searchUrlProgress;
        }

        public async Task<SearchUrlProgress> UpdateAsync(SearchUrlProgress searchUrlProgress, CancellationToken ct = default)
        {
            _logger.LogInformation("Updating SearchUrlProgress");
            try
            {
                _dbContext.SearchUrlsProgress.Update(searchUrlProgress);
                await _dbContext.SaveChangesAsync(ct);

                _logger.LogDebug("Successfully updated SearchUrlProgress");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update SearchUrlProgress");
                return null;
            }
            return searchUrlProgress;
        }

        public async Task<IList<SearchUrlProgress>> GetAllByCampaignIdAsync(string campaignId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving all SearchUrlProgress for campaign id {campaignId}", campaignId);
            IList<SearchUrlProgress> searchUrlsProgress = default;
            try
            {
                searchUrlsProgress = await _dbContext.SearchUrlsProgress
                    .Where(status => status.CampaignId == campaignId)
                    .ToListAsync(ct);

                _logger.LogDebug("Successfully retrieved all searchUrlProgress for campaign id {campaignId}", campaignId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all searchUrlProgress for campaign id {campaignId}. Returning an explicit null", campaignId);
                return null;
            }
            return searchUrlsProgress;
        }

        public async Task<SearchUrlProgress> GetByIdAsync(string searchUrlProgressId, CancellationToken ct = default)
        {
            SearchUrlProgress searchUrlProgress = default;
            try
            {
                searchUrlProgress = await _dbContext.SearchUrlsProgress.FindAsync(searchUrlProgressId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve SearchUrlProgress by id {searchUrlProgressId}", searchUrlProgressId);
                return null;
            }
            return searchUrlProgress;
        }
    }
}
