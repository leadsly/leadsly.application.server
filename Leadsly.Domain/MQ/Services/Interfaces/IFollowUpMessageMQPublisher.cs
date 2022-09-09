using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.MQ.Messages;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Services.Interfaces
{
    public interface IFollowUpMessageMQPublisher
    {
        public Task PublishPhaseAsync(FollowUpMessageBody messageBody, string queueNameIn, string routingKeyIn, string halId);
    }
}
