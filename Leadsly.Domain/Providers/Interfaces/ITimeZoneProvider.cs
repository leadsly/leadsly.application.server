using Leadsly.Application.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Providers.Interfaces
{
    public interface ITimeZoneProvider
    {
        Task AddHalTimezoneAsync(HalUnit halUnit, CancellationToken ct = default);
    }
}
