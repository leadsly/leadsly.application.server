using Leadsly.Application.Model.Campaigns;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface IFollowUpMessagesService
    {
        Task<IList<PublishMessageBody>> CreateMQFollowUpMessagesAsync(string halId, CancellationToken ct = default);
    }
}
