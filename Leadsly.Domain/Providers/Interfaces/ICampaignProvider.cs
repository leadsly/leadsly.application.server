using Leadsly.Application.Model;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.ViewModels;
using Leadsly.Application.Model.ViewModels.Response;
using Leadsly.Domain.Models.Requests;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers.Interfaces
{
    public interface ICampaignProvider
    {
        Task<IList<ProspectListPhase>> GetIncompleteProspectListPhasesAsync(string halId, CancellationToken ct = default);
        Task<List<string>> GetHalIdsWithActiveCampaignsAsync(CancellationToken ct = default);
        CampaignProspectList CreateCampaignProspectList(PrimaryProspectList primaryProspectList, string userId);
        Task<int> CreateDailyWarmUpLimitConfigurationAsync(long startDateTimestamp, CancellationToken ct = default);
        Task<IList<Campaign>> GetAllActiveByUserIdAsync(string userId, CancellationToken ct = default);

        Task<Campaign> GetCampaignByIdAsync(string campaignId, CancellationToken ct = default);
        Task<Campaign> UpdateCampaignAsync(Campaign campaign, CancellationToken ct = default);

        Task<bool> DeleteCampaignAsync(string campaignId, CancellationToken ct = default);

        Task<HalOperationResult<T>> UpdateSentConnectionsUrlStatusesAsync<T>(string campaignId, UpdateSearchUrlDetailsRequest request, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> GetSentConnectionsUrlStatusesAsync<T>(string campaignId, CancellationToken ct = default)
            where T : IOperationResponse;

        Task<HalOperationResultViewModel<T>> CreateCampaignAsync<T>(CreateCampaignRequest request, string userId, CancellationToken ct = default)
            where T : IOperationResponseViewModel;

        Task<HalOperationResult<T>> ProcessCampaignProspectsRepliedAsync<T>(ProspectsRepliedRequest request, CancellationToken ct = default)
           where T : IOperationResponse;

        Task<HalOperationResult<T>> ProcessProspectsRepliedAsync<T>(ProspectsRepliedRequest request, CancellationToken ct = default)
           where T : IOperationResponse;

        Task<IList<Campaign>> GetAllByUserIdAsync(string userId, CancellationToken ct = default);

        Task<long> GetTotalConnectionsSentAsync(string campaignId, CancellationToken ct = default);
        Task<long> GetConnectionsAcceptedAsync(string campaignId, CancellationToken ct = default);
        Task<long> GetRepliesAsync(string campaignId, CancellationToken ct = default);
    }
}
