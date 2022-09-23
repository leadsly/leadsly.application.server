using Leadsly.Domain.Decorators;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.MQ.Messages;
using Leadsly.Domain.MQ.Services.Interfaces;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.MQ.Services
{
    public class AllInOneVirtualAssistantMQService : IAllInOneVirtualAssistantMQService
    {
        public AllInOneVirtualAssistantMQService(
            ILogger<AllInOneVirtualAssistantMQService> logger,
            IAllInOneVirtualAssistantCreateMQService createMQService,
            VirtualAssistantRepositoryCache virtualAssistantRepository,
            IProvisionResourcesService service)
        {
            _service = service;
            _logger = logger;
            _createMQService = createMQService;
            _virtualAssistantRepository = virtualAssistantRepository;
        }

        private readonly IProvisionResourcesService _service;
        private readonly ILogger<AllInOneVirtualAssistantMQService> _logger;
        private readonly IAllInOneVirtualAssistantCreateMQService _createMQService;
        private readonly VirtualAssistantRepositoryCache _virtualAssistantRepository;

        public async Task<bool> ProvisionResourcesAsync(string halId, string userId, CancellationToken ct = default)
        {
            // save it to the database
            VirtualAssistant virtualAssistant = await _virtualAssistantRepository.GetByHalIdAsync(halId, ct);
            if (virtualAssistant == null)
            {
                _logger.LogError("Virtual assistant is not found by HalId {0} for UserId {1}", halId, userId);
                return false;
            }

            if (await _service.CreateAwsResourcesAsync(halId, userId, ct) == false)
            {
                _logger.LogError("Failed to provision aws resources for HalId {0} and UserId {1}", halId, userId);
                return false;
            }

            virtualAssistant.CloudMapDiscoveryServices = _service.CloudMapDiscoveryServices;
            virtualAssistant.EcsServices = _service.EcsServices;
            virtualAssistant = await _virtualAssistantRepository.UpdateAsync(virtualAssistant, ct);
            if (virtualAssistant == null)
            {
                _logger.LogError("Failed to successfully update virtual assistant's cloud resources.");
                // here we need to roll back all of the resources
                await _service.RollbackAllResourcesAsync(userId, ct);
                return false;
            }

            return true;

        }

        public async Task<AllInOneVirtualAssistantMessageBody> CreateMQAllInOneVirtualAssistantMessageAsync(string halId, bool initial, CancellationToken ct = default)
        {
            AllInOneVirtualAssistantMessageBody mqMessage = await _createMQService.CreateMQMessageAsync(halId, ct) as AllInOneVirtualAssistantMessageBody;
            if (mqMessage == null)
            {
                return mqMessage;
            }

            await _createMQService.SetDeepScanProspectsForRepliesProperties(halId, initial, mqMessage, ct);

            await _createMQService.SetCheckOffHoursNewConnectionsProperties(halId, initial, mqMessage, ct);

            return mqMessage;
        }

    }
}
