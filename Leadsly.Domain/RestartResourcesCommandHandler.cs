using Leadsly.Application.Model.Aws.ElasticContainerService;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public class RestartResourcesCommandHandler : ICommandHandler<RestartResourcesCommand>
    {
        public RestartResourcesCommandHandler(IAwsElasticContainerService awsEcsService, IVirtualAssistantRepository virtualAssistantRepository, ILogger<RestartResourcesCommandHandler> logger)
        {
            _virtualAssistantRepository = virtualAssistantRepository;
            _logger = logger;
            _awsEcsService = awsEcsService;
        }

        private readonly IAwsElasticContainerService _awsEcsService;
        private readonly IVirtualAssistantRepository _virtualAssistantRepository;
        private readonly ILogger<RestartResourcesCommandHandler> _logger;

        public async Task HandleAsync(RestartResourcesCommand command)
        {
            string halId = command.HalId;

            VirtualAssistant virtualAssistant = await _virtualAssistantRepository.GetByHalIdAsync(halId);

            if (virtualAssistant != null)
            {
                if (virtualAssistant.EcsService != null)
                {
                    ICollection<EcsTask> ecsTasks = virtualAssistant.EcsService.EcsTasks;

                    foreach (EcsTask task in ecsTasks)
                    {
                        StopEcsTaskRequest request = new()
                        {
                            Cluster = virtualAssistant.EcsService.ClusterArn,
                            Reason = "Routine restart",
                            Task = task.TaskArn
                        };

                        await _awsEcsService.StopTaskAsync(request);
                    }
                }
            }
        }
    }
}
