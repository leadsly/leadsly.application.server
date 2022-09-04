using Leadsly.Application.Model.Campaigns;
using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class CreateScanProspectsForRepliesMessageService : ICreateScanProspectsForRepliesMessageService
    {
        public CreateScanProspectsForRepliesMessageService(
            ILogger<CreateScanProspectsForRepliesMessageService> logger,
            IVirtualAssistantRepository virtualAssistantRepository,
            IScanProspectsForRepliesMessagesFactory factory)
        {
            _logger = logger;
            _virtualAssistantRepository = virtualAssistantRepository;
            _factory = factory;
        }

        private readonly IVirtualAssistantRepository _virtualAssistantRepository;
        private readonly IScanProspectsForRepliesMessagesFactory _factory;
        private readonly ILogger<CreateScanProspectsForRepliesMessageService> _logger;

        public async Task<PublishMessageBody> CreateMQMessageAsync(string userId, string halId, ScanProspectsForRepliesPhase phase, CancellationToken ct = default)
        {
            _logger.LogDebug("Hal unit found by halId {halId}", halId);

            VirtualAssistant virtualAssistant = await _virtualAssistantRepository.GetByHalIdAsync(halId, ct);
            if (virtualAssistant == null)
            {
                _logger.LogDebug("Failed to locate virtual assistant for halId {halId}", halId);
                return null;
            }

            return await _factory.CreateMQMessageAsync(userId, halId, virtualAssistant, phase, ct);
        }
    }
}
