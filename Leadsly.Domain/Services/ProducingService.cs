using Hangfire;
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
                _logger.LogInformation("Enquing CreateAndPublishJobsAsync");                
                _hangfireService.Enqueue<IRecurringJobsHandler>((x) => x.CreateAndPublishJobsAsync());
            }
            else
            {
                _logger.LogDebug($"Local timezone is {TimeZoneInfo.Local}");
                _logger.LogInformation("Executing RecurringJob.AddOrUpdate");
                // run the server under Eastern Standard Time timezone
                TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

                _hangfireService.AddOrUpdate<IRecurringJobsHandler>("activeCampaigns", (x) => x.CreateAndPublishJobsAsync(), Cron.Daily(6, 40), tzInfo);
            }
        }
    }
}
