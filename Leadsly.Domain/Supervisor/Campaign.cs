using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Campaigns.Interfaces;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Application.Model.Requests;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.ViewModels;
using Leadsly.Application.Model.ViewModels.Campaigns;
using Leadsly.Application.Model.ViewModels.Response;
using Leadsly.Domain.Campaigns;
using Leadsly.Domain.Converters;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public async Task<CampaignViewModel> PatchUpdateCampaignAsync(string campaignId, JsonPatchDocument<Campaign> patchDoc, CancellationToken ct = default)
        {
            return await _campaignProvider.PatchUpdateCampaignAsync(campaignId, patchDoc, ct);
        }

        public async Task<HalOperationResult<T>> UpdateSentConnectionsUrlStatusesAsync<T>(string campaignId, UpdateSentConnectionsUrlStatusRequest request, CancellationToken ct = default)
            where T : IOperationResponse
        {
            return await _campaignProvider.UpdateSentConnectionsUrlStatusesAsync<T>(campaignId, request, ct);
        }

        public async Task<HalOperationResult<T>> GetSentConnectionsUrlStatusesAsync<T>(string campaignId, CancellationToken ct = default)
            where T : IOperationResponse
        {
            return await _campaignProvider.GetSentConnectionsUrlStatusesAsync<T>(campaignId, ct);
        }

        public async Task<HalOperationResultViewModel<T>> CreateCampaignAsync<T>(CreateCampaignRequest request, string userId, CancellationToken ct = default)
            where T : IOperationResponseViewModel
        {
            return await _campaignProvider.CreateCampaignAsync<T>(request, userId, ct);
        }

        public async Task<HalOperationResult<T>> ProcessProspectsRepliedAsync<T>(ProspectsRepliedRequest request, CancellationToken ct = default)
            where T : IOperationResponse
        {
            return await _campaignProvider.ProcessProspectsRepliedAsync<T>(request, ct);
        }

    }
}
