using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Application.Model.Responses;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Serializers.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Facades
{
    public class SerializerFacade : ISerializerFacade
    {
        public SerializerFacade(ILogger<SerializerFacade> logger, 
            IConnectAccountResponseSerializer connectAccountResponseDeserializer, 
            IEnterTwoFactorAuthCodeResponseSerializer enterTwoFactorAuthCodeResponseDeserializer, 
            ICampaignPhaseSerializer campaignPhaseSerializer)
        {
            _logger = logger;
            _connectAccountResponseDeserializer = connectAccountResponseDeserializer;
            _enterTwoFactorAuthCodeResponseDeserializer = enterTwoFactorAuthCodeResponseDeserializer;
            _campaignPhaseSerializer = campaignPhaseSerializer;
        }

        private readonly ILogger<SerializerFacade> _logger;
        private readonly IConnectAccountResponseSerializer _connectAccountResponseDeserializer;
        private readonly IEnterTwoFactorAuthCodeResponseSerializer _enterTwoFactorAuthCodeResponseDeserializer;
        private readonly ICampaignPhaseSerializer _campaignPhaseSerializer;

        public async Task<HalOperationResult<T>> DeserializeConnectAccountResponseAsync<T>(HttpResponseMessage response) where T : IOperationResponse
        {
            return await _connectAccountResponseDeserializer.DeserializeAsync<T>(response);
        }

        public async Task<HalOperationResult<T>> DeserializeEnterTwoFactorAuthCodeResponseAsync<T>(HttpResponseMessage response) where T : IOperationResponse
        {
            return await _enterTwoFactorAuthCodeResponseDeserializer.DeserializeAsync<T>(response);
        }

        public byte[] SerializeProspectList(ProspectListBody content)
        {
            return _campaignPhaseSerializer.SerializeProspectList(content);
        }

        public byte[] SerializeScanProspectsForReplies(ScanProspectsForRepliesBody content)
        {
            return _campaignPhaseSerializer.SerializeScanProspectsForReplies(content);
        }

        public byte[] SerializeMonitorForNewAcceptedConnections(MonitorForNewAcceptedConnectionsBody content)
        {
            return _campaignPhaseSerializer.SerializeMonitorForNewAcceptedConnections(content);
        }

        public byte[] SerializeSendConnections(SendConnectionsBody content)
        {
            return _campaignPhaseSerializer.SerializeSendConnections(content);
        }
    }
}
