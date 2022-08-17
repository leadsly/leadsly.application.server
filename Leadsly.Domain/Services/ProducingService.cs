using Leadsly.Application.Model;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Leadsly.Domain.Services
{
    public class ProducingService : IProducingService
    {
        public ProducingService(ILogger<ProducingService> logger, IWebHostEnvironment env, IHangfireService hangfireService)
        {
            _logger = logger;
            _env = env;
            _hangfireService = hangfireService;
        }

        private readonly IHangfireService _hangfireService;
        private readonly ILogger<ProducingService> _logger;
        private readonly IWebHostEnvironment _env;

        public void StartRecurringJobs()
        {
            _logger.LogInformation("Starting to execute StartRecurringJobs");
            //*************************** Daily Jobs ***************************
            ////////////////////////////////////////////////////////////////////
            if (_env.IsDevelopment())
            {
                _logger.LogInformation("Current enviornment is in Development. Executing 'ScheduleJobsForNewTimeZonesAsync' right away");
                _hangfireService.Enqueue<IRecurringJobsHandler>((x) => x.ScheduleJobsForNewTimeZonesAsync());

                _logger.LogInformation("Current enviornment is in Development. Executing 'ScheduleRestartJobsForNewTimeZonesAsync' right away");
                _hangfireService.Enqueue<IRecurringJobsHandler>((x) => x.ScheduleRestartJobsForNewTimeZonesAsync());
            }
            else
            {
                _logger.LogInformation("Current enviornment is NOT in Development. Executing 'ScheduleJobsForNewTimeZonesAsync' on scheduled basis");
                TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

                _logger.LogTrace("Server will execute 'ScheduleJobsForNewTimeZonesAsync' on a daily cron schedule using 'Eastern Standard Time'");
                _logger.LogDebug($"The 'ScheduleJobsForNewTimeZonesAsync' recurring job has an id of {HangFireConstants.RecurringJobs.ScheduleNewTimeZones}");
                _hangfireService.AddOrUpdate<IRecurringJobsHandler>(HangFireConstants.RecurringJobs.ScheduleNewTimeZones, (x) => x.ScheduleJobsForNewTimeZonesAsync(), HangFireConstants.RecurringJobs.DailyCronSchedule, tzInfo);

                _logger.LogTrace("Server will execute 'ScheduleRestartJobsForNewTimeZonesAsync' on a daily cron schedule using 'Eastern Standard Time'");
                _logger.LogDebug($"The 'ScheduleRestartJobsForNewTimeZonesAsync' recurring job has an id of {HangFireConstants.RecurringJobs.ScheduleNewTimeZones}");
                _hangfireService.AddOrUpdate<IRecurringJobsHandler>(HangFireConstants.RecurringJobs.ScheduleRestartForNewTimeZones, (x) => x.ScheduleRestartJobsForNewTimeZonesAsync(), HangFireConstants.RecurringJobs.DailyCronSchedule, tzInfo);
            }
        }
    }
}
