using Leadsly.Application.Model.Entities.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IProspectListRepository
    {
        Task<PrimaryProspectList> GetPrimaryProspectListByNameAndUserIdAsync(string prospectListName, string userId, CancellationToken ct = default);
        Task<PrimaryProspectList> CreatePrimaryProspectListAsync(PrimaryProspectList primaryProspectList, CancellationToken ct = default);
    }
}
