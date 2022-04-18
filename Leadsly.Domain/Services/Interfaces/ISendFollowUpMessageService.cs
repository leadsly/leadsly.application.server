using Leadsly.Application.Model.Entities.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface ISendFollowUpMessageService
    {
        Task<IDictionary<CampaignProspectFollowUpMessage, DateTimeOffset>> SendFollowUpMessagesAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default);
    }
}
