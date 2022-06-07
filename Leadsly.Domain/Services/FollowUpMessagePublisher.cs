using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class FollowUpMessagePublisher : IFollowUpMessagePublisher
    {
        public FollowUpMessagePublisher(ILogger<IFollowUpMessagePublisher> logger, IFollowUpMessageJobsRepository followUpMessageJobsRepository, IMessageBrokerOutlet messageBrokerOutlet)
        {
            _followUpMessageJobsRepository = followUpMessageJobsRepository;
            _logger = logger;
            _messageBrokerOutlet = messageBrokerOutlet;
        }

        private readonly IMessageBrokerOutlet _messageBrokerOutlet;
        private readonly IFollowUpMessageJobsRepository _followUpMessageJobsRepository;
        private readonly ILogger<IFollowUpMessagePublisher> _logger;

        public async Task PublishPhaseAsync(FollowUpMessageBody messageBody, string queueNameIn, string routingKeyIn, string halId)
        {
            _messageBrokerOutlet.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, null);

            FollowUpMessageJob followUpMessageJob = await _followUpMessageJobsRepository.GetByFollowUpmessageIdAsync(messageBody.FollowUpMessageId);
            if(followUpMessageJob != null)
            {
                // remove this job from the table now
                await _followUpMessageJobsRepository.DeleteFollowUpMessageJobAsync(followUpMessageJob.FollowUpMessageId);
            }
        }
    }
}
