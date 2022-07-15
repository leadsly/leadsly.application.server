using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Infrastructure.Repositories
{
    public class TimeZoneRepository : ITimeZoneRepository
    {
        public TimeZoneRepository(DatabaseContext dbContext, ILogger<TimeZoneRepository> logger)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private readonly ILogger<TimeZoneRepository> _logger;
        private readonly DatabaseContext _dbContext;

        public async Task<IList<LeadslyTimeZone>> GetAllSupportedTimeZonesAsync(CancellationToken ct = default)
        {
            _logger.LogInformation($"Retrieving list of all supported time zones");
            IList<LeadslyTimeZone> supportedTimeZones = default;
            try
            {
                supportedTimeZones = await _dbContext.SupportedTimeZones.ToListAsync(ct);
                _logger.LogInformation("Successfully retrieved all supported time zones");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve all supported time zones");
            }
            return supportedTimeZones;
        }
    }
}
