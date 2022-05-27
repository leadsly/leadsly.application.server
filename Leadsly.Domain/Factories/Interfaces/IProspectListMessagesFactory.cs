using Leadsly.Application.Model.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Factories.Interfaces
{
    public interface IProspectListMessagesFactory
    {
        Task<ProspectListBody> CreateMessageAsync(string prospectListPhaseId, string userId, CancellationToken ct = default);
    }
}
