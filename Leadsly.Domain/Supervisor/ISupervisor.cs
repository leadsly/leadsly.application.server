using Leadsly.Application.Model;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Application.Model.Requests;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.ViewModels;
using Leadsly.Application.Model.ViewModels.Campaigns;
using Leadsly.Application.Model.ViewModels.Cloud;
using Leadsly.Application.Model.ViewModels.Response;
using Leadsly.Domain.Models.Requests;
using Leadsly.Domain.Models.ViewModels;
using Leadsly.Domain.Models.ViewModels.LinkedInAccount;
using Leadsly.Domain.Models.ViewModels.VirtualAssistant;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public interface ISupervisor
    {
        Task<Customer_Stripe> AddCustomerAsync_Stripe(Customer_Stripe stripeCustomerViewModel);
        Task<CampaignViewModel> PatchUpdateCampaignAsync(string campaignId, JsonPatchDocument<Campaign> campaignUpdate, CancellationToken ct = default);
        Task<HalOperationResultViewModel<T>> PatchUpdateSocialAccountAsync<T>(string socialAccountId, JsonPatchDocument<SocialAccount> socialAccountUpdate, CancellationToken ct = default)
            where T : IOperationResponseViewModel;

        [Obsolete("No longer in use")]
        Task<SetupAccountResultViewModel> LeadslyAccountSetupAsync(SetupAccountViewModel setup, CancellationToken ct = default);

        Task<HalOperationResultViewModel<T>> LeadslyAccountSetupAsync<T>(SetupAccountViewModel setup, CancellationToken ct = default)
            where T : IOperationResponseViewModel;

        Task<VirtualAssistantViewModel> CreateVirtualAssistantAsync(CreateVirtualAssistantRequest request, CancellationToken ct = default);
        Task<VirtualAssistantInfoViewModel> GetVirtualAssistantInfoAsync(string userId, CancellationToken ct = default);

        [Obsolete("This method is not longer used. We are not creating new chrome instances per campaign, we're using new tabs instead")]
        Task<HalOperationResultViewModel<T>> LeadslyRequestNewWebDriverAsync<T>(NewWebDriverRequest request, CancellationToken ct = default)
            where T : IOperationResponseViewModel;
        Task<HalOperationResultViewModel<T>> LeadslyAuthenticateUserAsync<T>(ConnectAccountRequest request, CancellationToken ct = default)
            where T : IOperationResponseViewModel;
        Task<ConnectLinkedInAccountResultViewModel> LinkLinkedInAccount(ConnectLinkedInAccountRequest request, string userId, CancellationToken ct = default);
        Task<HalOperationResultViewModel<T>> LeadslyTwoFactorAuthAsync<T>(TwoFactorAuthRequest request, CancellationToken ct = default)
            where T : IOperationResponseViewModel;

        Task<HalOperationResult<T>> ProcessProspectsAsync<T>(CollectedProspectsRequest request, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> UpdateProspectListPhaseAsync<T>(string prospectListPhaseId, JsonPatchDocument<ProspectListPhase> prospectListPhaseUpdate, CancellationToken ct)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> ProcessCampaignProspectsRepliedAsync<T>(ProspectsRepliedRequest request, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> ProcessProspectsRepliedAsync<T>(ProspectsRepliedRequest request, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> ProcessConnectionRequestSentForCampaignProspectsAsync<T>(CampaignProspectListRequest request, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> ProcessFollowUpMessageSentAsync<T>(FollowUpMessageSentRequest request, CancellationToken ct = default)
            where T : IOperationResponse;

        Task TriggerSendConnectionsPhaseAsync(TriggerSendConnectionsRequest request, CancellationToken ct = default);

        Task TriggerScanProspectsForRepliesPhaseAsync(TriggerScanProspectsForRepliesRequest request, CancellationToken ct = default);

        Task TriggerFollowUpMessagesPhaseAsync(TriggerFollowUpMessageRequest request, CancellationToken ct = default);

        Task<HalOperationResult<T>> ProcessNewlyAcceptedProspectsAsync<T>(NewProspectsConnectionsAcceptedRequest request, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResultViewModel<T>> CreateCampaignAsync<T>(CreateCampaignRequest request, string userId, CancellationToken ct = default)
            where T : IOperationResponseViewModel;

        Task<HalOperationResult<T>> GetSentConnectionsUrlStatusesAsync<T>(string campaignId, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> GetSearchUrlProgressAsync<T>(string campaignId, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> UpdateSentConnectionsUrlStatusesAsync<T>(string campaignId, UpdateSearchUrlDetailsRequest request, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> UpdateSearchUrlProgressAsync<T>(string searchUrlProgressId, JsonPatchDocument<SearchUrlProgress> searchUrlProgressToUpdate, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<ConnectedViewModel> GetConnectedAccountAsync(string userId, CancellationToken ct = default);

        Task<HalOperationResultViewModel<T>> GetProspectListsByUserIdAsync<T>(string userId, CancellationToken ct = default)
            where T : IOperationResponseViewModel;

        Task<IList<TimeZoneViewModel>> GetSupportedTimeZonesAsync(CancellationToken ct = default);
    }
}
