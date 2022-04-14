﻿using Leadsly.Application.Model.Entities.Campaigns;
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
    public class CampaignProspectRepository : ICampaignProspectRepository
    {
        public CampaignProspectRepository(DatabaseContext dbContext, ILogger<CampaignProspectRepository> logger)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private readonly DatabaseContext _dbContext;
        private readonly ILogger<CampaignProspectRepository> _logger;

        public async Task<IList<CampaignProspect>> CreateAllAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default)
        {
            _logger.LogInformation("Creating CampaignProspects from a list");
            try
            {
                await _dbContext.CampaignProspects.AddRangeAsync(campaignProspects);
                await _dbContext.SaveChangesAsync(ct);

                _logger.LogDebug("Successfully created CampaignProspects from a list");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create CampaignProspects from a list. Returning an explicit null");
                return null;
            }

            return campaignProspects;
        }

        public async Task<IList<CampaignProspect>> GetAllByCampaignIdAsync(string campaignId, CancellationToken ct = default)
        {
            _logger.LogDebug("Retrieving all CampaignProspects by campaign id {campaignId}", campaignId);
            IList<CampaignProspect> campaignProspects = default;
            try
            {
                campaignProspects = await _dbContext.CampaignProspects
                    .Where(c => c.CampaignId == campaignId)
                    .Include(p => p.PrimaryProspect)
                    .ToListAsync(ct);

                _logger.LogDebug("Successfully retrieved all CampaignProspects by campaign id {campaignId}", campaignId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all CampaignProspects by campaign id {campaignid}. Returning an explicit null", campaignId);
                return null;
            }
            return campaignProspects;
        }

        public async Task<CampaignProspectList> GetListByListIdAsync(string campaignProspectListId, CancellationToken ct = default)
        {
            _logger.LogInformation("Retrieving CampaignProspectList by id {campaignProspectListId}", campaignProspectListId);

            CampaignProspectList campaignProspectList = default;
            try
            {
                 campaignProspectList = await _dbContext.CampaignProspectLists
                    .Where(cpl => cpl.CampaignProspectListId == campaignProspectListId)
                    .Include(cpl => cpl.CampaignProspects)
                    .SingleAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get CampaignProspectList by its id {campaignProspectListId}. Returning an explicit null", campaignProspectListId);
                return null;
            }

            return campaignProspectList;
        }

        public async Task<IList<CampaignProspect>> UpdateAllAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default)
        {
            _logger.LogInformation("Updating all CampaignProspects from a list");
            try
            {
                _dbContext.CampaignProspects.UpdateRange(campaignProspects);
                await _dbContext.SaveChangesAsync(ct);

                _logger.LogDebug("Successfully updated all CampaignProspects from a list");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update all CampaignProspects from a list. Returning an explicit null");
                return null;
            }

            return campaignProspects;
        }
    }
}