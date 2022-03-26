using Leadsly.Application.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IHalRepository
    {
        public Task<HalUnit> AddHalDetailsAsync(HalUnit halDetails, CancellationToken ct = default);
        public Task<HalUnit> GetHalDetailsByConnectedAccountUsernameAsync(string connectedAccountUsername, CancellationToken ct = default);
    }
}
