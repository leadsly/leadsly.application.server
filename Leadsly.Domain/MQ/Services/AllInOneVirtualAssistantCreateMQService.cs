using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.MQ.Messages;
using Leadsly.Domain.MQ.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Services
{
    public class AllInOneVirtualAssistantCreateMQService : IAllInOneVirtualAssistantCreateMQService
    {
        public AllInOneVirtualAssistantCreateMQService(
            ILogger<AllInOneVirtualAssistantCreateMQService> logger,
            IAllInOneVirtualAssistantFactory factory,
            IMQCreatorFacade facade,
            ICampaignRepositoryFacade repositoryFacade)
        {
            _logger = logger;
            _factory = factory;
            _facade = facade;
            _repositoryFacade = repositoryFacade;
        }

        private readonly ILogger<AllInOneVirtualAssistantCreateMQService> _logger;
        private readonly IAllInOneVirtualAssistantFactory _factory;
        private readonly ICampaignRepositoryFacade _repositoryFacade;
        private readonly IMQCreatorFacade _facade;

        public async Task<PublishMessageBody> CreateMQMessageAsync(string halId, CancellationToken ct = default)
        {
            AllInOneVirtualAssistantMessageBody mqMessage = await _factory.CreateMQMessageAsync(halId, ct) as AllInOneVirtualAssistantMessageBody;
            if (mqMessage == null)
            {
                _logger.LogError("Failed to create {0}. It will not be published", nameof(AllInOneVirtualAssistantMessageBody));
            }

            return mqMessage;
        }

        public async Task SetCheckOffHoursNewConnectionsProperties(string halId, bool initial, PublishMessageBody mqMessage, CancellationToken ct = default)
        {
            if (initial == true)
            {
                CheckOffHoursNewConnectionsBody message = await _facade.CreateCheckOffHoursNewConnectionsMQMessageAsync(halId, ct) as CheckOffHoursNewConnectionsBody;
                if (message == null)
                {
                    _logger.LogWarning("{0} will not be executed witht his {1} phase", nameof(CheckOffHoursNewConnectionsBody), nameof(AllInOneVirtualAssistantMessageBody));
                }
                else
                {
                    ((AllInOneVirtualAssistantMessageBody)mqMessage).CheckOffHoursNewConnections = message;
                }
            }
        }

        public async Task SetDeepScanProspectsForRepliesProperties(string halId, bool initial, PublishMessageBody mqMessage, CancellationToken ct = default)
        {
            if (initial == true)
            {
                if (await ShouldPublishDeepScanAsync(halId, ct) == true)
                {
                    // if null it means there are no active campaigns or something went wrong
                    DeepScanProspectsForRepliesBody message = await _facade.CreateDeepScanProspectsForRepliesMQMessageAsync(halId, ct) as DeepScanProspectsForRepliesBody;
                    if (message == null)
                    {
                        _logger.LogWarning("{0} will not be exected with this {1} phase.", nameof(DeepScanProspectsForRepliesBody), nameof(AllInOneVirtualAssistantMessageBody));
                    }
                    else
                    {
                        ((AllInOneVirtualAssistantMessageBody)mqMessage).DeepScanProspectsForReplies = message;
                    }
                }
            }
        }

        private async Task<bool> ShouldPublishDeepScanAsync(string halId, CancellationToken ct = default)
        {
            IList<CampaignProspect> campaignProspects = await _repositoryFacade.GetAllActiveCampaignProspectsByHalIdAsync(halId);
            if (campaignProspects.Any(p => p.Accepted == true && p.FollowUpMessageSent == true && p.Replied == false && p.FollowUpComplete == false) == true)
            {
                return true;
            }
            return false;
        }
    }
}
