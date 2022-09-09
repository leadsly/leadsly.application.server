using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.MQ.Messages;
using Leadsly.Domain.MQ.Services.Interfaces;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Services
{
    public class MonitorForNewConnectionsCreateMQService : IMonitorForNewConnectionsCreateMQService
    {
        public MonitorForNewConnectionsCreateMQService(
            ILogger<MonitorForNewConnectionsCreateMQService> logger,
            IVirtualAssistantRepository virtualAssistantRepository,
            IMonitorForNewConnectionsMessagesFactory factory,
            ICampaignRepositoryFacade campaignRepositoryFacade)
        {
            _logger = logger;
            _virtualAssistantRepository = virtualAssistantRepository;
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _factory = factory;
        }

        private readonly IMonitorForNewConnectionsMessagesFactory _factory;
        private readonly IVirtualAssistantRepository _virtualAssistantRepository;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly ILogger<MonitorForNewConnectionsCreateMQService> _logger;

        public async Task<PublishMessageBody> CreateMQMonitorForNewConnectionsMessageAsync(string userId, string halId, MonitorForNewConnectionsPhase phase, CancellationToken ct = default)
        {
            if (await _campaignRepositoryFacade.AnyActiveCampaignsByHalIdAsync(halId, ct) == false)
            {
                _logger.LogDebug("HalId {halId} does not contain any active campaigns", halId);
                return null;
            }

            if (phase == null)
            {
                _logger.LogDebug("ScanProspectsForRepliesPhase does not exist on social account");
                return null;
            }

            VirtualAssistant virtualAssistant = await _virtualAssistantRepository.GetByHalIdAsync(halId, ct);
            if (virtualAssistant == null)
            {
                _logger.LogDebug("Failed to locate virtual assistant for halId {halId}", halId);
                return null;
            }

            return await _factory.CreateMQMessageAsync(userId, halId, virtualAssistant, phase, ct);
        }
    }
}
