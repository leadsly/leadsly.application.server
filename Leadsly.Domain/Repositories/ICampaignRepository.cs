using Leadsly.Application.Model.Entities.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface ICampaignRepository
    {
        Task<List<Campaign>> GetAllActiveAsync(CancellationToken ct = default);
    }
}
