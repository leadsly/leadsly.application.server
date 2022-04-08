using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model;
using Leadsly.Application.Model.ViewModels.Cloud;
using Leadsly.Application.Model.Requests;
using Leadsly.Application.Model.ViewModels.Response;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.ViewModels;
using System;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.ViewModels.Campaigns;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.JsonPatch;

namespace Leadsly.Domain.Supervisor
{
    public interface ISupervisor
    {
        Task<Customer_Stripe> AddCustomerAsync_Stripe(Customer_Stripe stripeCustomerViewModel);
        Task<CampaignViewModel> PatchUpdateCampaignAsync(string campaignId, JsonPatchDocument<Campaign> campaignUpdate, CancellationToken ct = default);
        Task<SetupAccountResultViewModel> LeadslyAccountSetupAsync(SetupAccountViewModel setup, CancellationToken ct = default);

        [Obsolete("This method is not longer used. We are not creating new chrome instances per campaign, we're using new tabs instead")]
        Task<HalOperationResultViewModel<T>> LeadslyRequestNewWebDriverAsync<T>(NewWebDriverRequest request, CancellationToken ct = default)
            where T : IOperationResponseViewModel;
        Task<HalOperationResultViewModel<T>> LeadslyAuthenticateUserAsync<T>(ConnectAccountRequest request, CancellationToken ct = default)
            where T : IOperationResponseViewModel;
        Task<HalOperationResultViewModel<T>> LeadslyTwoFactorAuthAsync<T>(TwoFactorAuthRequest request, CancellationToken ct = default)
            where T : IOperationResponseViewModel;

        Task<HalOperationResult<T>> ProcessNewMyNetworkConnectionsAsync<T>(MyNetworkNewConnectionsRequest request, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> ProcessProspectsAsync<T>(ProspectListPhaseCompleteRequest request, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> ProcessConnectionRequestSentForCampaignProspectsAsync<T>(CampaignProspectListRequest request, CancellationToken ct = default)
            where T : IOperationResponse;

        HalOperationResult<T> TriggerSendConnectionsPhase<T>(TriggerSendConnectionsRequest request, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResultViewModel<T>> CreateCampaignAsync<T>(CreateCampaignRequest request, string userId, CancellationToken ct = default)
            where T : IOperationResponseViewModel;

        Task<HalOperationResult<T>> GetSentConnectionsUrlStatusesAsync<T>(string campaignId, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> UpdateSentConnectionsUrlStatusesAsync<T>(string campaignId, UpdateSentConnectionsUrlStatusRequest request, CancellationToken ct = default)
            where T : IOperationResponse;

    }
}
