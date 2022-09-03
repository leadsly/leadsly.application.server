using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class FollowUpMessagePublisherService : IFollowUpMessagePublisherService
    {
        public FollowUpMessagePublisherService(
            IMessageBrokerOutlet messageBrokerOutlet,
            IHangfireService hangfireService,
            IFollowUpMessageJobsRepository jobsRepository,
            ILogger<FollowUpMessagePublisherService> logger)
        {
            _logger = logger;
            _jobsRepository = jobsRepository;
            _messageBrokerOutlet = messageBrokerOutlet;
            _hangfireService = hangfireService;
        }

        private readonly IMessageBrokerOutlet _messageBrokerOutlet;
        private readonly IHangfireService _hangfireService;
        private readonly ILogger<FollowUpMessagePublisherService> _logger;
        private readonly IFollowUpMessageJobsRepository _jobsRepository;

        public void PublishMessage(PublishMessageBody message, string queueNameIn, string routingKeyIn, string halId)
        {
            _messageBrokerOutlet.PublishPhase(message, queueNameIn, routingKeyIn, halId, null);
        }

        public async Task ScheduleMessageAsync(PublishMessageBody message, string queueNameIn, string routingKeyIn, string halId, CancellationToken ct = default)
        {
            FollowUpMessageBody followUpMessage = message as FollowUpMessageBody;
            _logger.LogDebug("Scheduling FollowUpMessage to be published at {0}", followUpMessage.ExpectedDeliveryDateTime);
            string jobId = _hangfireService.Schedule<IFollowUpMessagePublisher>(x => x.PublishPhaseAsync(followUpMessage, queueNameIn, routingKeyIn, halId), followUpMessage.ExpectedDeliveryDateTime);
            _logger.LogDebug($"Scheduled hangfire job id is {jobId}");

            FollowUpMessageJob followUpJob = new()
            {
                CampaignProspectId = followUpMessage.CampaignProspectId,
                HangfireJobId = jobId,
                FollowUpMessageId = followUpMessage.FollowUpMessageId
            };

            await _jobsRepository.AddFollowUpJobAsync(followUpJob, ct);
        }
    }
}
