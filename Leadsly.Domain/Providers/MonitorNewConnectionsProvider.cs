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


        public async Task<HalOperationResult<T>> ProcessNewConnectionsAsync<T>(List<NewConnectionProspect> newConnectionProspects, string halId, CancellationToken ct = default)
            where T : IOperationResponse
        {
            throw new NotImplementedException();
            //HalOperationResult<T> result = new();

            //List<Campaign> activeCampaigns = await _campaignRepository.GetAllActiveByHalIdAsync(halId, ct);
            //List<ProspectListPhase> prospectListPhases = activeCampaigns.Select(c => c.ProspectListPhase).ToList();
            //List<Prospect> linkedInProspects = prospectListPhases.SelectMany(p => p.Prospects).ToList();
            //List<Prospect> followUpMessageProspects = new();

            //foreach (NewConnectionProspect newMyNetworkProspect in newConnectionProspects)
            //{
            //    // this won't work exactly, it needs to handle profile url normalization because my new network prospect url is only relative
            //    Prospect prosp = linkedInProspects.Find(p => p.ProfileUrl == newMyNetworkProspect.ProfileUrl);
            //    if (prosp != null)
            //    {
            //        // grab first follow up message for campaign id
            //        followUpMessageProspects.Add(prosp);
            //    }
            //}

            //if (followUpMessageProspects.Count == 0)
            //{
            //    // if none of these prospects came from us move on
            //    _logger.LogInformation("None of the new my network prospects came from any of our campaigns");
            //    result.Succeeded = true;
            //    return result;
            //}

            //return await GetFirstFollowUpMessagesAsync<T>(followUpMessageProspects, ct);
        }

        //public async Task<HalOperationResult<T>> GetFirstFollowUpMessagesAsync<T>(List<Prospect> followUpMessageProspects, CancellationToken ct = default)
        //    where T : IOperationResponse
        //{
        //    foreach (Prospect prospect in followUpMessageProspects)
        //    {
        //        string campaignId = prospect.ProspectList;
        //        FollowUpMessage msg = await _campaignRepository.GetFollowUpMessageByCampaignIdAsync(1, prospect.ProspectListPhase.CampaignId, ct);
        //        if(msg == null)
        //        {
        //            _logger.LogWarning("Could not locate message with the order 1 for campaign id {campaignId}", campaignId);
        //            continue;
        //        }
        //    }
        //}
    }
}
