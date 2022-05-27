using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain
{
    /// <summary>
    /// Determines which phase will be executed
    /// </summary>
    public interface IPhaseManager
    {
        /// <summary>
        /// Determines whether DeepScanProspectsForRepliesPhase will be executed or FollowUpMessagePhase and SendProspectsForReplies
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task ProspectingPhaseAsync(CancellationToken ct = default);

        /// <summary>
        /// Determines whether ProspectListPhase will be executed or SendConnectionsPhase
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task NetworkingConnectionsPhaseAsync(CancellationToken ct = default);


        Task NetworkingPhaseAsync(CancellationToken ct = default);
    }
}
