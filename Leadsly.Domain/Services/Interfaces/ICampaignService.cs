using Leadsly.Domain.Models.Entities.Campaigns;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface ICampaignService
    {
        Task<int> CreateDailyWarmUpLimitConfigurationAsync(long startDateTimestamp, CancellationToken ct = default);

        CampaignProspectList GenerateCampaignProspectList(PrimaryProspectList primaryProspectList, string userId);
    }
}
