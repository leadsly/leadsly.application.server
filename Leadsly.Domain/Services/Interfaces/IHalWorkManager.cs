using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface IHalWorkManager
    {
        /// <summary>
        /// Checks if right now falls between hal's start and end of the work day
        /// </summary>
        /// <param name="halId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<bool> IsNowWithinWorkHoursAsync(string halId, CancellationToken ct = default);
    }
}
