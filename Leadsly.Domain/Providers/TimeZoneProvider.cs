using Hangfire;
using Leadsly.Application.Model.Entities;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public class TimeZoneProvider : ITimeZoneProvider
    {
        public const string HalActiveCampaigns = "ActiveCampaigns_{HalId}";
        public TimeZoneProvider(ITimeZoneRepository timeZoneRepository, ILogger<TimeZoneProvider> logger)
        {
            _timeZoneRepository = timeZoneRepository;
            _logger = logger;
        }

        private readonly ILogger<TimeZoneProvider> _logger;
        private readonly ITimeZoneRepository _timeZoneRepository;

        public async Task AddHalTimezoneAsync(HalUnit halUnit, CancellationToken ct = default)
        {
            using (var connection = JobStorage.Current.GetConnection())
            {
                string activeCampJob = HalActiveCampaigns.Replace("{HalId}", halUnit.HalId);
                Dictionary<string, string> halActiveCampaignsRecurringJob = connection.GetAllEntriesFromHash($"recurring-job:{activeCampJob}");

                if (halActiveCampaignsRecurringJob.Count == 0)
                {
                    HalTimeZone newHalTimeZone = new()
                    {
                        HalId = halUnit.HalId,
                        TimeZoneId = halUnit.TimeZoneId
                    };

                    await _timeZoneRepository.AddHalTimeZoneAsync(newHalTimeZone, ct);
                }
            }
        }
    }
}
