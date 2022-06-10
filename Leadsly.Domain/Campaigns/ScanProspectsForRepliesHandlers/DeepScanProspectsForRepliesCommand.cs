using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.ScanProspectsForRepliesHandlers
{
    public class DeepScanProspectsForRepliesCommand : ICommand
    {
        public DeepScanProspectsForRepliesCommand(IList<string> halIds)
        {
            HalIds = halIds;
        }

        public DeepScanProspectsForRepliesCommand(string halId)
        {
            HalId = halId;
        }

        public IList<string> HalIds { get; set; }

        public string HalId { get; set; }
    }
}
