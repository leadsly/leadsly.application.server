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
    public class PrimaryProspectRepository : IPrimaryProspectRepository
    {
        public PrimaryProspectRepository(DatabaseContext dbContext, ILogger<PrimaryProspectRepository> logger)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private readonly DatabaseContext _dbContext;
        private readonly ILogger<PrimaryProspectRepository> _logger;

        public async Task<PrimaryProspectList> GetListByNameAndUserIdAsync(string prospectListName, string userId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving PrimaryProspectList by primary prospect list name: {prospectListName} and by user id: {userId}", prospectListName, userId);
            PrimaryProspectList primaryProspectList = default;
            try
            {
                primaryProspectList = await _dbContext.PrimaryProspectLists
                    .Where(p => p.Name == prospectListName && p.UserId == userId)
                    .SingleAsync(ct);

                _logger.LogDebug("Successfully retrieved PrimaryProspectList by primary list name: {prospectListName} and user id: {userId}", prospectListName, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve PrimaryProspectList by primary prospect list name {prospectListName} and user id {userId}. Returning an explict null", prospectListName, userId);
                return null;
            }

            return primaryProspectList;
        }

        public async Task<PrimaryProspectList> CreateListAsync(PrimaryProspectList primaryProspectList, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating PrimaryProspectList");
            try
            {
                _dbContext.PrimaryProspectLists.Add(primaryProspectList);
                await _dbContext.SaveChangesAsync(ct);
                string primaryProspectListId = primaryProspectList.PrimaryProspectListId;
                _logger.LogDebug("Successfully created PrimaryProspectList. New PrimaryProspectList id is: {primaryProspectListId}", primaryProspectListId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create a new PrimaryProspectList. Returning an explicit null");
                return null;
            }

            return primaryProspectList;
        }

        public async Task<PrimaryProspectList> GetListByIdAsync(string primaryProspectListId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving PrimaryProspectList by id {primaryProspectListId}", primaryProspectListId);
            PrimaryProspectList primaryProspectList = default;
            try
            {
                primaryProspectList = await _dbContext.PrimaryProspectLists
                    .Where(p => p.PrimaryProspectListId == primaryProspectListId)
                    .Include(p => p.PrimaryProspects)
                    .SingleAsync(ct);

                _logger.LogDebug("Successfully retrieved PrimaryProspectList by id {primaryProspectListId}", primaryProspectListId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve PrimaryProspectList by id {primaryProspectListId}. Returning an explict null", primaryProspectListId);
                return null;
            }

            return primaryProspectList;
        }

        public async Task<PrimaryProspect> GetByIdAsync(string primaryProspectId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving PrimaryProspect by id {primaryProspectId}", primaryProspectId);
            PrimaryProspect primaryProspect = default;
            try
            {
                primaryProspect = await _dbContext.PrimaryProspects
                    .Where(p => p.PrimaryProspectId == primaryProspectId)
                    .SingleAsync(ct);

                _logger.LogDebug("Successfully retrieved PrimaryProspect by id {primaryProspectId}", primaryProspectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve PrimaryProspect by id {primaryProspectId}. Returning an explicit null", primaryProspectId);
                return null;
            }

            return primaryProspect;
        }

        public async Task<IList<PrimaryProspect>> CreateAllAsync(IList<PrimaryProspect> primaryProspectList, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating PrimaryProspects from a list");
            try
            {
                await _dbContext.PrimaryProspects.AddRangeAsync(primaryProspectList);
                await _dbContext.SaveChangesAsync(ct);
                _logger.LogDebug("Successfully created PrimaryProspects from a list");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add PrimaryProspects from a list. Returning an explicit null");
                return null;
            }
            return primaryProspectList;
        }

        public async Task<IList<PrimaryProspectList>> GetListsByUserIdAsync(string userId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving PrimaryProspectLists by user id {userId}", userId);
            IList<PrimaryProspectList> primaryProspectLists = default;
            try
            {
                primaryProspectLists = await _dbContext.PrimaryProspectLists
                    .Where(p => p.UserId == userId)
                    .ToListAsync(ct);

                _logger.LogDebug("Successfully retrieved PrimaryProspectLists by user id {userId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve PrimaryProspectLists by user id {userId}. Returning an explicit null", userId);
                return null;
            }

            return primaryProspectLists;
        }
    }
}
