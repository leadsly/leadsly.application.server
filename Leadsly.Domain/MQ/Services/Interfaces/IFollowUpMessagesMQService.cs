using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.MQ.Messages;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Services.Interfaces
{
    public interface IFollowUpMessagesMQService
    {
        Task<IList<PublishMessageBody>> CreateMQFollowUpMessagesAsync(string halId, CancellationToken ct = default);
        Task<IList<PublishMessageBody>> CreateMQFollowUpMessagesAsync(string halId, IList<Campaign> campaigns, CancellationToken ct = default);
    }
}
