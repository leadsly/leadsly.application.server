using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.MQ.Messages;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories
{
    public class AllInOneVirtualAssistantFactory : IAllInOneVirtualAssistantFactory
    {
        public AllInOneVirtualAssistantFactory(
            ILogger<AllInOneVirtualAssistantFactory> logger,
            IHalRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        private readonly ILogger<AllInOneVirtualAssistantFactory> _logger;
        private readonly IHalRepository _repository;

        public Task<PublishMessageBody> CreateMQMessageAsync(string halId, VirtualAssistant virtualAssistant)
        {
            throw new System.NotImplementedException();
        }
    }
}
