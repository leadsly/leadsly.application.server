using Hangfire;
using Leadsly.Application.Model.Entities;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Leadsly.Domain.Services
{
    public class LeadslyRecurringJobsManager : ILeadslyRecurringJobsManager
    {
        public const string HalActiveCampaigns = "ActiveCampaigns_{HalId}";
        public LeadslyRecurringJobsManager(ILogger<LeadslyRecurringJobsManager> logger, IHangfireService hangfireService)
        {
            _logger = logger;
            _hangfireService = hangfireService;
        }

        private readonly ILogger<LeadslyRecurringJobsManager> _logger;
        private readonly IHangfireService _hangfireService;        

        public bool OnboardNewHalUnit(HalUnit halUnit)
        {
            using (var connection = JobStorage.Current.GetConnection())
            {
                string activeCampJob = HalActiveCampaigns.Replace("{HalId}", halUnit.HalId);
                Dictionary<string, string> halActiveCampaignsRecurringJob = connection.GetAllEntriesFromHash($"recurring-job:{activeCampJob}");

                if(halActiveCampaignsRecurringJob.Count == 0)
                {
                    TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(halUnit.TimeZoneId);
                    _hangfireService.AddOrUpdate<IRecurringJobsHandler>(activeCampJob, (x) => x.CreateAndPublishJobsByHalIdAsync(halUnit.HalId), HangfireService.ActiveCampaignsCronSchedule, tzInfo);
                }
            }

            return true;
        }
    }
}
