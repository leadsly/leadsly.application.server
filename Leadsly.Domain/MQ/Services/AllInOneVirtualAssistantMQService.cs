using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.MQ.Messages;
using Leadsly.Domain.MQ.Services.Interfaces;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Services
{
    public class AllInOneVirtualAssistantMQService : IAllInOneVirtualAssistantMQService
    {
        public AllInOneVirtualAssistantMQService(
            ILogger<AllInOneVirtualAssistantMQService> logger,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IMQCreatorFacade facade,
            IProvisionResourcesService service)
        {
            _service = service;
            _logger = logger;
            _facade = facade;
            _campaignRepositoryFacade = campaignRepositoryFacade;
        }

        private readonly IProvisionResourcesService _service;
        private readonly ILogger<AllInOneVirtualAssistantMQService> _logger;
        private readonly IMQCreatorFacade _facade;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;

        public async Task<bool> ProvisionResourcesAsync(string halId, string userId, CancellationToken ct = default)
        {
            if (await _service.CreateAwsResourcesAsync(halId, userId, ct) == false)
            {
                _logger.LogError("Failed to provision aws resources for HalId {0} and UserId {1}", halId, userId);
                return false;
            }

            // save it to the database
        }

        public async Task<AllInOneVirtualAssistantMessageBody> CreateMQAllInOneVirtualAssistantMessageAsync(string halId, bool initial, CancellationToken ct = default)
        {
            AllInOneVirtualAssistantMessageBody mqMessage = new();

            await SetDeepScanProspectsForRepliesProperties(halId, initial, mqMessage, ct);

            await SetCheckOffHoursNewConnectionsProperties(halId, initial, mqMessage, ct);

            return mqMessage;
        }

        private async Task SetCheckOffHoursNewConnectionsProperties(string halId, bool initial, AllInOneVirtualAssistantMessageBody mqMessage, CancellationToken ct = default)
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
                    mqMessage.CheckOffHoursNewConnections = message;
                }
            }
        }

        private async Task SetDeepScanProspectsForRepliesProperties(string halId, bool initial, AllInOneVirtualAssistantMessageBody mqMessage, CancellationToken ct = default)
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
                        mqMessage.DeepScanProspectsForReplies = message;
                    }
                }
            }
        }

        private async Task<bool> ShouldPublishDeepScanAsync(string halId, CancellationToken ct = default)
        {
            IList<CampaignProspect> campaignProspects = await _campaignRepositoryFacade.GetAllActiveCampaignProspectsByHalIdAsync(halId);
            if (campaignProspects.Any(p => p.Accepted == true && p.FollowUpMessageSent == true && p.Replied == false && p.FollowUpComplete == false) == true)
            {
                return true;
            }
            return false;
        }
    }
}
