using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
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
        public Task<HalUnit> CreateAsync(HalUnit halDetails, CancellationToken ct = default);
        public Task<HalUnit> GetBySocialAccountUsernameAsync(string connectedAccountUsername, CancellationToken ct = default);

        /// <summary>
        /// Creates ChromeProfile name for the given campaign phase. This is the name that is used by hal to create a copy of chrome profile name
        /// on the physical machine.
        /// </summary>
        /// <param name="chromeProfileName"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public Task<ChromeProfile> CreateChromeProfileAsync(ChromeProfile chromeProfile, CancellationToken ct = default);

        public Task<ChromeProfile> GetChromeProfileAsync(PhaseType campaignType, CancellationToken ct = default);

        public Task<HalUnit> GetByHalIdAsync(string halId, CancellationToken ct = default);

        public Task<IList<string>> GetAllHalIdsAsync(CancellationToken ct = default);
    }
}
