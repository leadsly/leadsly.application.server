using Leadsly.Application.Model.Entities;
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
    public class TimeZoneRepository : ITimeZoneRepository
    {
        public TimeZoneRepository(DatabaseContext dbContext, ILogger<TimeZoneRepository> logger)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private readonly ILogger<TimeZoneRepository> _logger;
        private readonly DatabaseContext _dbContext;

        public async Task<HalTimeZone> AddHalTimeZoneAsync(HalTimeZone newHalInTimeZone, CancellationToken ct = default)
        {
            _logger.LogInformation($"Adding entry into the HalTimeZone table for HalId {newHalInTimeZone.HalId} for time zone {newHalInTimeZone.HalTimeZoneId}");            
            try
            {
                _dbContext.HalTimeZones.Add(newHalInTimeZone);
                await _dbContext.SaveChangesAsync(ct);
                _logger.LogInformation($"Successfully saved HalTimeZone for HalId {newHalInTimeZone.HalId} for time zone {newHalInTimeZone.HalTimeZoneId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to save the new HalTimeZone for HalId {newHalInTimeZone.HalId} for time zone {newHalInTimeZone.HalTimeZoneId}");
            }
            return newHalInTimeZone;
        }

        public async Task<IList<HalTimeZone>> GetAllByTimeZoneIdAsync(string timeZoneId, CancellationToken ct = default)
        {
            _logger.LogInformation($"Retrieving list of all HalTimeZones for time zone {timeZoneId}");
            IList<HalTimeZone> halTimeZones = default;
            try
            {
                halTimeZones = await _dbContext.HalTimeZones.Where(h => h.TimeZoneId == timeZoneId).ToListAsync(ct);
                _logger.LogInformation($"Successfully retrieved HalTimeZones for time zone {timeZoneId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve all of HalTimeZones for time zone {timeZoneId}");
            }
            return halTimeZones;
        }

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
