using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
