using Hangfire;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Services.Interfaces;
using Leadsly.Domain.Supervisor;
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
        public ProducingService(ILogger<ProducingService> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<ProducingService> _logger;

        public void StartRecurringJobs()
        {
            //*************************** Daily Jobs ***************************
            ////////////////////////////////////////////////////////////////////

            // RecurringJob.AddOrUpdate<ISupervisor>("activeCampaigns", (x) => x.ProcessAllCampaignsAsync(), Cron.Daily(6, 40));
            BackgroundJob.Enqueue<ICampaignManager>((x) => x.ProcessAllActiveCampaigns());
        }
    }
}
