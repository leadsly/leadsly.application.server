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
    public class ProspectListRepository : IProspectListRepository
    {
        public ProspectListRepository(ILogger<ProspectListRepository> logger, DatabaseContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private readonly ILogger<ProspectListRepository> _logger;
        private readonly DatabaseContext _dbContext;

        public async Task<PrimaryProspectList> CreatePrimaryProspectListAsync(PrimaryProspectList primaryProspectList, CancellationToken ct = default)
        {
            try
            {
                _dbContext.PrimaryProspectLists.Add(primaryProspectList);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch(Exception ex)
            {

            }

            return primaryProspectList;
        }

        public async Task<PrimaryProspectList> GetPrimaryProspectListByNameAndUserIdAsync(string prospectListName, string userId, CancellationToken ct = default)
        {
            PrimaryProspectList primaryProspectList = default;
            try
            {
                primaryProspectList = await _dbContext.PrimaryProspectLists.Where(p => p.Name == prospectListName && p.UserId == userId).SingleAsync();
            }
            catch (Exception ex)
            {
                primaryProspectList = null;
            }

            return primaryProspectList;
        }
    }
}
