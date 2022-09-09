using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.MQ.Messages;

namespace Leadsly.Domain.MQ.Creators
{
    public abstract class MQCreatorBase
    {
        protected abstract void PublishMessage(PublishMessageBody message);
    }
}
