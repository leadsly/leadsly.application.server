using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.FollowUpMessagesHandler.FollowUpMessages
{
    public class FollowUpMessagesCommand : ICommand
    {
        public FollowUpMessagesCommand(string halId)
        {
            HalId = halId;
        }

        public string HalId { get; set; }
    }
}
