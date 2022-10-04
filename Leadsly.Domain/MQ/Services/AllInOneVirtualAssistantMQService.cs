using Leadsly.Domain.Decorators;
using Leadsly.Domain.Models;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.MQ.Messages;
using Leadsly.Domain.MQ.Services.Interfaces;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;
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
            IWebHostEnvironment env,
            IProvisionResourcesService service)
        {
            _env = env;
            _service = service;
            _logger = logger;
            _createMQService = createMQService;
            _virtualAssistantRepository = virtualAssistantRepository;
        }

        private readonly IWebHostEnvironment _env;
        private readonly IProvisionResourcesService _service;
        private readonly ILogger<AllInOneVirtualAssistantMQService> _logger;
        private readonly IAllInOneVirtualAssistantCreateMQService _createMQService;
        private readonly VirtualAssistantRepositoryCache _virtualAssistantRepository;

        public async Task<bool> ProvisionResourcesAsync(string halId, string userId, CancellationToken ct = default)
        {
            if (_env.IsDevelopment() == false)
            {
                // save it to the database
                VirtualAssistant virtualAssistant = await _virtualAssistantRepository.GetByHalIdAsync(halId, ct);
                if (virtualAssistant == null)
                {
                    _logger.LogError("Virtual assistant is not found by HalId {0} for UserId {1}", halId, userId);
                    return false;
                }

                // 1. create new hal ecs service
                EcsService halEcsService = virtualAssistant.EcsServices.FirstOrDefault(s => s.Purpose == EcsResourcePurpose.Hal);
                if (halEcsService == null)
                {
                    _logger.LogError("Expected to find previous Hal ecs service but none was found off of virtual assistant");
                    return false;
                }

                if (await _service.CreateAwsEcsServiceAsync(userId, halEcsService, ct) == false)
                {
                    _logger.LogError("Failed to create Hal ecs service in aws");
                    return false;
                }

                // 2. create new grid ecs service
                EcsService gridEcsService = virtualAssistant.EcsServices.FirstOrDefault(s => s.Purpose == EcsResourcePurpose.Grid);
                if (gridEcsService == null)
                {
                    _logger.LogError("Expected to find previous Grid ecs service but none was found off of virtual assistant");
                    return false;
                }

                if (await _service.CreateAwsEcsServiceAsync(userId, gridEcsService, ct) == false)
                {
                    _logger.LogError("Failed to create Grid ecs service in aws");
                    return false;
                }

                // virtualAssistant.EcsServices = _service.EcsServices;
                virtualAssistant.Provisioned = true;
                virtualAssistant = await _virtualAssistantRepository.UpdateAsync(virtualAssistant, ct);
                if (virtualAssistant == null)
                {
                    _logger.LogError("Failed to successfully update virtual assistant's cloud resources.");
                    // here we need to roll back all of the resources
                    await _service.RollbackAllResourcesAsync(userId, ct);
                    return false;
                }
            }

            return true;
        }

        public async Task<PublishMessageBody> CreateMQAllInOneVirtualAssistantMessageAsync(string halId, bool initial, CancellationToken ct = default)
        {
            PublishMessageBody mqMessage = await _createMQService.CreateMQMessageAsync(halId, ct);
            if (mqMessage == null)
            {
                return mqMessage;
            }

            return mqMessage;
        }

    }
}
