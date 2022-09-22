using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.MQ.Messages;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories.Interfaces
{
    public interface IAllInOneVirtualAssistantFactory
    {
        Task<PublishMessageBody> CreateMQMessageAsync(string halId, VirtualAssistant virtualAssistant);
    }
}
