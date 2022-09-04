using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers
{
    public class CreateScanProspectsForRepliesMessageProvider : ICreateScanProspectsForRepliesMessageProvider
    {
        public CreateScanProspectsForRepliesMessageProvider(
            ILogger<CreateScanProspectsForRepliesMessageProvider> logger,
            ICreateScanProspectsForRepliesMessageService service,
            IUserRepository userRepository,
            ICampaignRepositoryFacade campaignRepositoryFacade)
        {
            _userRepository = userRepository;
            _logger = logger;
            _service = service;
            _campaignRepositoryFacade = campaignRepositoryFacade;
        }

        private readonly IUserRepository _userRepository;
        private readonly ICreateScanProspectsForRepliesMessageService _service;
        private readonly ILogger<CreateScanProspectsForRepliesMessageProvider> _logger;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        public async Task<PublishMessageBody> CreateMQScanProspectsForRepliesMessageAsync(string userId, string halId, CancellationToken ct = default)
        {
            if (await _campaignRepositoryFacade.AnyActiveCampaignsByHalIdAsync(halId, ct) == false)
            {
                _logger.LogDebug("HalId {halId} does not contain any active campaigns", halId);
                return null;
            }

            SocialAccount socialAccount = await _userRepository.GetSocialAccountByHalIdAsync(halId, ct);
            if (socialAccount == null)
            {
                _logger.LogDebug("Unable to locate SocialAccount associated with halId {halId}", halId);
                return null;
            }

            string email = socialAccount.Username;
            _logger.LogDebug("Social account {email} is associated with HalId {halId}", email, halId);

            ScanProspectsForRepliesPhase phase = await _campaignRepositoryFacade.GetScanProspectsForRepliesPhaseByIdAsync(socialAccount.ScanProspectsForRepliesPhase.ScanProspectsForRepliesPhaseId, ct);
            if (phase == null)
            {
                _logger.LogDebug("ScanProspectsForRepliesPhase does not exist on social account {email}", email);
                return null;
            }

            return await _service.CreateMQMessageAsync(userId, halId, phase, ct);
        }
    }
}
