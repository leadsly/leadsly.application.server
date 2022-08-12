using Leadsly.Application.Model.Aws.ElasticContainerService;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
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

            _logger.LogDebug("Executing RestartResourceCommand for Hal Id {halId}", halId);

            VirtualAssistant virtualAssistant = await _virtualAssistantRepository.GetByHalIdAsync(halId);

            if (virtualAssistant != null)
            {
                if (virtualAssistant.EcsServices != null && virtualAssistant.EcsServices.Count > 0)
                {
                    EcsService halEcsService = virtualAssistant.EcsServices.FirstOrDefault(x => x.Purpose == Purpose.Hal);

                    ICollection<EcsTask> ecsTasks = halEcsService.EcsTasks;

                    foreach (EcsTask task in ecsTasks)
                    {
                        StopEcsTaskRequest request = new()
                        {
                            Cluster = halEcsService.ClusterArn,
                            Reason = "Routine restart",
                            Task = task.TaskArn
                        };

                        string ecsServiceName = halEcsService.ServiceName;
                        _logger.LogDebug("Sending request to stop all tasks running under Hal's ECS service {ecsServiceName}", ecsServiceName);

                        await _awsEcsService.StopTaskAsync(request);
                    }
                }
            }
        }
    }
}
