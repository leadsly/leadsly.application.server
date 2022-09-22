using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.MQ.Creators.Interfaces;
using Leadsly.Domain.MQ.Messages;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Facades
{
    public class MQCreatorFacade : IMQCreatorFacade
    {
        public MQCreatorFacade(
            IFollowUpMessagesMQCreator followUpMessagesMQCreator,
            IMonitorForNewConnectionsMQCreator monitorForNewConnectionsMQCreator,
            INetworkingMQCreator networkingMQCreator,
            IScanProspectsForRepliesMQCreator scanProspectsForRepliesMQCreator,
            IDeepScanProspectsForRepliesMQCreator deepScanProspectsForRepliesMQCreator,
            ICheckOffHoursNewConnectionsMQCreator checkOffHoursNewConnectionsMQCreator
            )
        {
            _checkOffHoursNewConnectionsMQCreator = checkOffHoursNewConnectionsMQCreator;
            _deepScanProspectsForRepliesMQCreator = deepScanProspectsForRepliesMQCreator;
            _followUpMessagesMQCreator = followUpMessagesMQCreator;
            _monitorForNewConnectionsMQCreator = monitorForNewConnectionsMQCreator;
            _networkingMQCreator = networkingMQCreator;
            _scanProspectsForRepliesMQCreator = scanProspectsForRepliesMQCreator;
        }

        private readonly ICheckOffHoursNewConnectionsMQCreator _checkOffHoursNewConnectionsMQCreator;
        private readonly IDeepScanProspectsForRepliesMQCreator _deepScanProspectsForRepliesMQCreator;
        private readonly IFollowUpMessagesMQCreator _followUpMessagesMQCreator;
        private readonly IMonitorForNewConnectionsMQCreator _monitorForNewConnectionsMQCreator;
        private readonly INetworkingMQCreator _networkingMQCreator;
        private readonly IScanProspectsForRepliesMQCreator _scanProspectsForRepliesMQCreator;

        public async Task PublishFollowUpMessagesMessageAsync(string halId, IList<Campaign> campaigns, CancellationToken ct = default)
        {
            await _followUpMessagesMQCreator.PublishMessageAsync(halId, campaigns, ct);
        }

        public async Task PublishMonitorForNewConnectionsMessageAsync(string halId, CancellationToken ct = default)
        {
            await _monitorForNewConnectionsMQCreator.PublishMessageAsync(halId, ct);
        }

        public async Task PublishNetworkingMessageAsync(string halId, Campaign campaign, CancellationToken ct = default)
        {
            await _networkingMQCreator.PublishMessageAsync(halId, campaign);
        }

        public async Task PublishScanProspectsForRepliesMessageAsync(string halId, CancellationToken ct = default)
        {
            await _scanProspectsForRepliesMQCreator.PublishMessageAsync(halId, ct);
        }

        public async Task<PublishMessageBody> CreateDeepScanProspectsForRepliesMQMessageAsync(string halId, CancellationToken ct = default)
        {
            return await _deepScanProspectsForRepliesMQCreator.CreateMQMessageAsync(halId, ct);
        }

        public async Task<PublishMessageBody> CreateCheckOffHoursNewConnectionsMQMessageAsync(string halId, CancellationToken ct = default)
        {
            return await _checkOffHoursNewConnectionsMQCreator.CreateMQMessage(halId, ct);
        }
    }
}
