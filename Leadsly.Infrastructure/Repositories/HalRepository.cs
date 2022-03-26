using Leadsly.Application.Model.Entities;
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

        public async Task<HalUnit> AddHalDetailsAsync(HalUnit halDetails, CancellationToken ct = default)
        {
            try
            {
                _dbContext.HalUnits.Add(halDetails);
                await _dbContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured adding hal details to the database");
            }

            return halDetails;
        }   
        
        public async Task<HalUnit> GetHalDetailsByConnectedAccountUsernameAsync(string connectedAccountUsername, CancellationToken ct = default)
        {
            HalUnit halDetails = null;
            try
            {
                // should always be just one
                halDetails = await _dbContext.HalUnits.Include(h => h.SocialAccount).Where(h => h.SocialAccount.Username == connectedAccountUsername).SingleAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured getting hal details for {connectedAccountUsername}", connectedAccountUsername);
            }
            return halDetails;
        }
    }
}
