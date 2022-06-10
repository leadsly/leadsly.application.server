using Hangfire;
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
            if(_env.IsDevelopment())
            {
                //_hangfireService.Enqueue<IRecurringJobsHandler>((x) => x.CreateAndPublishJobsAsync());
                _hangfireService.Enqueue<IRecurringJobsHandler>((x) => x.ScheduleJobsForNewTimeZonesAsync());
            }
            else
            {
                TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                // 1. Run a daily job that scans for any new supported time zones                            
                _hangfireService.AddOrUpdate<IRecurringJobsHandler>(HangFireConstants.RecurringJobs.ScheduleNewTimeZones, (x) => x.ScheduleJobsForNewTimeZonesAsync(), HangFireConstants.RecurringJobs.DailyCronSchedule, tzInfo);
            }
        }
    }
}
