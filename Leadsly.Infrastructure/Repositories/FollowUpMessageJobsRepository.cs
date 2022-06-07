using Leadsly.Application.Model.Entities.Campaigns;
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
    internal class FollowUpMessageJobsRepository : IFollowUpMessageJobsRepository
    {
        public FollowUpMessageJobsRepository(ILogger<FollowUpMessageJobsRepository> logger, DatabaseContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private readonly ILogger<FollowUpMessageJobsRepository> _logger;
        private readonly DatabaseContext _dbContext;

        public async Task<FollowUpMessageJob> AddFollowUpJobAsync(FollowUpMessageJob followUpJob, CancellationToken ct = default)
        {
            _logger.LogInformation($"Adding HangFire JobId {followUpJob.HangfireJobId} to the database");
            try
            {
                _dbContext.FollowUpMessageJobs.Add(followUpJob);
                await _dbContext.SaveChangesAsync(ct);
                _logger.LogDebug("Successfully saved FollowUpMessageJob to the database");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save FollowUpMessageJob to the database");
            }
            return followUpJob;
        }

        public async Task<IList<FollowUpMessageJob>> GetFollowUpJobIdsAsync(string campaignProspectId, CancellationToken ct = default)
        {
            _logger.LogInformation($"Retrieving FollowUpMessageJobs by campaignProspectId {campaignProspectId}");
            IList<FollowUpMessageJob> followUpMessageJobs = default;
            try
            {
                followUpMessageJobs = await _dbContext.FollowUpMessageJobs.Where(f => f.CampaignProspectId == campaignProspectId).ToListAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve FollowUpMessageJobs by campaignPropsectId {campaignProspectId}");
            }
            return followUpMessageJobs;
        }
    }
}
