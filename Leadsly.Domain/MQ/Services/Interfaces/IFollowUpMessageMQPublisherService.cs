using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.MQ.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Services.Interfaces
{
    public interface IFollowUpMessageMQPublisherService
    {
        void PublishMessage(PublishMessageBody message, string queueNameIn, string routingKeyIn, string halId);
        Task ScheduleMessageAsync(PublishMessageBody message, string queueNameIn, string routingKeyIn, string halId, CancellationToken ct = default);
    }
}
