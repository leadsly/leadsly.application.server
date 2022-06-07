using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers.Interfaces
{
    public interface ISendFollowUpMessageProvider
    {
        Task<IDictionary<CampaignProspectFollowUpMessage, DateTimeOffset>> CreateSendFollowUpMessagesAsync(IList<CampaignProspect> campaignProspects, CancellationToken ct = default);
        Task ScheduleFollowUpMessageAsync(FollowUpMessageBody followUpMessageBody, string queueNameIn, string routingKeyIn, string halId, DateTimeOffset scheduleTime, CancellationToken ct = default);
    }
}
