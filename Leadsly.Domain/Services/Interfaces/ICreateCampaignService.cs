using Leadsly.Application.Model.ViewModels.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface ICreateCampaignService
    {
        Task<Campaign> CreateAsync(Campaign newCampaign, CancellationToken ct = default);
        Task<PrimaryProspectList> GetByNameAndUserIdAsync(string prospectListName, string userId, CancellationToken ct = default);
        Task<PrimaryProspectList> CreatePrimaryProspectListAsync(string prospectListName, IList<string> searchUrls, string userId, CancellationToken ct = default);
        IList<FollowUpMessage> CreateFollowUpMessages(Campaign newCampaign, List<FollowUpMessageViewModel> followUpMessages, string userId);
        IList<SearchUrlDetails> CreateSearchUrlDetails(IList<string> searchUrls, Campaign campaign);
        IList<SearchUrlProgress> CreateSearchUrlProgress(IList<string> searchUrls, Campaign campaign);
        IList<SendConnectionsStage> CreateSendConnectionsStages();
    }
}
