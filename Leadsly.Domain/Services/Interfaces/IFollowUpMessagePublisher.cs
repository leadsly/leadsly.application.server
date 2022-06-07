using Leadsly.Application.Model.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface IFollowUpMessagePublisher
    {
        public Task PublishPhaseAsync(FollowUpMessageBody messageBody, string queueNameIn, string routingKeyIn, string halId);
    }
}
