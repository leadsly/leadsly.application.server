﻿using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Facades.Interfaces
{
    public interface ISerializerFacade
    {
        Task<HalOperationResult<T>> DeserializeConnectAccountResponseAsync<T>(HttpResponseMessage response)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> DeserializeEnterTwoFactorAuthCodeResponseAsync<T>(HttpResponseMessage response)
            where T : IOperationResponse;

        byte[] SerializeProspectList(ProspectListBody content);

        byte[] SerializeScanProspectsForReplies(ScanProspectsForRepliesBody content);

        byte[] SerializeMonitorForNewAcceptedConnections(MonitorForNewAcceptedConnectionsBody content);

        byte[] SerializeSendConnections(SendConnectionsBody content);
    }
}
