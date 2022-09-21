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
    public class RecentlyAddedProspectRepository : IRecentlyAddedProspectRepository
    {
        public RecentlyAddedProspectRepository(DatabaseContext dbContext, ILogger<RecentlyAddedProspectRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        private readonly ILogger<RecentlyAddedProspectRepository> _logger;
        private readonly DatabaseContext _dbContext;


        public async Task<IList<RecentlyAddedProspect>> GetAllBySocialAccountIdAsync(string socialAccountId, CancellationToken ct = default)
        {
            IList<RecentlyAddedProspect> recentlyAddedProspects = default;
            try
            {
                recentlyAddedProspects = await _dbContext.RecentlyAddedProspects.Where(x => x.SocialAccountId == socialAccountId).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all {0} by SocialAccountId {1}", nameof(RecentlyAddedProspect), socialAccountId);
            }

            return recentlyAddedProspects;
        }

        public async Task<IList<RecentlyAddedProspect>> CreateAllAsync(IList<RecentlyAddedProspect> newEntites, CancellationToken ct = default)
        {
            string socialAccountId = newEntites.FirstOrDefault()?.SocialAccountId;
            _logger.LogInformation("Creating all {0} items for SocialAccountId {1}", nameof(RecentlyAddedProspect), socialAccountId);
            try
            {
                _dbContext.RecentlyAddedProspects.AddRange(newEntites);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create {0} items.", nameof(RecentlyAddedProspect));
            }

            return newEntites;
        }

        public async Task<bool> DeleteAllBySocialAccountIdAsync(string socialAccountId, CancellationToken ct = default)
        {
            _logger.LogInformation("Deleting all {0} by SocialAccountId {1}", nameof(RecentlyAddedProspect), socialAccountId);
            bool succeeded = false;
            try
            {
                IList<RecentlyAddedProspect> toRemove = await _dbContext.RecentlyAddedProspects.Where(x => x.SocialAccountId == socialAccountId).ToListAsync();
                _dbContext.RemoveRange(toRemove);
                await _dbContext.SaveChangesAsync(ct);
                succeeded = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete all of the {0} items by SocialAccountId {1}", nameof(RecentlyAddedProspect), socialAccountId);
            }

            return succeeded;
        }
    }
}
