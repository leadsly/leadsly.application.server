using Leadsly.Domain.Models.Entities.Campaigns.Phases;
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
    public class MonitorForNewConnectionsPhaseRepository : IMonitorForNewConnectionsPhaseRepository
    {
        public MonitorForNewConnectionsPhaseRepository(DatabaseContext dbContext, ILogger<MonitorForNewConnectionsPhaseRepository> logger)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private readonly DatabaseContext _dbContext;
        private readonly ILogger<MonitorForNewConnectionsPhaseRepository> _logger;

        private async Task<bool> PhaseExists(string id, CancellationToken ct = default)
        {
            return await _dbContext.MonitorForNewConnectionsPhases.AnyAsync(x => x.MonitorForNewConnectionsPhaseId == id, ct);
        }

        public async Task<MonitorForNewConnectionsPhase> CreateAsync(MonitorForNewConnectionsPhase phase, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating new MonitorForNewConnectionsPhase");
            try
            {
                _dbContext.MonitorForNewConnectionsPhases.Add(phase);
                await _dbContext.SaveChangesAsync(ct);

                _logger.LogDebug("Successfully created MonitorForNewConnectionsPhase");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create MonitorForNewConnectionsPhase. Returning an explict null");
                return null;
            }

            return phase;
        }

        public async Task<IList<MonitorForNewConnectionsPhase>> GetAllByUserIdAsync(string userId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving MonitorForNewConnectionsPhase by user id {userId}", userId);
            IList<MonitorForNewConnectionsPhase> monitorForNewConnectionsPhases = null;
            try
            {
                monitorForNewConnectionsPhases = await _dbContext.MonitorForNewConnectionsPhases
                    .Include(m => m.SocialAccount)
                    .Where(p => p.SocialAccount.UserId == userId)
                    .ToListAsync(ct);

                _logger.LogDebug("Successfully retrieved MonitorForNewConnectionsPhase by user id {userId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve MonitorForNewConnectionsPhase by user id {userId}. Returning an explicit null", userId);
                return null;
            }

            return monitorForNewConnectionsPhases;
        }

        public async Task<MonitorForNewConnectionsPhase> GetBySocialAccountIdAsync(string socialAccountId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving MonitorForNewConnectionsPhase by social account id {socialAccountId}", socialAccountId);
            MonitorForNewConnectionsPhase monitorForNewConnectionsPhase = null;
            try
            {
                monitorForNewConnectionsPhase = await _dbContext.MonitorForNewConnectionsPhases
                    .Where(p => p.SocialAccountId == socialAccountId)
                    .SingleAsync(ct);

                _logger.LogDebug("Successfully retrieved MonitorForNewConnectionsPhase by social account id {socialAccountId}", socialAccountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve MonitorForNewConnectionsPhase by social account id {socialAccountId}. Returning an explicit null", socialAccountId);
                return null;
            }
            return monitorForNewConnectionsPhase;
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
        {
            if (!await PhaseExists(id, ct))
            {
                return false;
            }

            try
            {
                MonitorForNewConnectionsPhase toRemove = _dbContext.MonitorForNewConnectionsPhases.Find(id);
                _dbContext.MonitorForNewConnectionsPhases.Remove(toRemove);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Failed to delete MonitorForNewConnectionsPhase with id: {id}", id);
                return false;
            }

            return true;
        }
    }
}
