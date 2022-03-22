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

        public Task<List<ProspectListPhase>> GetAllActivePropspectListPhasesAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
