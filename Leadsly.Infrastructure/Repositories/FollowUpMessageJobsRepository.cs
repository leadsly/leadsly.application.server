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
    public class FollowUpMessageJobsRepository : IFollowUpMessageJobsRepository
    {
        public FollowUpMessageJobsRepository(ILogger<FollowUpMessageJobsRepository> logger, DatabaseContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private readonly ILogger<FollowUpMessageJobsRepository> _logger;
        private readonly DatabaseContext _dbContext;

        private async Task<bool> FollowUpMessageJobExists(string id, CancellationToken ct = default) => await _dbContext.FollowUpMessageJobs.AnyAsync(c => c.FollowUpMessageJobId == id, ct);

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

        public async Task<IList<FollowUpMessageJob>> GetAllByCampaignProspectIdAsync(string campaignProspectId, CancellationToken ct = default)
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

        public async Task<FollowUpMessageJob> GetByFollowUpmessageIdAsync(string followUpMessageId, CancellationToken ct = default)
        {
            FollowUpMessageJob followUpMessageJob = default;
            try
            {
                followUpMessageJob = await _dbContext.FollowUpMessageJobs.Where(x => x.FollowUpMessageId == followUpMessageId).SingleAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve FollowUpMessageJob by FollowUpMessageId {followUpMessageId}");
            }

            return followUpMessageJob;
        }

        public async Task<bool> DeleteFollowUpMessageJobAsync(string followUpMessageJobId, CancellationToken ct = default)
        {
            if (!await FollowUpMessageJobExists(followUpMessageJobId, ct))
            {
                return false;
            }

            FollowUpMessageJob toRemove = _dbContext.FollowUpMessageJobs.Find(followUpMessageJobId);
            _dbContext.FollowUpMessageJobs.Remove(toRemove);
            await _dbContext.SaveChangesAsync(ct);

            return true;
        }
    }
}
