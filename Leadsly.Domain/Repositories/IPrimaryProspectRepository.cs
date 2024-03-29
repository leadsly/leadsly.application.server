﻿using Leadsly.Domain.Models.Entities.Campaigns;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IPrimaryProspectRepository
    {
        Task<PrimaryProspectList> GetListByNameAndUserIdAsync(string prospectListName, string userId, CancellationToken ct = default);
        Task<PrimaryProspectList> CreateListAsync(PrimaryProspectList primaryProspectList, CancellationToken ct = default);
        Task<PrimaryProspectList> GetListByIdAsync(string primaryProspectListId, CancellationToken ct = default);
        Task<IList<PrimaryProspectList>> GetListsByUserIdAsync(string userId, CancellationToken ct = default);
        Task<PrimaryProspect> GetByIdAsync(string primaryProspectId, CancellationToken ct = default);
        Task<IList<PrimaryProspect>> CreateAllAsync(IList<PrimaryProspect> primaryProspectList, CancellationToken ct = default);
    }
}
