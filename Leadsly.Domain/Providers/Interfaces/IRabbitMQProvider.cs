using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers.Interfaces
{
    public interface IRabbitMQProvider
    {
        CloudPlatformConfiguration GetCloudPlatformConfiguration();
        Task<string> CreateNewChromeProfileAsync(PhaseType phaseType, CancellationToken ct = default);
    }
}
