using Hangfire;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Services.Interfaces;
using Leadsly.Domain.Supervisor;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class ProducingService : IProducingService
    {
        public ProducingService(ILogger<ProducingService> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

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
                BackgroundJob.Enqueue<IRecurringJobsHandler>((x) => x.CreateAndPublishJobsAsync());
            }
            else
            {
                _logger.LogDebug($"Local timezone is {TimeZoneInfo.Local}");
                _logger.LogInformation("Executing RecurringJob.AddOrUpdate");
                RecurringJob.AddOrUpdate<IRecurringJobsHandler>("activeCampaigns", (x) => x.CreateAndPublishJobsAsync(), Cron.Daily(6, 40), TimeZoneInfo.Local);
            }
        }
    }
}
