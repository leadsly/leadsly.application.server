using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.ScanProspectsForRepliesHandlers
{
    public class ScanProspectsForRepliesCommand : ICommand
    {
        public ScanProspectsForRepliesCommand(string halId, string userId)
        {
            HalId = halId;
            UserId = userId;
        }

        public string HalId { get; set; }
        public string UserId { get; set; }
    }
}
