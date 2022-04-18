using Leadsly.Application.Model.Entities;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class HalWorkManager : IHalWorkManager
    {
        public HalWorkManager(Logger<HalWorkManager> logger, ITimestampService timestampService)
        {
            _logger = logger;
            _timestampService = timestampService;
        }

        private readonly ILogger<HalWorkManager> _logger;
        private readonly ITimestampService _timestampService;

        public async Task<bool> IsNowWithinWorkHoursAsync(string halId, CancellationToken ct = default)
        {
            _logger.LogInformation("Checking if DateTime.Now is within hal's work day. Hal id {halId}", halId);

            DateTimeOffset now = await _timestampService.CreateNowDatetimeOffsetAsync(halId, ct);
            DateTimeOffset start = await _timestampService.GetStartWorkDayAsync(halId, ct);
            DateTimeOffset end = await _timestampService.GetEndWorkDayAsync(halId, ct);

            if(now > start && now < end)
            {
                return true;
            }

            return false;
        }
    }
}
