using Leadsly.Domain.Models.Entities.Campaigns;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface ICreateFollowUpMessagesService
    {
        Task<IList<CampaignProspectFollowUpMessage>> GenerateProspectsFollowUpMessagesAsync(string halId, CancellationToken ct = default);
    }
}
