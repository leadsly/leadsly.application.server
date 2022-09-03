using Leadsly.Application.Model.Campaigns;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface IFollowUpMessagePublisherService
    {
        void PublishMessage(PublishMessageBody message, string queueNameIn, string routingKeyIn, string halId);
        Task ScheduleMessageAsync(PublishMessageBody message, string queueNameIn, string routingKeyIn, string halId, CancellationToken ct = default);
    }
}
