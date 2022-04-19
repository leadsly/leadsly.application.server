using Leadsly.Domain.Campaigns.Commands;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public class RecurringJobsHandler : IRecurringJobsHandler
    {
        public RecurringJobsHandler(IServiceProvider serviceProbider)
        {
            _serviceProbider = serviceProbider;
        }

        private readonly IServiceProvider _serviceProbider;

        public async Task CreateAndPublishJobsAsync()
        {
            using (var scope = _serviceProbider.CreateScope())
            {
                ICampaignPhaseCommandProducer campaignPhaseCommandProducer = scope.ServiceProvider.GetService<ICampaignPhaseCommandProducer>();

                IList<ICommand> commands = campaignPhaseCommandProducer.CreateRecurringJobCommands();

                ICampaignManager campaignManager = scope.ServiceProvider.GetService<ICampaignManager>();
                campaignManager.SetCommands(commands);

                await campaignManager.ExecuteAllAsync();
            } 
        }
    }
}
