using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns.MonitorForNewProspects;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Application.Model.Responses;
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
    public class MonitorNewConnectionsProvider : IMonitorNewConnectionsProvider
    {
        public MonitorNewConnectionsProvider(ICampaignRepository campaignRepository, ILogger<MonitorNewConnectionsProvider> logger)
        {
            _campaignRepository = campaignRepository;
            _logger = logger;
        }

        private ICampaignRepository _campaignRepository;
        private ILogger<MonitorNewConnectionsProvider> _logger;

    }
}
