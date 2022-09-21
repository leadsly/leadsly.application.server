using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Requests;
using Leadsly.Domain.Models.Responses;
using Leadsly.Domain.Models.ViewModels;
using Leadsly.Domain.Models.ViewModels.Campaigns;
using Leadsly.Domain.Models.ViewModels.LinkedInAccount;
using Leadsly.Domain.Models.ViewModels.Reports;
using Leadsly.Domain.Models.ViewModels.VirtualAssistant;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public interface ISupervisor
    {
        Task<Customer_Stripe> AddCustomerAsync_Stripe(Customer_Stripe stripeCustomerViewModel);
        Task<CampaignViewModel> UpdateCampaignAsync(string campaignId, JsonPatchDocument<Campaign> campaignUpdate, CancellationToken ct = default);
        Task<bool> PatchUpdateSocialAccountAsync(string socialAccountId, JsonPatchDocument<SocialAccount> updates, CancellationToken ct = default);
        Task<CampaignViewModel> CloneCampaignAsync(string campaignId, CancellationToken ct = default);
        Task<CampaignViewModel> GetCampaignByIdAsync(string campaignId, CancellationToken ct = default);
        Task<bool> DeleteCampaignAsync(string campaignId, CancellationToken ct = default);
        Task<GeneralReportViewModel> GetGeneralReportAsync(string userId, CancellationToken ct = default);
        Task<VirtualAssistantViewModel> CreateVirtualAssistantAsync(CreateVirtualAssistantRequest request, CancellationToken ct = default);
        Task<VirtualAssistantInfoViewModel> GetVirtualAssistantInfoAsync(string userId, CancellationToken ct = default);
        Task<DeleteVirtualAssistantViewModel> DeleteVirtualAssistantAsync(string userId, CancellationToken ct = default);
        Task<ConnectLinkedInAccountResultViewModel> LinkLinkedInAccount(ConnectLinkedInAccountRequest request, string userId, IHeaderDictionary responseHeaders, IHeaderDictionary requestHeaders, CancellationToken ct = default);
        Task<TwoFactorAuthResultViewModel> EnterTwoFactorAuthAsync(string userId, TwoFactorAuthRequest request, CancellationToken ct = default);
        Task<EmailChallengePinResultViewModel> EnterEmailChallengePinAsync(string userId, EmailChallengePinRequest request, CancellationToken ct = default);
        Task<bool> ProcessProspectsAsync(CollectedProspectsRequest request, CancellationToken ct = default);
        Task<bool> ProcessCampaignProspectsRepliesAsync(ProspectsRepliedRequest request, CancellationToken ct = default);
        Task<NetworkProspectsResponse> GetAllNetworkProspectsAsync(string halId, CancellationToken ct = default);
        Task<bool> ProcessPotentialProspectsRepliesAsync(string halId, NewMessagesRequest request, CancellationToken ct = default);
        Task<bool> ProcessSentFollowUpMessageAsync(string campaignProspectId, SentFollowUpMessageRequest request, CancellationToken ct = default);
        Task ProcessNewlyAcceptedProspectsAsync(string halId, RecentlyAddedProspectsRequest request, CancellationToken ct = default);
        Task<CampaignViewModel> CreateCampaignAsync(CreateCampaignRequest request, string userId, CancellationToken ct = default);
        Task<GetSearchUrlsProgressResponse> GetSearchUrlProgressAsync(string campaignId, CancellationToken ct = default);
        Task<bool> UpdateSearchUrlProgressAsync(string searchUrlProgressId, JsonPatchDocument<SearchUrlProgress> updates, CancellationToken ct = default);
        Task<ConnectedViewModel> GetConnectedAccountAsync(string userId, CancellationToken ct = default);
        Task<IList<TimeZoneViewModel>> GetSupportedTimeZonesAsync(CancellationToken ct = default);
        Task<CampaignsViewModel> GetCampaignsByUserIdAsync(string userId, CancellationToken ct = default);
        Task<ConnectedNetworkProspectsResponse> GetAllPreviouslyConnectedNetworkProspectsAsync(string halId, CancellationToken ct = default);
        Task UpdatePreviouslyConnectedNetworkProspectsAsync(string halId, UpdateConnectedNetworkProspectsRequest request, CancellationToken ct);
    }
}
