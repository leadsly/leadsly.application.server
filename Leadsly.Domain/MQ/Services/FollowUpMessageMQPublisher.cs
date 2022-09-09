using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.MQ.Messages;
using Leadsly.Domain.MQ.Services.Interfaces;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Services
{
    public class FollowUpMessageMQPublisher : IFollowUpMessageMQPublisher
    {
        public FollowUpMessageMQPublisher(ILogger<FollowUpMessageMQPublisher> logger, IFollowUpMessageJobsRepository followUpMessageJobsRepository, IMessageBrokerOutlet messageBrokerOutlet)
        {
            _followUpMessageJobsRepository = followUpMessageJobsRepository;
            _logger = logger;
            _messageBrokerOutlet = messageBrokerOutlet;
        }

        private readonly IMessageBrokerOutlet _messageBrokerOutlet;
        private readonly IFollowUpMessageJobsRepository _followUpMessageJobsRepository;
        private readonly ILogger<FollowUpMessageMQPublisher> _logger;

        public async Task PublishPhaseAsync(FollowUpMessageBody messageBody, string queueNameIn, string routingKeyIn, string halId)
        {
            _messageBrokerOutlet.PublishPhase(messageBody, queueNameIn, routingKeyIn, halId, null);

            FollowUpMessageJob followUpMessageJob = await _followUpMessageJobsRepository.GetByFollowUpmessageIdAsync(messageBody.FollowUpMessageId);
            if (followUpMessageJob != null)
            {
                // remove this job from the table now
                await _followUpMessageJobsRepository.DeleteFollowUpMessageJobAsync(followUpMessageJob.FollowUpMessageId);
            }
        }
    }
}
