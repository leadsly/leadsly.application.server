using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public class RestartResourcesCommandHandler : ICommandHandler<RestartResourcesCommand>
    {
        public RestartResourcesCommandHandler(IHalRepository halRepository, ICloudPlatformRepository cloudPlatformRepository, ILogger<RestartResourcesCommandHandler> logger)
        {
            _halRepository = halRepository;
            _cloudPlatformRepository = cloudPlatformRepository;
            _logger = logger;
        }

        private readonly IHalRepository _halRepository;
        private readonly ICloudPlatformRepository _cloudPlatformRepository;
        private readonly ILogger<RestartResourcesCommandHandler> _logger;

        public async Task HandleAsync(RestartResourcesCommand command)
        {
            string halId = command.HalId;

            HalUnit halUnit = await _halRepository.GetByHalIdAsync(halId);


        }
    }
}
