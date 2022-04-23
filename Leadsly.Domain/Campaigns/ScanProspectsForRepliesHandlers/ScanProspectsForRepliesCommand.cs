using System.Collections.Generic;

namespace Leadsly.Domain.Campaigns.ScanProspectsForRepliesHandlers
{
    public class ScanProspectsForRepliesCommand : ICommand
    {
        public ScanProspectsForRepliesCommand(string halId)
        {
            HalId = halId;
        }

        public ScanProspectsForRepliesCommand(IList<string> halIds)
        {
            HalIds = halIds;
        }

        public IList<string> HalIds { get; set; }
        public string HalId { get; set; }
    }
}
